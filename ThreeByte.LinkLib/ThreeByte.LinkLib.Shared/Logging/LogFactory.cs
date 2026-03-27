using System;
using Microsoft.Extensions.Logging;

namespace ThreeByte.LinkLib.Shared.Logging
{
    /// <summary>
    ///     Factory for creating loggers for the shared libs
    /// </summary>
    public class LogFactory
    {
        private static readonly Lazy<ILoggerFactory> Factory = new Lazy<ILoggerFactory>(() =>
            LoggerFactory.Create(builder => { builder.AddConsole(); }));

        public static ILogger Create<T>()
        {
            return Factory.Value.CreateLogger<T>();
        }
    }
}
