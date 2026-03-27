using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using ThreeByte.LinkLib.Shared.Logging;

namespace ThreeByte.LinkLib.NetBooter
{
    public class NetBooterLink : INotifyPropertyChanged
    {
        private readonly string _ipAddress;
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly Dictionary<int, bool> _powerStates = new Dictionary<int, bool>();

        public NetBooterLink(string ipAddress)
        {
            _logger = LogFactory.Create<NetBooterLink>();
            _ipAddress = ipAddress;

            var handler = new HttpClientHandler
            {
                Credentials = new NetworkCredential("admin", "admin")
            };
            _httpClient = new HttpClient(handler);
        }

        public bool this[int port]
        {
            get
            {
                if (_powerStates.ContainsKey(port))
                {
                    return _powerStates[port];
                }

                return false; // report false for all other ports that don't exist
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<Exception>? ErrorOccurred;

        public void Power(int outlet, bool state)
        {
            try
            {
                var commandUri = string.Format("http://{0}/cmd.cgi?$A3 {1} {2}", _ipAddress, outlet, state ? 1 : 0);
                var response = _httpClient.GetStringAsync(commandUri).GetAwaiter().GetResult();
                _logger.LogDebug("Response: {0}", response);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error setting power state: {0} Outlet {1} [{2}]", _ipAddress, outlet, state);
                HandleError(ex, "Cannot Set Status");
            }
        }

        public void PollState()
        {
            try
            {
                var commandUri = string.Format("http://{0}/cmd.cgi?$A5", _ipAddress);
                var response = _httpClient.GetStringAsync(commandUri).GetAwaiter().GetResult();

                // Expected response: xxxx,cccc,tttt
                // read right to left for each field, eg - 01 means port 1 is on
                var powerStates = response.Split(',')[0].ToCharArray();
                for (var i = 0; i < powerStates.Length; i++)
                {
                    var powerBit = powerStates[powerStates.Length - 1 - i];
                    if (powerBit == '1')
                    {
                        _powerStates[i + 1] = true;
                    }
                    else if (powerBit == '0')
                    {
                        _powerStates[i + 1] = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting power state: {0}", ex);
                HandleError(ex, "Cannot Get Status");
            }
        }

        private void HandleError(Exception ex, string message)
        {
            _logger.LogError(ex, message);
            ErrorOccurred?.Invoke(this, ex);
        }
    }
}