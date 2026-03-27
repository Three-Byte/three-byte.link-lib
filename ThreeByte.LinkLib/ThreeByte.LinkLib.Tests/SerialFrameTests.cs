using ThreeByte.LinkLib.SerialLink;
using Xunit;

namespace ThreeByte.LinkLib.Tests
{
    public class SerialFrameTests
    {
        [Fact]
        public void Header_CanBeSetAndRetrieved()
        {
            var frame = new SerialFrame
            {
                Header = new byte[] { 0x02 }
            };

            Assert.Equal(new byte[] { 0x02 }, frame.Header);
        }

        [Fact]
        public void Footer_CanBeSetAndRetrieved()
        {
            var frame = new SerialFrame
            {
                Footer = new byte[] { 0x03 }
            };

            Assert.Equal(new byte[] { 0x03 }, frame.Footer);
        }

        [Fact]
        public void HeaderAndFooter_MultiByteSequences()
        {
            var frame = new SerialFrame
            {
                Header = new byte[] { 0x02, 0x01 },
                Footer = new byte[] { 0x0D, 0x0A }
            };

            Assert.Equal(2, frame.Header.Length);
            Assert.Equal(2, frame.Footer.Length);
            Assert.Equal(0x02, frame.Header[0]);
            Assert.Equal(0x0A, frame.Footer[1]);
        }
    }
}
