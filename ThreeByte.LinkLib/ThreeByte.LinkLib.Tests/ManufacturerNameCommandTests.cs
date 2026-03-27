using ThreeByte.LinkLib.ProjectorLink.Commands;
using Xunit;

namespace ThreeByte.LinkLib.Tests
{
    public class ManufacturerNameCommandTests
    {
        [Fact]
        public void GetCommandString_ReturnsCorrectQuery()
        {
            var cmd = new ManufacturerNameCommand();
            Assert.Equal("%1INF1 ?", cmd.GetCommandString());
        }

        [Fact]
        public void ProcessAnswerString_Success_SetsManufacturer()
        {
            var cmd = new ManufacturerNameCommand();
            cmd.ProcessAnswerString("%1INF1=Epson");

            Assert.Equal(CommandResponse.SUCCESS, cmd.CmdResponse);
            Assert.Equal("Epson", cmd.Manufacturer);
        }

        [Fact]
        public void ProcessAnswerString_WithSpaces_SetsManufacturer()
        {
            var cmd = new ManufacturerNameCommand();
            cmd.ProcessAnswerString("%1INF1=Sony Corporation");

            Assert.Equal("Sony Corporation", cmd.Manufacturer);
        }

        [Fact]
        public void DumpToString_ReturnsManufacturerInfo()
        {
            var cmd = new ManufacturerNameCommand();
            cmd.ProcessAnswerString("%1INF1=NEC");

            Assert.Equal("Manufacturer: NEC", cmd.DumpToString());
        }

        [Fact]
        public void Manufacturer_DefaultsToEmpty()
        {
            var cmd = new ManufacturerNameCommand();
            Assert.Equal("", cmd.Manufacturer);
        }

        [Fact]
        public void ProcessAnswerString_Error_ReturnsFalse()
        {
            var cmd = new ManufacturerNameCommand();
            bool result = cmd.ProcessAnswerString("%1INF1=ERR1");

            Assert.False(result);
            Assert.Equal(CommandResponse.UNDEFINED_CMD, cmd.CmdResponse);
        }
    }
}
