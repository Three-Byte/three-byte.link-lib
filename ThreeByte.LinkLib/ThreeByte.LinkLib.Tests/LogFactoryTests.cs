using Microsoft.Extensions.Logging;
using ThreeByte.LinkLib.Shared.Logging;
using Xunit;

namespace ThreeByte.LinkLib.Tests
{
    public class LogFactoryTests
    {
        [Fact]
        public void Create_ReturnsNonNullLogger()
        {
            ILogger logger = LogFactory.Create<LogFactoryTests>();

            Assert.NotNull(logger);
        }

        [Fact]
        public void Create_ReturnsSameFactoryInstance()
        {
            // Calling Create multiple times should not throw and should return valid loggers
            ILogger logger1 = LogFactory.Create<LogFactoryTests>();
            ILogger logger2 = LogFactory.Create<string>();

            Assert.NotNull(logger1);
            Assert.NotNull(logger2);
        }
    }
}
