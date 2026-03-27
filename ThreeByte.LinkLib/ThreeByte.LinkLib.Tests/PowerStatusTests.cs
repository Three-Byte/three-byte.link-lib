using ThreeByte.LinkLib.ProjectorLink.Commands;
using Xunit;

namespace ThreeByte.LinkLib.Tests
{
    public class PowerStatusTests
    {
        [Fact]
        public void OFF_HasValue0()
        {
            Assert.Equal(0, (int)PowerStatus.OFF);
        }

        [Fact]
        public void ON_HasValue1()
        {
            Assert.Equal(1, (int)PowerStatus.ON);
        }

        [Fact]
        public void COOLING_HasValue2()
        {
            Assert.Equal(2, (int)PowerStatus.COOLING);
        }

        [Fact]
        public void WARMUP_HasValue3()
        {
            Assert.Equal(3, (int)PowerStatus.WARMUP);
        }

        [Fact]
        public void UNKNOWN_HasValue4()
        {
            Assert.Equal(4, (int)PowerStatus.UNKNOWN);
        }
    }

    public class CommandResponseTests
    {
        [Theory]
        [InlineData(CommandResponse.SUCCESS)]
        [InlineData(CommandResponse.UNDEFINED_CMD)]
        [InlineData(CommandResponse.OUT_OF_PARAMETER)]
        [InlineData(CommandResponse.UNAVAILABLE_TIME)]
        [InlineData(CommandResponse.PROJECTOR_FAILURE)]
        [InlineData(CommandResponse.AUTH_FAILURE)]
        [InlineData(CommandResponse.COMMUNICATION_ERROR)]
        public void AllValues_AreDefined(CommandResponse response)
        {
            Assert.True(System.Enum.IsDefined(typeof(CommandResponse), response));
        }
    }
}
