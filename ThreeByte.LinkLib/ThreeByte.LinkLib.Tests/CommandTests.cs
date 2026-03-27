using ThreeByte.LinkLib.ProjectorLink.Commands;
using Xunit;

namespace ThreeByte.LinkLib.Tests
{
    public class CommandTests
    {
        // Concrete subclass for testing the abstract Command base class
        private class TestCommand : Command
        {
            internal override string GetCommandString() => "%1TEST ?";
        }

        [Fact]
        public void ProcessAnswerString_ERR1_SetsUndefinedCmd()
        {
            var cmd = new TestCommand();
            cmd.ProcessAnswerString("%1TEST=ERR1");
            Assert.Equal(CommandResponse.UNDEFINED_CMD, cmd.CmdResponse);
        }

        [Fact]
        public void ProcessAnswerString_ERR2_SetsUndefinedCmd()
        {
            var cmd = new TestCommand();
            cmd.ProcessAnswerString("%1TEST=ERR2");
            Assert.Equal(CommandResponse.UNDEFINED_CMD, cmd.CmdResponse);
        }

        [Fact]
        public void ProcessAnswerString_ERR3_SetsUnavailableTime()
        {
            var cmd = new TestCommand();
            cmd.ProcessAnswerString("%1TEST=ERR3");
            Assert.Equal(CommandResponse.UNAVAILABLE_TIME, cmd.CmdResponse);
        }

        [Fact]
        public void ProcessAnswerString_ERR4_SetsProjectorFailure()
        {
            var cmd = new TestCommand();
            cmd.ProcessAnswerString("%1TEST=ERR4");
            Assert.Equal(CommandResponse.PROJECTOR_FAILURE, cmd.CmdResponse);
        }

        [Fact]
        public void ProcessAnswerString_ERRA_SetsAuthFailure()
        {
            var cmd = new TestCommand();
            cmd.ProcessAnswerString("%1TEST ERRA");
            Assert.Equal(CommandResponse.AUTH_FAILURE, cmd.CmdResponse);
        }

        [Fact]
        public void ProcessAnswerString_OK_SetsSuccess()
        {
            var cmd = new TestCommand();
            cmd.ProcessAnswerString("%1TEST=OK");
            Assert.Equal(CommandResponse.SUCCESS, cmd.CmdResponse);
        }

        [Fact]
        public void ProcessAnswerString_ReturnsTrue_OnSuccess()
        {
            var cmd = new TestCommand();
            bool result = cmd.ProcessAnswerString("%1TEST=OK");
            Assert.True(result);
        }

        [Fact]
        public void ProcessAnswerString_ReturnsFalse_OnError()
        {
            var cmd = new TestCommand();
            bool result = cmd.ProcessAnswerString("%1TEST=ERR1");
            Assert.False(result);
        }

        [Fact]
        public void GetCommandString_ReturnsExpectedString()
        {
            var cmd = new TestCommand();
            Assert.Equal("%1TEST ?", cmd.GetCommandString());
        }

        [Fact]
        public void DumpToString_ReturnsEmptyByDefault()
        {
            var cmd = new TestCommand();
            Assert.Equal("", cmd.DumpToString());
        }
    }
}
