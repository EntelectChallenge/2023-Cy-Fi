using Microsoft.Extensions.Logging;

namespace Logger
{
    public interface IGameLogger<T>
    {
        void Console(LogLevel level, string? message, Exception? exp = null, object?[]? args = null);
        void File(LogLevel level, string? message, Exception? exp = null, object?[]? args = null);
        void Log(LogLevel level, string? message, Exception? exp = null, object?[]? args = null);
    }
}