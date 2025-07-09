using Microsoft.Extensions.Logging;
using System;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using ThreeByte.LinkLib.ProjectorLink.Commands;
using ThreeByte.LinkLib.Shared.Logging;

namespace ThreeByte.LinkLib.ProjectorLink
{
    public class Projector : IDisposable
    {
        public event EventHandler<Exception>? ErrorOccurred;
        public string Address => $"{_hostName}/{_port}";

        private const int DefaultPort = 4352;
        private readonly ILogger _logger;

        /// <summary>
        /// host name or IP (as string, e.g. "192.168.1.12") of the projector
        /// </summary>
        private readonly string _hostName;
        /// <summary>
        /// PJLink port, default ist 4352 (see Spec)
        /// </summary>
        private readonly int _port;
        /// <summary>
        /// Flag is true, if the projector requires authentication
        /// </summary>
        private bool _useAuth = false;
        /// <summary>
        /// Password supplied by user if authentication is enabled
        /// </summary>
        private readonly string _password = "";
        /// <summary>
        /// Random key returned by projector for MD5 sum generation 
        /// </summary>
        private string _projectorKey = "";
        /// <summary>
        /// The connection client
        /// </summary>
        TcpClient? _tcpClient = null;
        /// <summary>
        /// The Network stream the _client provides
        /// </summary>
        NetworkStream?_stream = null;

        private bool _isDisposed;

        public Projector(string host, int port, string password)
        {
            _hostName = host;
            _port = port;
            _password = password;
            _useAuth = password != "";
            _logger = LogFactory.Create<Projector>();
        }

        public Projector(string host, string password)
            : this(host, DefaultPort, password)
        {
        }

        public Projector(string host, int port)
            : this(host, port, string.Empty)
        {
        }

        public Projector(string host)
            : this(host, DefaultPort, string.Empty)
        {
        }

        /// <summary>
        ///     Cancels the thread and releases resources.
        ///     Clients of this class are responsible for calling it.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _logger.LogInformation("Cleaning up network resources.");

            CloseConnection();
        }

        /// <summary>
        /// Turn on projector. Returns true if projector answered with SUCCESS
        /// </summary>
        public bool TurnOn()
        {
            PowerCommand pc = new PowerCommand(PowerCommand.Power.ON);
            return SendCommand(pc) == CommandResponse.SUCCESS;
        }

        /// <summary>
        /// Turn off projector. Returns true if projector answered with SUCCESS
        /// </summary>
        public bool TurnOff()
        {
            PowerCommand pc = new PowerCommand(PowerCommand.Power.OFF);
            return SendCommand(pc) == CommandResponse.SUCCESS;
        }

        /// <summary>
        /// Check power state of Projector. Returns unknown in case of an error
        /// </summary>
        public PowerStatus GetState()
        {
            PowerCommand pc3 = new PowerCommand(PowerCommand.Power.QUERY);
            if (SendCommand(pc3) == CommandResponse.SUCCESS)
            {
                return pc3.Status;
            }
            return PowerStatus.UNKNOWN;
        }

        /// <summary>
        /// Return String in the form
        /// Manufacturer Product (ProjectorName)
        /// or 
        /// Manufacturer Product 
        /// if no projector name is set. 
        /// </summary>
        public string GetInfo()
        {
            string toRet = "";
            ManufacturerNameCommand mnc = new ManufacturerNameCommand();
            if (SendCommand(mnc) == CommandResponse.SUCCESS)
            {
                toRet = mnc.Manufacturer;
            }

            ProductNameCommand prnc = new ProductNameCommand();
            if (SendCommand(prnc) == CommandResponse.SUCCESS)
            {
                toRet += " " + prnc.ProductName;
            }

            ProjectorNameCommand pnc = new ProjectorNameCommand();
            if (SendCommand(pnc) == CommandResponse.SUCCESS && pnc.Name.Length > 0)
            {
                toRet += " (" + pnc.Name + ")";
            }
            return toRet;
        }

        private CommandResponse SendCommand(Command cmd)
        {
            if (InitConnection())
            {
                try
                {
                    string cmdString = cmd.GetCommandString() + "\r";

                    if (_useAuth && _projectorKey != "")
                    {
                        cmdString = GetMD5Hash(_projectorKey + _password) + cmdString;
                    }

                    byte[] sendBytes = Encoding.ASCII.GetBytes(cmdString);
                    _stream!.Write(sendBytes, 0, sendBytes.Length);

                    byte[] recvBytes = new byte[_tcpClient!.ReceiveBufferSize];
                    int bytesRcvd = _stream.Read(recvBytes, 0, _tcpClient!.ReceiveBufferSize);
                    string returndata = Encoding.ASCII.GetString(recvBytes, 0, bytesRcvd);
                    returndata = returndata.Trim();
                    _logger.LogDebug("Received data: {d}", returndata);

                    cmd.ProcessAnswerString(returndata);
                    return cmd.CmdResponse;
                }
                catch (Exception ex)
                {
                    HandleError(ex, $"Error during sending the command {cmd.GetType().Name}");
                }
                finally
                {
                    CloseConnection();
                }
            }

            return CommandResponse.COMMUNICATION_ERROR;
        }

        private bool InitConnection()
        {
            _logger.LogDebug("Open connection to projector. {addr}/{port}", _hostName, _port);

            if (_tcpClient != null && _tcpClient.Connected)
            {
                return true;
            }

            try
            {
                _tcpClient = new TcpClient(_hostName, _port);
                _stream = _tcpClient.GetStream();
                byte[] recvBytes = new byte[_tcpClient.ReceiveBufferSize];
                int bytesRcvd = _stream.Read(recvBytes, 0, _tcpClient.ReceiveBufferSize);
                string retVal = Encoding.ASCII.GetString(recvBytes, 0, bytesRcvd);
                retVal = retVal.Trim();

                if (retVal.IndexOf("PJLINK 0") >= 0)
                {
                    _useAuth = false;  //pw provided but projector doesn't need it.
                    return true;
                }
                else if (retVal.IndexOf("PJLINK 1 ") >= 0)
                {
                    _projectorKey = retVal.Replace("PJLINK 1 ", "");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                HandleError(ex, "Connection Error.");
                return false;
            }
        }

        private void CloseConnection()
        {
            _logger.LogDebug("Close connection to projector.");

            _tcpClient?.Close();
            _stream?.Close();
        }

        private string GetMD5Hash(string input)
        {
            MD5CryptoServiceProvider cryptoProvider = new MD5CryptoServiceProvider();
            byte[] bs = Encoding.ASCII.GetBytes(input);
            byte[] hash = cryptoProvider.ComputeHash(bs);

            string toRet = "";
            foreach (byte b in hash)
            {
                toRet += b.ToString("x2");
            }
            return toRet;
        }

        private void HandleError(Exception ex, string message)
        {
            _logger.LogError(ex, message);
            ErrorOccurred?.Invoke(this, ex);
        }
    }
}
