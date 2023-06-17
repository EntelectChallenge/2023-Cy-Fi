using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System.Text.Json;

namespace Logger
{
    public enum FILE_STATE
    {
        START,
        APPEND,
        END
    }

    public class GameLogger<T> : IGameLogger<T>
    {
        private readonly ILogger<T> logger;
        private static readonly string DIR_PATH = Path.Combine(Directory.GetCurrentDirectory(), "2023-CyFi-Logging");
        private static readonly string FILE_NAME = $"{DateTime.Now.ToString("yy-MM-dd-THHmm")}logging";
        private static IAmazonS3 s3Client;

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

        public async Task File(T? state, FILE_STATE? fileState, string filename = "")
        {
            Console.WriteLine($"LoggerPath: {Directory.GetCurrentDirectory()}");

            if (!Directory.Exists(DIR_PATH))
            {
                Directory.CreateDirectory(DIR_PATH);
            }

            if (filename == string.Empty)
            {
                filename = FILE_NAME;
            }

            var filePath = Path.Combine(DIR_PATH, $"{filename}.json");
            Console.WriteLine($"FILENAME {filePath}");

            string jsonState = "";

            if (state != null)
            {
                jsonState = JsonSerializer.Serialize(state);
            }

            using StreamWriter outputFile = new StreamWriter(filePath, true);

            switch (fileState)
            {
                case FILE_STATE.START:
                    jsonState = "[" + jsonState;
                    break;
                case FILE_STATE.APPEND:
                    jsonState = "," + jsonState;
                    break;
                case FILE_STATE.END:
                    jsonState = "]";
                    break;
                default:
                    break;
            }
            await outputFile.WriteAsync(jsonState);
        }

        public async Task FlushToS3(bool saveToS3)
        {
            Console.WriteLine("Game Complete. Saving logs...");

            if (saveToS3)
            {
                try
                {
                    var fullS3Path = Environment.GetEnvironmentVariable("S3_BUCKET_NAME");
                    if (!string.IsNullOrWhiteSpace(fullS3Path))
                    {
                        string[] parts = fullS3Path.Split('/');
                        var bucketName = parts[0];
                        string prefix = "/" + string.Join("/", parts.Skip(1));
                        var bucketRegion = RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION"));

                        Console.WriteLine("Beginning S3 Upload");
                        var s3Client = new AmazonS3Client(bucketRegion);
                        var transferUtility = new TransferUtility(s3Client);
                        TransferUtilityUploadDirectoryRequest uploadRequest = new()
                        {
                            BucketName = bucketName,
                            Directory = DIR_PATH,
                            KeyPrefix = prefix
                        };
                        await transferUtility.UploadDirectoryAsync(uploadRequest);
                        Console.WriteLine("Completed S3 Upload");
                    }                    
                }
                catch (Exception exp)
                {
                    Console.WriteLine($"Failed to upload to S3 - {exp.Message}");
                }
            }
        }
    }
}
