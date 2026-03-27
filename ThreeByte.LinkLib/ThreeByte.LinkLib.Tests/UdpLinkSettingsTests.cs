using ThreeByte.LinkLib.UdpLink;
using Xunit;

namespace ThreeByte.LinkLib.Tests
{
    public class UdpLinkSettingsTests
    {
        [Fact]
        public void Constructor_ThreeArgs_SetsAllProperties()
        {
            var settings = new UdpLinkSettings("192.168.1.50", 5000, 6000);

            Assert.Equal("192.168.1.50", settings.Address);
            Assert.Equal(5000, settings.RemotePort);
            Assert.Equal(6000, settings.LocalPort);
        }

        [Fact]
        public void Constructor_TwoArgs_DefaultsLocalPortToZero()
        {
            var settings = new UdpLinkSettings("10.0.0.1", 8080);

            Assert.Equal("10.0.0.1", settings.Address);
            Assert.Equal(8080, settings.RemotePort);
            Assert.Equal(0, settings.LocalPort);
        }
    }
}
