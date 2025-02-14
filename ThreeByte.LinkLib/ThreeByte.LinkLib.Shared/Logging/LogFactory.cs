using Microsoft.Extensions.Logging;

namespace ThreeByte.LinkLib.Shared.Logging
{
    public class LogFactory
    {
        public static ILogger Create<T>()
        {
            var factory = LoggerFactory.Create(builder => {
                builder.AddConsole();
            });

            return factory.CreateLogger<T>();
        }
    }
}
