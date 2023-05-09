using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Logger
{
    public class GameLogger<T> : IGameLogger<T>
    {
        private readonly ILogger<T> logger;

        public GameLogger(ILogger<T> logger)
        {
            this.logger = logger;
        }

        public void Log(LogLevel level, string? message, Exception? exp = null, object?[]? args = null)
        {
            logger.Log(level, exp, message, args);
        }

        public void ConsoleL(LogLevel level, string? message, Exception? exp = null, object?[]? args = null)
        {
            using (LogContext.PushProperty("ConsoleOnly", value: true))
            {
                logger.Log(level, exp, message, args);
            }
        }

        public void File(LogLevel level, string? message, Exception? exp = null, object?[]? args = null)
        {
            using (LogContext.PushProperty("FileOnly", value: true))
            {
                logger.Log(level, exp, message, args);
            }
        }
    }
}
