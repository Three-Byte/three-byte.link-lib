using Microsoft.Extensions.Logging;
using System.IO.Ports;
using ThreeByte.LinkLib.Shared.Logging;

namespace ThreeByte.LinkLib.SerialLink;

public class SerialLink : IDisposable
{
    public event EventHandler<bool>? IsEnabledChanged;
    public event EventHandler<bool>? IsConnectedChanged;
    public event EventHandler<Exception>? ErrorOccurred;
    public event EventHandler? DataReceived;
    public bool IsConnected => _isConnected;
    public bool IsEnabled => _isEnabled;
    public bool HasData => _incomingData.Count > 0;

    private const int MaxDataSize = 100;

    private readonly SerialLinkSettings _settings;
    private readonly ILogger _logger;

    private bool _isEnabled = true;
    private bool _isDisposed = false;
    private bool _isConnected = false;
    private List<byte[]> _incomingData = new ();
    private object _serialLock = new ();
    private SerialPort? _serialPort;

    public SerialLink(string comPort, int baudRate = 9600, int dataBits = 8, Parity parity = Parity.None, bool enabled = true)
        : this(new SerialLinkSettings(comPort, baudRate, dataBits, parity), enabled)
    {
    }

    public SerialLink(SerialLinkSettings settings, bool enabled = true)
    {
        _settings = settings;
        _isEnabled = enabled;
        _logger = LogFactory.Create<SerialLink>();
    }

    public void SendData(byte[] message)
    {
        if (!_isEnabled)
        {
            return;
        }

        lock (_serialLock)
        {
            try
            {
                _serialPort?.Write(message, 0, message.Length);
            }
            catch (Exception ex)
            {
                HandleError(ex, "Cannot write serial data.");
                ChangeIsConnected(false);
                SafeClose();
                SafeConnect();
            }
        }
    }

    public byte[]? GetMessage()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        if (!_isEnabled)
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

    /// <summary>
    /// Sets a value indicating whether messages should be propagated to the network or not
    /// </summary>
    /// <param name="value"></param>
    public void SetEnabled(bool value)
    {
        _isEnabled = value;

        if (!_isEnabled)
        {
            SafeClose();
        }
        else if (!_isConnected)
        {
            SafeConnect();
        }

        IsEnabledChanged?.Invoke(this, value);
    }

    /// <summary>
    /// Cancels the thread and releases resources.
    /// Clients of this class are responsible for calling it.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _logger.LogInformation("Cleaning up serial ports.");

        SafeClose();
    }

    private void SafeClose()
    {
        _logger.LogDebug("Safe Close.");

        lock (_serialLock)
        {
            if (_serialPort != null)
            {
                try
                {
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                        _serialPort.DataReceived -= OnDataReceived;
                    }
                }
                catch (ObjectDisposedException objDisEx)
                {
                    HandleError(objDisEx, "Cannot close disposed SerialLink.");
                }
            }

            _serialPort = null;

            lock (_incomingData)
            {
                _incomingData.Clear();
            }

            ChangeIsConnected(false);
        }
    }

    private void SafeConnect()
    {
        if (_isDisposed)
        {
            return;
        }

        lock (_serialLock)
        {
            try
            {
                if (_serialPort == null || !_isConnected)
                {
                    SafeClose();
                    _serialPort = new SerialPort(_settings.ComPort, _settings.BaudRate);
                    _serialPort.Parity = _settings.Parity;
                    _serialPort.DataBits = _settings.DataBits;
                    _serialPort.StopBits = StopBits.One;
                    _serialPort.DataReceived += new SerialDataReceivedEventHandler(OnDataReceived);
                }

                if (!_isConnected)
                {
                    _logger.LogInformation("Connecting: {com}.", _settings.ComPort);

                    try
                    {
                        _serialPort?.Open();
                        ChangeIsConnected(true);
                    }
                    catch (Exception ex)
                    {
                        HandleError(ex, "Serial Connection Error.");
                        ChangeIsConnected(false);
                    }
                }
            }
            catch (ObjectDisposedException objDisEx)
            {
                HandleError(objDisEx, "Cannot connect to disposed SerialLink.");
            }
        }
    }

    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        ReceiveData();
    }

    private void ReceiveData()
    {
        if (!_isEnabled)
        {
            return;
        }

        bool hasNewData = false;
        lock (_serialLock)
        {
            try
            {
                int bytesToRead = _serialPort?.BytesToRead ?? 0;
                byte[] buf = new byte[bytesToRead];
                int bytesRead = _serialPort?.Read(buf, 0, bytesToRead) ?? 0;

                _incomingData.Add(buf);
                hasNewData = true;
                if (_incomingData.Count > MaxDataSize)
                {
                    _logger.LogError("Too many incoming messages to handle: {qty}.", _incomingData.Count);
                    _incomingData.RemoveAt(_incomingData.Count - 1);
                }

                ChangeIsConnected(true);
            }
            catch (Exception ex)
            {
                HandleError(ex, "Error reading from serial.");
                ChangeIsConnected(false);
                SafeConnect();
            }
        }

        if (hasNewData && DataReceived != null && !_isDisposed)
        {
            DataReceived?.Invoke(this, new EventArgs());
        }
    }

    private void ChangeIsConnected(bool value)
    {
        _isConnected = value;
        IsConnectedChanged?.Invoke(this, value);
    }

    private void HandleError(Exception ex, string message)
    {
        _logger.LogError(exception: ex, message: message);
        ErrorOccurred?.Invoke(this, ex);
    }
}
