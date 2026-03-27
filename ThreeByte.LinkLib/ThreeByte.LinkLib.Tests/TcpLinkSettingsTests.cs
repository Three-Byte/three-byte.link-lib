using ThreeByte.LinkLib.TcpLink;
using Xunit;

namespace ThreeByte.LinkLib.Tests
{
    public class TcpLinkSettingsTests
    {
        [Fact]
        public void Constructor_SetsAddressAndPort()
        {
            var settings = new TcpLinkSettings("192.168.1.100", 9100);

            Assert.Equal("192.168.1.100", settings.Address);
            Assert.Equal(9100, settings.Port);
        }

        [Fact]
        public void Constructor_AcceptsHostname()
        {
            var settings = new TcpLinkSettings("projector.local", 4352);

            Assert.Equal("projector.local", settings.Address);
            Assert.Equal(4352, settings.Port);
        }

        [Fact]
        public void Constructor_AcceptsZeroPort()
        {
            var settings = new TcpLinkSettings("localhost", 0);

            Assert.Equal(0, settings.Port);
        }
    }
}
