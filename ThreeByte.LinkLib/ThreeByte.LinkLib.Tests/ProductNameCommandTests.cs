using ThreeByte.LinkLib.ProjectorLink.Commands;
using Xunit;

namespace ThreeByte.LinkLib.Tests
{
    public class ProductNameCommandTests
    {
        [Fact]
        public void GetCommandString_ReturnsCorrectQuery()
        {
            var cmd = new ProductNameCommand();
            Assert.Equal("%1INF2 ?", cmd.GetCommandString());
        }

        [Fact]
        public void ProcessAnswerString_Success_SetsProductName()
        {
            var cmd = new ProductNameCommand();
            cmd.ProcessAnswerString("%1INF2=EB-L1075U");

            Assert.Equal(CommandResponse.SUCCESS, cmd.CmdResponse);
            Assert.Equal("EB-L1075U", cmd.ProductName);
        }

        [Fact]
        public void DumpToString_ReturnsProductInfo()
        {
            var cmd = new ProductNameCommand();
            cmd.ProcessAnswerString("%1INF2=VPL-FHZ80");

            Assert.Equal("ProductName: VPL-FHZ80", cmd.DumpToString());
        }

        [Fact]
        public void ProductName_DefaultsToEmpty()
        {
            var cmd = new ProductNameCommand();
            Assert.Equal("", cmd.ProductName);
        }

        [Fact]
        public void ProcessAnswerString_Error_ReturnsFalse()
        {
            var cmd = new ProductNameCommand();
            bool result = cmd.ProcessAnswerString("%1INF2=ERR4");

            Assert.False(result);
            Assert.Equal(CommandResponse.PROJECTOR_FAILURE, cmd.CmdResponse);
        }
    }
}
