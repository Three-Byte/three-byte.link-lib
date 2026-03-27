using ThreeByte.LinkLib.ProjectorLink.Commands;
using Xunit;

namespace ThreeByte.LinkLib.Tests
{
    public class ProjectorNameCommandTests
    {
        [Fact]
        public void GetCommandString_ReturnsCorrectQuery()
        {
            var cmd = new ProjectorNameCommand();
            Assert.Equal("%1NAME ?", cmd.GetCommandString());
        }

        [Fact]
        public void ProcessAnswerString_Success_SetsName()
        {
            var cmd = new ProjectorNameCommand();
            cmd.ProcessAnswerString("%1NAME=MainHall");

            Assert.Equal(CommandResponse.SUCCESS, cmd.CmdResponse);
            Assert.Equal("MainHall", cmd.Name);
        }

        [Fact]
        public void DumpToString_ReturnsNameInfo()
        {
            var cmd = new ProjectorNameCommand();
            cmd.ProcessAnswerString("%1NAME=Theater1");

            Assert.Equal("Name: Theater1", cmd.DumpToString());
        }

        [Fact]
        public void Name_DefaultsToEmpty()
        {
            var cmd = new ProjectorNameCommand();
            Assert.Equal("", cmd.Name);
        }

        [Fact]
        public void ProcessAnswerString_Error_ReturnsFalse()
        {
            var cmd = new ProjectorNameCommand();
            bool result = cmd.ProcessAnswerString("%1NAME ERRA");

            Assert.False(result);
            Assert.Equal(CommandResponse.AUTH_FAILURE, cmd.CmdResponse);
        }
    }
}
