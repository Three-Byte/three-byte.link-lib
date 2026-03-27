using System.IO.Ports;
using ThreeByte.LinkLib.SerialLink;
using Xunit;

namespace ThreeByte.LinkLib.Tests
{
    public class SerialLinkSettingsTests
    {
        [Fact]
        public void Constructor_SetsAllProperties()
        {
            var settings = new SerialLinkSettings("COM3", 115200, 7, Parity.Even);

            Assert.Equal("COM3", settings.ComPort);
            Assert.Equal(115200, settings.BaudRate);
            Assert.Equal(7, settings.DataBits);
            Assert.Equal(Parity.Even, settings.Parity);
        }

        [Fact]
        public void Constructor_DefaultLikeValues()
        {
            var settings = new SerialLinkSettings("COM1", 9600, 8, Parity.None);

            Assert.Equal("COM1", settings.ComPort);
            Assert.Equal(9600, settings.BaudRate);
            Assert.Equal(8, settings.DataBits);
            Assert.Equal(Parity.None, settings.Parity);
        }

        [Fact]
        public void Constructor_HighBaudRate()
        {
            var settings = new SerialLinkSettings("COM5", 921600, 8, Parity.Odd);

            Assert.Equal(921600, settings.BaudRate);
            Assert.Equal(Parity.Odd, settings.Parity);
        }
    }
}
