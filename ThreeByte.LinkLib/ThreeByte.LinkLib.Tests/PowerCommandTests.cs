using ThreeByte.LinkLib.ProjectorLink.Commands;
using Xunit;

namespace ThreeByte.LinkLib.Tests
{
    public class PowerCommandTests
    {
        [Fact]
        public void GetCommandString_ON_ReturnsCorrectCommand()
        {
            var cmd = new PowerCommand(PowerCommand.Power.ON);
            Assert.Equal("%1POWR 1", cmd.GetCommandString());
        }

        [Fact]
        public void GetCommandString_OFF_ReturnsCorrectCommand()
        {
            var cmd = new PowerCommand(PowerCommand.Power.OFF);
            Assert.Equal("%1POWR 0", cmd.GetCommandString());
        }

        [Fact]
        public void GetCommandString_QUERY_ReturnsCorrectCommand()
        {
            var cmd = new PowerCommand(PowerCommand.Power.QUERY);
            Assert.Equal("%1POWR ?", cmd.GetCommandString());
        }

        [Fact]
        public void ProcessAnswerString_ON_Success_SetsStatusON()
        {
            var cmd = new PowerCommand(PowerCommand.Power.ON);
            cmd.ProcessAnswerString("%1POWR=OK");
            Assert.Equal(CommandResponse.SUCCESS, cmd.CmdResponse);
        }

        [Fact]
        public void ProcessAnswerString_Query_PowerOff_SetsStatusOFF()
        {
            var cmd = new PowerCommand(PowerCommand.Power.QUERY);
            cmd.ProcessAnswerString("%1POWR=0");
            Assert.Equal(CommandResponse.SUCCESS, cmd.CmdResponse);
            Assert.Equal(PowerStatus.OFF, cmd.Status);
        }

        [Fact]
        public void ProcessAnswerString_Query_PowerOn_SetsStatusON()
        {
            var cmd = new PowerCommand(PowerCommand.Power.QUERY);
            cmd.ProcessAnswerString("%1POWR=1");
            Assert.Equal(CommandResponse.SUCCESS, cmd.CmdResponse);
            Assert.Equal(PowerStatus.ON, cmd.Status);
        }

        [Fact]
        public void ProcessAnswerString_Query_Cooling_SetsStatusCOOLING()
        {
            var cmd = new PowerCommand(PowerCommand.Power.QUERY);
            cmd.ProcessAnswerString("%1POWR=2");
            Assert.Equal(PowerStatus.COOLING, cmd.Status);
        }

        [Fact]
        public void ProcessAnswerString_Query_Warmup_SetsStatusWARMUP()
        {
            var cmd = new PowerCommand(PowerCommand.Power.QUERY);
            cmd.ProcessAnswerString("%1POWR=3");
            Assert.Equal(PowerStatus.WARMUP, cmd.Status);
        }

        [Fact]
        public void ProcessAnswerString_Query_OutOfRange_SetsUnknown()
        {
            var cmd = new PowerCommand(PowerCommand.Power.QUERY);
            cmd.ProcessAnswerString("%1POWR=9");
            Assert.Equal(PowerStatus.UNKNOWN, cmd.Status);
        }

        [Fact]
        public void ProcessAnswerString_Error_SetsStatusUnknown()
        {
            var cmd = new PowerCommand(PowerCommand.Power.QUERY);
            cmd.ProcessAnswerString("%1POWR=ERR3");
            Assert.Equal(CommandResponse.UNAVAILABLE_TIME, cmd.CmdResponse);
            Assert.Equal(PowerStatus.UNKNOWN, cmd.Status);
        }

        [Fact]
        public void Status_DefaultsToUnknown()
        {
            var cmd = new PowerCommand(PowerCommand.Power.QUERY);
            Assert.Equal(PowerStatus.UNKNOWN, cmd.Status);
        }
    }
}
