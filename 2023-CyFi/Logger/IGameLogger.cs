using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Logger
{
    public interface IGameLogger<T>
    {
        void ConsoleL(LogLevel level, string? message, Exception? exp = null, object?[]? args = null);
        Task File(T? state, FILE_STATE? fileState, string filename = "");
        void Log(LogLevel level, string? message, Exception? exp = null, object?[]? args = null);
        Task FlushToS3(bool saveToS3);
    }
}