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
using Runner.Factories;
using Runner.Services;
using Serilog;
using Serilog.Context;
using System.Reflection;

namespace CyFi
{
    public class Program
    {
        private static IConfiguration? configuration;

        private static IWebHostBuilder _hostBuilder;

        public static async Task Main(string[] args)
        {
            Environment.SetEnvironmentVariable("LOGPATH", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            string environment;
#if DEBUG
            environment = Environments.Development;
#elif RELEASE
        environment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? Environments.Development;;
#endif

            configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            KeyValuePair<string, string?> filePathKV = configuration.AsEnumerable().FirstOrDefault(kv => kv.Key.Contains("WriteTo:0:Args:Path"));

            // Overwrite file name because Serilog does not have a way to do it in appsettings
            configuration[filePathKV.Key] = filePathKV.Value?.Replace("@t", DateTime.UtcNow.ToString("yyyy'_'MM'_'dd'T'HH'_'mm'_'ss.ffff"));

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            using (LogContext.PushProperty("ConsoleOnly", value: true))
            {
                try
                {
                    Log.Information($"Starting {Assembly.GetCallingAssembly().GetName().Name}");
                    var host = Host.CreateDefaultBuilder(args)
                        .ConfigureServices((context, services) =>
                        {
                            services.AddSingleton<AppSettings>();
                            services.Configure<CyFiGameSettings>(configuration.GetSection("GameSettings"));
                            services.AddSingleton<BotFactory>();
                            services.AddSingleton<ICloudCallbackFactory, CloudCallbackFactory>();
                            services.AddSingleton<ICloudIntegrationService, CloudIntegrationService>();
                            services.AddSingleton<Queue<BotCommand>>();
                            services.AddSingleton<CyFiEngine>();
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
                    host.Services.GetRequiredService<ICloudIntegrationService>().Announce(CloudCallbackType.Initializing);

                    host.Run();

                    Log.Information($"{Assembly.GetCallingAssembly().GetName().Name} started successfully");

                    Console.WriteLine($"Application Running");
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, $"{Assembly.GetCallingAssembly().GetName().Name} failed to start");
                }
            }
        }
    }
}
