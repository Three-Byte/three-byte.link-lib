using Microsoft.Extensions.Logging;
using System.IO.Ports;
using System.Text;
using ThreeByte.LinkLib.Shared.Logging;

namespace ThreeByte.LinkLib.SerialLink;

public class FramedSerialLink
{
    public event EventHandler<Exception>? ErrorOccurred;
    public event EventHandler? DataReceived;
    public SerialFrame? SendFrame { get; set; }
    public SerialFrame? ReceiveFrame { get; set; }
    public bool IsConnected => _serialLink.IsConnected;
    public bool IsEnabled => _serialLink.IsEnabled;
    public bool HasData => _incomingData.Count > 0;

    private const int MaxDataSize = 100;

    private bool _isDisposed = false;
    private int _headerPos = 0;
    private int _footerPos = 0;
    private List<string> _incomingData = new ();
    private MemoryStream _incomingBuffer = new (2048);
    
    private readonly SerialLink _serialLink;
    private readonly ILogger _logger;

    public FramedSerialLink(string comPort, int baudRate = 9600, int dataBits = 8, Parity parity = Parity.None, bool enabled = true)
        : this(new SerialLinkSettings(comPort, baudRate, dataBits, parity), enabled)
    {
    }

    public FramedSerialLink(SerialLinkSettings settings, bool enabled = true)
    {
        _serialLink = new SerialLink(settings, enabled);
        _serialLink.DataReceived += OnDataReceived;
        _serialLink.ErrorOccurred += OnErrorOccurred;

        _logger = LogFactory.Create<FramedSerialLink>();
    }

    public void SetEnabled(bool value)
    {
        _serialLink.SetEnabled(value);

        if (!value)
        {
            //If the link is disabled, clear the buffer of messages
            lock (_incomingData)
            {
                _incomingData.Clear();
            }
        }
    }

    /// <summary>
    /// Asynchronously sends the tcp message, waiting until the connection is reestablihsed if necessary
    /// </summary>
    /// <param name="message"></param>
    public void SendMessage(string message)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        //Don't do anything if the link is not enabled
        if (!IsEnabled)
        {
            return;
        }

        //Add the header and footer
        byte[] header = [];
        if (SendFrame != null && SendFrame.Header != null)
        {
            header = SendFrame.Header;
        }
        byte[] footer = [];
        if (SendFrame != null && SendFrame.Footer != null)
        {
            footer = SendFrame.Footer;
        }
        byte[] messageBytes = new byte[message.Length + header.Length + footer.Length];

        header.CopyTo(messageBytes, 0);
        Encoding.UTF8.GetBytes(message, 0, message.Length, messageBytes, header.Length);
        footer.CopyTo(messageBytes, message.Length + header.Length);

        if (_serialLink != null)
        {
            try
            {
                _serialLink.SendData(messageBytes);
            }
            catch (ObjectDisposedException ode)
            {
                HandleError(ode, "Cannot send a message of disposed FramedSerialLink.");
            }
            catch (Exception ex)
            {
                //Also possible for the serial link to raise and UnauthorizedAccessException here
                HandleError(ex, "SendMessage error.");
            }
        }
    }

    /// <summary>
    /// Fetches and removes (pops) the next available message as received on this link in order (FIFO)
    /// </summary>
    /// <returns>null if the link is not Enabled or there are no messages currently queued to return, a string otherwise.</returns>
    public string? GetMessage()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        //Don't do anything if the link is not enabled
        if (!IsEnabled)
        {
            return null;
        }

        string? newMessage = null;
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
    /// Implementation of IDisposable interface.  Cancels the thread and releases resources.
    /// Clients of this class are responsible for calling it.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        _logger.LogInformation("Cleaning up serial resources.");
        _serialLink.DataReceived -= OnDataReceived;
        _serialLink.ErrorOccurred -= OnErrorOccurred;
        _serialLink.Dispose();
    }

    private void OnDataReceived(object? sender, EventArgs e)
    {
        bool hasNewData = false;

        byte[] header = [];
        if (ReceiveFrame != null && ReceiveFrame.Header != null)
        {
            header = ReceiveFrame.Header;
        }

        byte[] footer = [];
        if (ReceiveFrame != null && ReceiveFrame.Footer != null)
        {
            footer = ReceiveFrame.Footer;
        }

        while (_serialLink.HasData)
        {
            lock (_incomingBuffer)
            {
                byte[]? buffer = _serialLink.GetMessage();

                //Must validate this buffer - see issue #4934
                if (buffer == null)
                {
                    break;
                }

                for (int i = 0; i < buffer.Length; i++)
                {
                    if (_headerPos < header.Length - 1 && buffer[i] == header[_headerPos])
                    {
                        _headerPos++;
                    }
                    else if (_headerPos == header.Length - 1 && buffer[i] == header[_headerPos])
                    {
                        _headerPos = 0;
                        _footerPos = 0;
                        _incomingBuffer.Position = 0; //Reset to the beginning
                    }
                    else if (_footerPos < footer.Length - 1 && buffer[i] == footer[_footerPos])
                    {
                        _footerPos++;
                    }
                    else if (_footerPos == footer.Length - 1 && buffer[i] == footer[_footerPos])
                    {
                        _footerPos = 0;  //Reset Footer

                        string newMessage = Encoding.UTF8.GetString(_incomingBuffer.GetBuffer(), 0, (int)_incomingBuffer.Position);
                        if (newMessage.Trim() != string.Empty)
                        {
                            //log.Debug("Adding Message: " + newMessage.Substring(0, Math.Min(30, newMessage.Length)));
                            lock (_incomingData)
                            {
                                _incomingData.Add(newMessage);
                                if (_incomingData.Count > MaxDataSize)
                                {
                                    //Purge messages from the end of the list to prevent overflow
                                    _logger.LogError("Too many incoming messages to handle: {qty}", _incomingData.Count);
                                    _incomingData.RemoveAt(_incomingData.Count - 1);
                                }
                            }
                        }
                        hasNewData = true;
                        _incomingBuffer.Position = 0;
                    }
                    else
                    {
                        _headerPos = 0;
                        _incomingBuffer.WriteByte(buffer[i]);
                    }
                }
            }
        }

        if (hasNewData && DataReceived != null && !_isDisposed)
        {
            DataReceived(this, new EventArgs());
        }
    }

    private void OnErrorOccurred(object? sender, Exception ex)
    {
        ErrorOccurred?.Invoke(sender, ex);
    }

    private void HandleError(Exception ex, string message)
    {
        _logger.LogError(exception: ex, message: message);
        ErrorOccurred?.Invoke(this, ex);
    }
}
