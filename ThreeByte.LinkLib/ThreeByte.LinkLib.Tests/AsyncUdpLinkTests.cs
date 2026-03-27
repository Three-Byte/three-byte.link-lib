using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ThreeByte.LinkLib.UdpLink;
using Xunit;

namespace ThreeByte.LinkLib.Tests
{
    public class AsyncUdpLinkTests : IDisposable
    {
        private AsyncUdpLink? _link;

        public void Dispose()
        {
            _link?.Dispose();
        }

        [Fact]
        public void Constructor_SetsProperties()
        {
            _link = new AsyncUdpLink("192.168.1.10", 5000, 6000, enabled: false);

            Assert.Equal("192.168.1.10", _link.Address);
            Assert.Equal(5000, _link.Port);
            Assert.False(_link.IsEnabled);
        }

        [Fact]
        public void Constructor_DefaultEnabled()
        {
            _link = new AsyncUdpLink("127.0.0.1", 5001, 0);

            Assert.True(_link.IsEnabled);
        }

        [Fact]
        public void HasData_InitiallyFalse()
        {
            _link = new AsyncUdpLink("127.0.0.1", 5002, 0, enabled: false);

            Assert.False(_link.HasData);
        }

        [Fact]
        public void GetMessage_WhenDisabled_ReturnsNull()
        {
            _link = new AsyncUdpLink("127.0.0.1", 5003, 0, enabled: false);

            Assert.Null(_link.GetMessage());
        }

        [Fact]
        public void GetMessage_WhenDisposed_ThrowsObjectDisposedException()
        {
            _link = new AsyncUdpLink("127.0.0.1", 5004, 0, enabled: false);
            _link.Dispose();

            Assert.Throws<ObjectDisposedException>(() => _link.GetMessage());
        }

        [Fact]
        public void SetEnabled_FiresEvent()
        {
            _link = new AsyncUdpLink("127.0.0.1", 5005, 0, enabled: false);
            bool eventFired = false;
            bool eventValue = false;
            _link.IsEnabledChanged += (s, v) => { eventFired = true; eventValue = v; };

            _link.SetEnabled(true);

            Assert.True(eventFired);
            Assert.True(eventValue);
        }

        [Fact]
        public void Dispose_MultipleCalls_DoesNotThrow()
        {
            _link = new AsyncUdpLink("127.0.0.1", 5006, 0, enabled: false);

            _link.Dispose();
            _link.Dispose();
        }

        [Fact]
        public void ImplementsIDisposable()
        {
            _link = new AsyncUdpLink("127.0.0.1", 5007, 0, enabled: false);
            Assert.IsAssignableFrom<IDisposable>(_link);
        }

        [Fact]
        public void Constructor_WithSettings_SetsProperties()
        {
            var settings = new UdpLinkSettings("10.0.0.5", 9000, 9001);
            _link = new AsyncUdpLink(settings, enabled: false);

            Assert.Equal("10.0.0.5", _link.Address);
            Assert.Equal(9000, _link.Port);
        }

        [Fact]
        public void SendAndReceive_Loopback()
        {
            // Use a local UDP client to send data to the link's local port
            int localPort;
            using (var tempSocket = new UdpClient(0))
            {
                localPort = ((IPEndPoint)tempSocket.Client.LocalEndPoint!).Port;
            }

            // Small delay to let the port free up
            Thread.Sleep(50);

            _link = new AsyncUdpLink("127.0.0.1", 50000, localPort, enabled: true);

            // Send a UDP packet to the link's local port
            using (var sender = new UdpClient())
            {
                byte[] data = new byte[] { 0x01, 0x02, 0x03 };
                sender.Send(data, data.Length, new IPEndPoint(IPAddress.Loopback, localPort));
            }

            // Wait for data to arrive
            Thread.Sleep(500);

            if (_link.HasData)
            {
                byte[]? msg = _link.GetMessage();
                Assert.NotNull(msg);
                Assert.Equal(new byte[] { 0x01, 0x02, 0x03 }, msg);
            }
        }

        [Fact]
        public void SendMessage_WhenDisabled_DoesNotThrow()
        {
            _link = new AsyncUdpLink("127.0.0.1", 5008, 0, enabled: false);

            _link.SendMessage(new byte[] { 0x01 });
        }

        [Fact]
        public void ErrorOccurred_EventCanBeSubscribed()
        {
            _link = new AsyncUdpLink("127.0.0.1", 5009, 0, enabled: false);
            Exception? ex = null;
            _link.ErrorOccurred += (s, e) => { ex = e; };

            Assert.Null(ex);
        }
    }
}
