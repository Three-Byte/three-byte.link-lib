using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Logging;
using ThreeByte.LinkLib.Shared.Logging;

namespace ThreeByte.LinkLib.TcpLink
{
    public class AsyncTcpLink : IDisposable
    {
        private const int BufferSize = 8092;
        private const int MaxDataSize = 100;
        private readonly ILogger _logger;

        private readonly TcpLinkSettings _settings;
        private readonly object _clientLock = new object();
        private IAsyncResult? _connectResult;
        private readonly List<byte[]> _incomingData = new List<byte[]>();
        private bool _isDisposed;

        private NetworkStream? _networkStream;
        private IAsyncResult? _readResult;

        private TcpClient? _tcpClient;
        private IAsyncResult? _writeResult;

        public AsyncTcpLink(string address, int port)
            : this(address, port, true)
        {
        }

        public AsyncTcpLink(string address, int port, bool enabled = true)
        {
            _settings = new TcpLinkSettings(address, port);
            IsEnabled = enabled;
            _logger = LogFactory.Create<AsyncTcpLink>();

            if (enabled)
            {
                SafeConnect();
            }
        }

        public bool IsConnected { get; private set; }

        public bool IsEnabled { get; private set; } = true;

        public string Address => _settings.Address;
        public int Port => _settings.Port;
        public bool HasData => _incomingData.Count > 0;

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

            SafeClose();
        }

        public event EventHandler<bool>? IsConnectedChanged;
        public event EventHandler<bool>? IsEnabledChanged;
        public event EventHandler<Exception>? ErrorOccurred;
        public event EventHandler? DataReceived;

        /// <summary>
        ///     Sets a value indicating whether messages should be propagated to the network or not
        /// </summary>
        /// <param name="value"></param>
        public void SetEnabled(bool value)
        {
            IsEnabled = value;

            if (!IsEnabled)
            {
                SafeClose();
            }
            else if (!IsConnected)
            {
                SafeConnect();
            }

            IsEnabledChanged?.Invoke(this, value);
        }

        /// <summary>
        ///     Asynchronously sends the TCP message, waiting until the connection is reestablihsed if necessary
        /// </summary>
        /// <param name="message">binary message to be sent</param>
        public void SendMessage(byte[] message)
        {
            if (!IsEnabled)
            {
                return;
            }

            lock (_clientLock)
            {
                if (_networkStream != null)
                {
                    try
                    {
                        _writeResult = _networkStream.BeginWrite(message, 0, message.Length, WriteCallback, null);
                    }
                    catch (Exception ex)
                    {
                        HandleError(ex, "Cannot Write");
                        ChangeIsConnected(false);

                        SafeClose();
                        SafeConnect();
                    }
                }
                else
                {
                    ChangeIsConnected(false);
                    SafeConnect();
                }
            }
        }

        /// <summary>
        ///     Very carefully checks and shuts down the tcpClient and sets it to null
        /// </summary>
        private void SafeClose()
        {
            _logger.LogDebug("Safe Close.");

            lock (_clientLock)
            {
                //Resolve outstanding connections
                if (_connectResult != null)
                {
                    //End the connection process
                    _connectResult = null;
                }

                if (_readResult != null)
                {
                    //End the read process
                    _readResult = null;
                }

                if (_writeResult != null)
                {
                    //End the write process
                    _writeResult = null;
                }

                _networkStream = null;
                _tcpClient?.Client?.Close();
                _tcpClient?.Close();
                _tcpClient = null;

                lock (_incomingData)
                {
                    _incomingData.Clear();
                }
            }

            ChangeIsConnected(false);
        }

        private void SafeConnect(object state)
        {
            SafeConnect();
        }

        /// <summary>
        ///     Carefully check to see if the link is connected or can be reestablished
        /// </summary>
        private void SafeConnect()
        {
            _logger.LogDebug("Safe Connect.");

            if (_isDisposed)
            {
                return;
            }

            lock (_clientLock)
            {
                if (_connectResult != null)
                {
                    return;
                }

                if (_tcpClient == null || !_tcpClient.Connected)
                {
                    SafeClose();
                    _tcpClient = new TcpClient();
                }

                if (!_tcpClient.Connected)
                {
                    _logger.LogInformation("Connecting: {addr}/{prt}.", _settings.Address, _settings.Port);

                    try
                    {
                        _connectResult = _tcpClient.BeginConnect(_settings.Address, _settings.Port, ConnectCallback, null);
                    }
                    catch (Exception ex)
                    {
                        HandleError(ex, "Connection Error");
                        ChangeIsConnected(false);
                    }
                }
            }
        }

