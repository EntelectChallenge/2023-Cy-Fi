using CyFi.Factories;
using CyFi.Models;
using CyFi.Runner;
using Domain.Enums;
using Domain.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Runner.Factories;
using Runner.Services;
using Serilog;
using Serilog.Context;
using Serilog.Extensions.Logging;
using System.Reflection;

namespace CyFi
{
    public class Program
    {
        private static IConfiguration? configuration;

        public static async Task Main(string[] args)
        {
            Environment.SetEnvironmentVariable("LOGPATH", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            string environment;
#if DEBUG
            environment = Environments.Development;
#elif RELEASE
            environment = string.Equals(Environment.GetEnvironmentVariable("ENVIRONMENT"), "Production", StringComparison.InvariantCultureIgnoreCase) ? Environments.Production : Environments.Development;
#endif

            configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            KeyValuePair<string, string?> filePathKV = configuration.AsEnumerable().FirstOrDefault(kv => kv.Key.Contains("WriteTo:0:Args:Path"));

            // Overwrite file name because Serilog does not have a way to do it in appsettings
            configuration[filePathKV.Key] = filePathKV.Value?.Replace("@t", DateTime.UtcNow.ToString("yyyy'_'MM'_'dd'T'HH'_'mm'_'ss.ffff"));

            configuration["GameSettings:NumberOfPlayers"] = string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("BOT_COUNT")) ? configuration["GameSettings:NumberOfPlayers"] : Environment.GetEnvironmentVariable("BOT_COUNT");

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            // Initialize CloudCallbackFactory manually so we can announce failures early
            ILogger<CloudIntegrationService> cloudLog = new SerilogLoggerFactory(Log.Logger).CreateLogger<CloudIntegrationService>();
            AppSettings appSettings = new();
            CloudCallbackFactory cloudCallbackFactory = new(appSettings);
            CloudIntegrationService cloudIntegrationService = new(appSettings, cloudLog, cloudCallbackFactory);

            using (LogContext.PushProperty("ConsoleOnly", value: true))
            {
                IHost? host = default;
                try
                {
                    Log.Information($"Starting {Assembly.GetCallingAssembly().GetName().Name}");
                    host = Host.CreateDefaultBuilder(args)
                        .ConfigureServices((context, services) =>
                        {
                            services.AddSingleton<AppSettings>();
                            services.Configure<CyFiGameSettings>(configuration.GetSection("GameSettings"));
                            services.AddSingleton<BotFactory>();
                            services.AddSingleton<ICloudCallbackFactory>(cloudCallbackFactory);
                            services.AddSingleton<ICloudIntegrationService>(cloudIntegrationService);
                            services.AddSingleton<Queue<BotCommand>>();
                            services.AddSingleton<CyFiEngine>();
                            services.AddSingleton<WorldFactory>();
                            services.AddSignalR(service =>
                            {
                                service.EnableDetailedErrors = true;
                                service.MaximumReceiveMessageSize = 40000000;
                            }
                            ).AddMessagePackProtocol();
                        })
                        .ConfigureWebHostDefaults((webBuilder) =>
                        {
                            webBuilder.UseUrls("http://*:5000");
                            webBuilder.Configure((app) =>
                            {
                                app.UseRouting();

                                app.UseEndpoints(endpoints =>
                                {
                                    endpoints.MapHub<RunnerHub>("/runnerhub");
                                });
                            });
                        })
                        .UseSerilog()
                        .Build();

                    var signalRConfig = configuration.GetSection("SignalR").GetChildren().ToDictionary(x => x.Key, x => x.Value);

                    var connection = new HubConnectionBuilder().WithUrl(signalRConfig["RunnerURL"]).Build();

                    Console.WriteLine($"SignalR Config{signalRConfig["RunnerURL"]}");

                    connection.StartAsync();

                    connection.KeepAliveInterval = TimeSpan.FromSeconds(1000);
                    connection.ServerTimeout = TimeSpan.FromSeconds(1000);

                    host.Services.GetRequiredService<CyFiEngine>().SetHubConnection(ref connection);
                    await cloudIntegrationService.Announce(CloudCallbackType.Initializing);

                    host.Run();

                    Log.Information($"{Assembly.GetCallingAssembly().GetName().Name} started successfully");

                    Console.WriteLine($"Application Running");
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, $"{Assembly.GetCallingAssembly().GetName().Name} failed to start");
                    await cloudIntegrationService.Announce(CloudCallbackType.Failed, ex);
                }
            }
        }
    }
}
