using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ThreeByte.LinkLib.TcpLink;
using Xunit;

namespace ThreeByte.LinkLib.Tests
{
    public class AsyncTcpLinkTests : IDisposable
    {
        private AsyncTcpLink? _link;

        public void Dispose()
        {
            _link?.Dispose();
        }

        [Fact]
        public void Constructor_Disabled_DoesNotConnect()
        {
            _link = new AsyncTcpLink("127.0.0.1", 19999, enabled: false);

            Assert.False(_link.IsConnected);
            Assert.False(_link.IsEnabled);
        }

        [Fact]
        public void Address_ReturnsConfiguredAddress()
        {
            _link = new AsyncTcpLink("10.0.0.50", 9100, enabled: false);

            Assert.Equal("10.0.0.50", _link.Address);
            Assert.Equal(9100, _link.Port);
        }

        [Fact]
        public void HasData_InitiallyFalse()
        {
            _link = new AsyncTcpLink("127.0.0.1", 19999, enabled: false);

            Assert.False(_link.HasData);
        }

        [Fact]
        public void GetMessage_WhenDisabled_ReturnsNull()
        {
            _link = new AsyncTcpLink("127.0.0.1", 19999, enabled: false);

            Assert.Null(_link.GetMessage());
        }

        [Fact]
        public void GetMessage_WhenDisposed_ThrowsObjectDisposedException()
        {
            _link = new AsyncTcpLink("127.0.0.1", 19999, enabled: false);
            _link.Dispose();

            Assert.Throws<ObjectDisposedException>(() => _link.GetMessage());
        }

        [Fact]
        public void SendMessage_WhenDisabled_DoesNotThrow()
        {
            _link = new AsyncTcpLink("127.0.0.1", 19999, enabled: false);

            // Should silently return without throwing
            _link.SendMessage(new byte[] { 0x01, 0x02 });
        }

        [Fact]
        public void SetEnabled_True_FiresEvent()
        {
            _link = new AsyncTcpLink("127.0.0.1", 19999, enabled: false);
            bool eventFired = false;
            bool eventValue = false;
            _link.IsEnabledChanged += (s, v) => { eventFired = true; eventValue = v; };

            _link.SetEnabled(true);

            Assert.True(eventFired);
            Assert.True(eventValue);
            Assert.True(_link.IsEnabled);
        }

        [Fact]
        public void SetEnabled_False_FiresEvent()
        {
            _link = new AsyncTcpLink("127.0.0.1", 19999, enabled: false);
            bool eventFired = false;
            _link.IsEnabledChanged += (s, v) => { eventFired = true; };

            _link.SetEnabled(false);

            Assert.True(eventFired);
            Assert.False(_link.IsEnabled);
        }

        [Fact]
        public void Dispose_MultipleCalls_DoesNotThrow()
        {
            _link = new AsyncTcpLink("127.0.0.1", 19999, enabled: false);

            _link.Dispose();
            _link.Dispose(); // Second call should be safe
        }

        [Fact]
        public void IsConnectedChanged_EventCanBeSubscribed()
        {
            _link = new AsyncTcpLink("127.0.0.1", 19999, enabled: false);
            bool eventFired = false;
            _link.IsConnectedChanged += (s, v) => { eventFired = true; };

            // Event wont fire without a real connection, but subscription should work
            Assert.False(eventFired);
        }

        [Fact]
        public void ErrorOccurred_EventCanBeSubscribed()
        {
            _link = new AsyncTcpLink("127.0.0.1", 19999, enabled: false);
            Exception? receivedException = null;
            _link.ErrorOccurred += (s, ex) => { receivedException = ex; };

            // No error should occur in disabled mode
            Assert.Null(receivedException);
        }

        [Fact]
        public void ConnectAndExchangeData_WithLocalServer()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;

            // Keep the server connection open so the link stays connected
            TcpClient? serverClient = null;
            NetworkStream? serverStream = null;
            var serverThread = new Thread(() =>
            {
                serverClient = listener.AcceptTcpClient();
                serverStream = serverClient.GetStream();
                byte[] buf = new byte[1024];
                int n = serverStream.Read(buf, 0, buf.Length);
                serverStream.Write(buf, 0, n);
                // Keep connection open until test finishes
                Thread.Sleep(5000);
            });
            serverThread.IsBackground = true;
            serverThread.Start();

            try
            {
                _link = new AsyncTcpLink("127.0.0.1", port, enabled: true);

                // Poll for data with timeout instead of fixed sleep
                Thread.Sleep(500);
                _link.SendMessage(new byte[] { 0xAA, 0xBB, 0xCC });

                byte[]? received = null;
                for (int i = 0; i < 20; i++)
                {
                    Thread.Sleep(100);
                    if (_link.HasData)
                    {
                        received = _link.GetMessage();
                        break;
                    }
                }

                Assert.NotNull(received);
                Assert.Equal(new byte[] { 0xAA, 0xBB, 0xCC }, received);
            }
            finally
            {
                serverClient?.Close();
                listener.Stop();
            }
        }

        [Fact]
        public void DoesNotStackOverflow_WhenServerClosesConnection()
        {
            // Regression test: before the ThreadPool trampoline fix, the
            // ReadCallback -> ReceiveData -> BeginRead -> ReadCallback chain
            // could recurse on the same stack when BeginRead completed
            // synchronously (e.g. on connection close), causing a stack overflow
            // that crashed the process.
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;

            var serverThread = new Thread(() =>
            {
                var client = listener.AcceptTcpClient();
                // Close immediately to trigger synchronous BeginRead completion
                client.Close();
            });
            serverThread.IsBackground = true;
            serverThread.Start();

            try
            {
                _link = new AsyncTcpLink("127.0.0.1", port, enabled: true);

                // Give it enough time for the read loop to hit the closed connection.
                // Before the fix this would stack overflow and crash the process.
                Thread.Sleep(2000);

                // If we get here, the process didn't crash — the fix works.
                // The link auto-reconnects, so we don't assert IsConnected state.
            }
            finally
            {
                listener.Stop();
            }
        }
    }
}