        private void ConnectCallback(IAsyncResult asyncResult)
        {
            _logger.LogInformation("Connect Callback: {addr}/{prt}.", _settings.Address, _settings.Port);

            lock (_clientLock)
            {
                try
                {
                    _networkStream = null;
                    if (_tcpClient != null)
                    {
                        _tcpClient.EndConnect(asyncResult);
                        _networkStream = _tcpClient.GetStream();

                        ChangeIsConnected(_tcpClient.Connected);
                    }
                    else
                    {
                        ChangeIsConnected(false);
                    }

                    if (!IsEnabled)
                    {
                        SafeClose();
                    }
                }
                catch (Exception ex)
                {
                    HandleError(ex, "Connection Error.");
                    ChangeIsConnected(false);
                }

                if (_connectResult == asyncResult)
                {
                    _logger.LogDebug("Clearing Connect Result.");
                    _connectResult = null;

                    if (IsEnabled)
                    {
                        if (!IsConnected)
                        {
                            Timer timer = new Timer(
                                SafeConnect,
                                DateTime.Now,
                                TimeSpan.FromSeconds(3),
                                TimeSpan.FromMilliseconds(-1));
                        }
                        else
                        {
                            ReceiveData();
                        }
                    }
                }
            }
        }

        private void WriteCallback(IAsyncResult asyncResult)
        {
            lock (_clientLock)
            {
                try
                {
                    _networkStream?.EndWrite(asyncResult);
                    ChangeIsConnected(true);
                }
                catch (Exception ex)
                {
                    HandleError(ex, "Error writing to stream.");
                    SafeConnect();
                }

                if (_writeResult == asyncResult)
                {
                    _logger.LogDebug("Clearing Write Result.");
                    _writeResult = null;
                }
            }
        }

        private void ReceiveData()
        {
            if (!IsEnabled)
            {
                return;
            }

            byte[] buf = new byte[BufferSize];
            lock (_clientLock)
            {
                if (_networkStream != null)
                {
                    try
                    {
                        _readResult = _networkStream.BeginRead(buf, 0, buf.Length, ReadCallback, buf);
                        ChangeIsConnected(true);
                    }
                    catch (Exception ex)
                    {
                        HandleError(ex, "Error reading from stream.");
                        ChangeIsConnected(false);
                        SafeConnect();
                    }
                }
            }
        }

        private void ReadCallback(IAsyncResult asyncResult)
        {
            byte[] buffer = (byte[])asyncResult.AsyncState ?? Array.Empty<byte>();
            bool hasNewData = false;
            int bytesRead = 0;

            lock (_clientLock)
            {
                try
                {
                    if (_networkStream != null)
                    {
                        bytesRead = _networkStream.EndRead(asyncResult);
                    }

                    // If the remote host shuts down the Socket connection and all available data has been received,
                    // the EndRead method completes immediately and returns zero bytes.
                    if (bytesRead == 0)
                    {
                        SafeClose();
                        SafeConnect();
                    }

                    if (bytesRead > 0)
                    {
                        lock (_incomingData)
                        {
                            byte[] truncatedBuffer = new byte[bytesRead];
                            Array.Copy(buffer, truncatedBuffer, bytesRead);
                            _incomingData.Add(truncatedBuffer);
                            hasNewData = true;
                            if (_incomingData.Count > MaxDataSize)
                            {
                                // Purge messages from the end of the list to prevent overflow
                                _logger.LogError("Too many incoming messages to handle: {cnt}.", _incomingData.Count);
                                _incomingData.RemoveAt(_incomingData.Count - 1);
                            }
                        }

                        ChangeIsConnected(true);
                    }
                }
                catch (Exception ex)
                {
                    HandleError(ex, "Error Reading from stream.");
                    //Try to reopen the connection
                    SafeConnect();
                }

                if (_readResult == asyncResult)
                {
                    _logger.LogDebug("Clearing Read Result.");
                    _readResult = null;
                }
            }

            if (hasNewData && DataReceived != null && !_isDisposed)
            {
                DataReceived(this, new EventArgs());
            }

            ReceiveData();
        }

        /// <summary>
        ///     Fetches and removes (pops) the next available group of bytes as received on this link in order (FIFO)
        /// </summary>
        /// <returns>null if the link is not Enabled or there is no data currently queued to return, an array of bytes otherwise.</returns>
        public byte[]? GetMessage()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("Cannot get message from disposed NetworkLink");
            }

            //Return null if the link is not enabled
            if (!IsEnabled)
            {
                return null;
            }

            byte[]? newMessage = null;
            lock (_incomingData)
            {
                if (HasData)
                {
                    newMessage = _incomingData[0];
                    _incomingData.RemoveAt(0);
                }
            }

            return newMessage;
        }

        private void ChangeIsConnected(bool value)
        {
            IsConnected = value;
            IsConnectedChanged?.Invoke(this, value);
        }

        private void HandleError(Exception ex, string message)
        {
            _logger.LogError(ex, message);
            ErrorOccurred?.Invoke(this, ex);
        }
    }
}