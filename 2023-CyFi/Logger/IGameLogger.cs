using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Logger
{
    public interface IGameLogger<T>
    {

        private static readonly string DIRECTROY_PATH = Path.Combine(Directory.GetCurrentDirectory(), "2023-CyFi-Logging");
        private static readonly string FILE_NAME = $"{DateTime.Now.ToString("yy-MM-dd-THHmm")}logging";

        void ConsoleL(LogLevel level, string? message, Exception? exp = null, object?[]? args = null);
        void File(LogLevel level, string? message, Exception? exp = null, object?[]? args = null);

        void Log(LogLevel level, string? message, Exception? exp = null, object?[]? args = null);




        static async void File(T? state, int fileState, string filename = "")
        {
            Console.WriteLine($"LoggerPath: {Directory.GetCurrentDirectory()}");

            if (!Directory.Exists(DIRECTROY_PATH))
            {
                Directory.CreateDirectory(DIRECTROY_PATH);
            }

            if(filename == string.Empty)
            {
                filename = FILE_NAME;
            }

            var filePath = Path.Combine(DIRECTROY_PATH, $"{filename}.json");
            Console.WriteLine($"FILENAME {filePath}");

            string jsonState = "";

            if (state != null)
            {
                jsonState = JsonSerializer.Serialize(state);
            }

            using (StreamWriter outputFile = new StreamWriter(filePath, true))
            {
                var options = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                switch (fileState)
                {
                    case 0:
                        jsonState = "[" + jsonState;
                        break;
                    case 1:
                        jsonState = "," + jsonState;
                        break;
                    case 2:
                        jsonState = "]";
                        break;
                    default:
                        break;
                }
                await outputFile.WriteAsync(jsonState);
            }
        }
    }
}