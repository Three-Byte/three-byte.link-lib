using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using ThreeByte.LinkLib.Shared.Logging;

namespace ThreeByte.LinkLib.UdpLink
{
    public class AsyncUdpLink
    {
        public event EventHandler<bool> IsEnabledChanged;
        public event EventHandler<Exception> ErrorOccurred;
        public event EventHandler DataReceived;
        public bool IsEnabled => _isEnabled;
        public string Address => _settings.Address;
        public int Port => _settings.RemotePort;
        public bool HasData => _incomingData.Count > 0;

        private const int MaxDataSize = 100;

        private readonly UdpLinkSettings _settings;
        private readonly ILogger _logger;

        private bool _isEnabled = true;
        private bool _isDisposed = false;
        private List<byte[]> _incomingData = new List<byte[]>();
        private object _clientLock = new object();
        private IAsyncResult _receiveResult = null;
        private IAsyncResult _sendResult = null;

        private UdpClient _udpClient;

        public AsyncUdpLink(string address, int remotePort, int localPort = 0, bool enabled = true)
            : this(new UdpLinkSettings(address, remotePort, localPort), enabled)
        {
        }

        public AsyncUdpLink(UdpLinkSettings settings, bool enabled = true)
        {
            _settings = settings;
            _isEnabled = enabled;
            _udpClient = new UdpClient(settings.LocalPort);
            _logger = LogFactory.Create<AsyncUdpLink>();

            ReceiveData();
        }

        /// <summary>
        /// Asynchronously sends the udp message
        /// </summary>
        /// <param name="message">binary message to be sent</param>
        public void SendMessage(byte[] message)
        {
            if (!_isEnabled)
            {
                return;
            }

            lock (_clientLock)
            {
                try
                {
                    _sendResult = _udpClient?.BeginSend(message, message.Length, _settings.Address, _settings.RemotePort, SendCallback, null);
                }
                catch (Exception ex)
                {
                    HandleError(ex, "Cannot Send.");
                }
            }
        }

        /// <summary>
        /// Fetches and removes (pops) the next available group of bytes as received on this link in order (FIFO)
        /// </summary>
        /// <returns>
        /// null if the link is not Enabled or there is no data currently queued to return, an array of bytes otherwise.
        /// </returns>
        public byte[] GetMessage()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("Cannot get message from disposed AsyncUdpLink.");
            }

            if (!_isEnabled)
            {
                return null;
            }

            byte[] newMessage = null;
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

            if (value)
            {
                lock (_clientLock)
                {
                    if (_receiveResult == null)
                    {
                        ReceiveData();
                    }
                }
            }

            IsEnabledChanged?.Invoke(this, value);
        }

        /// <summary>
        /// Implementation of IDisposable interface. Cancels the thread and releases resources.
        /// Clients of this class are responsible for calling it.
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

        /// <summary>
        /// Very carefully checks and shuts down the tcpClient and sets it to null
        /// </summary>
        private void SafeClose()
        {
            _logger.LogDebug("Safe Close.");

            lock (_clientLock)
            {
                if (_receiveResult != null)
                {
                    //End the read process
                    _receiveResult = null;
                }

                if (_sendResult != null)
                {
                    //End the write process
                    _sendResult = null;
                }

                _udpClient?.Client?.Close();
                _udpClient?.Close();
                _udpClient = null;

                lock (_incomingData)
                {
                    _incomingData.Clear();
                }
            }
        }

        private void SendCallback(IAsyncResult asyncResult)
        {
            lock (_clientLock)
            {
                try
                {
                    _udpClient?.EndSend(asyncResult);
                }
                catch (Exception ex)
                {
                    HandleError(ex, "Error sending message.");
                }

                if (_sendResult == asyncResult)
                {
                    _sendResult = null;
                }
            }
        }

        private void ReceiveData()
        {
            if (!_isEnabled)
            {
                return;
            }

            lock (_clientLock)
            {
                try
                {
                    _receiveResult = _udpClient?.BeginReceive(ReceiveCallback, null);
                }
                catch (Exception ex)
                {
                    HandleError(ex, "Cannot receive.");
                }
            }
        }

        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            bool hasNewData = false;

            lock (_clientLock)
            {
                try
                {
                    IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, _settings.RemotePort);
                    byte[] bytesRead = _udpClient?.EndReceive(asyncResult, ref remoteEndpoint) ?? Array.Empty<byte>();

                    if (_isEnabled)
                    {
                        if (bytesRead.Length > 0)
                        {
                            _incomingData.Add(bytesRead);
                            hasNewData = true;
                            while (_incomingData.Count > MaxDataSize)
                            {
                                //Purge messages from the end of the list to prevent overflow
                                _logger.LogError("Too many incoming messages to handle: {cnt}.", _incomingData.Count);
                                _incomingData.RemoveAt(_incomingData.Count - 1);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    HandleError(ex, "Error receiving from stream.");
                }

                if (_receiveResult == asyncResult)
                {
                    _receiveResult = null;
                }
            }

            if (hasNewData && DataReceived != null && !_isDisposed)
            {
                DataReceived(this, new EventArgs());
            }

            if (_isEnabled)
            {
                ReceiveData();
            }
        }

        private void HandleError(Exception ex, string message)
        {
            _logger.LogError(exception: ex, message: message);
            ErrorOccurred?.Invoke(this, ex);
        }
    }
}
