using System.Net;
using System.Text;
using Domain.Configs;
using Engine.Communication;
using Engine.Enums;
using Engine.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Engine.Services
{
    public class SignalRService// : ISignalRService
    {
        private readonly EngineService engineService;
        private readonly EngineConfig engineConfig;
        private HubConnection connection;
        private string runnerUrl;
        private readonly CommandQueue commandQueue;
        private readonly List<Bot> bots = new();
        private readonly GameController gameController;

        public SignalRService(
            IConfigurationService engineConfig,
            EngineService engineService,
            CommandQueue commandQueue, GameController gameController)
        {
            this.engineService = engineService;
            this.engineConfig = engineConfig.Value;
            this.commandQueue = commandQueue;
            this.gameController = gameController;
        }

        public async Task Startup()
        {
            Logger.LogInfo("Core", "Starting up");
            
            var ip = Environment.GetEnvironmentVariable("RunnerIp");
            ip = string.IsNullOrWhiteSpace(ip)
                ? engineConfig.RunnerUrl
                : ip.StartsWith("http://")
                    ? ip
                    : "http://" + ip;

            runnerUrl = ip + ":" + engineConfig.RunnerPort;

            var canSeeRunner = false;
            using (var httpClient = new HttpClient())
            {
                while (!canSeeRunner)
                {
                    Logger.LogDebug("Core.Startup", "Testing network visibility of Runner");
                    Logger.LogDebug("Core.Startup", $"Testing URL: {runnerUrl}");
                    try
                    {
                        var result = await httpClient.GetAsync($"{runnerUrl}/api/health/runner");
                        if (result.StatusCode != HttpStatusCode.OK)
                        {
                            Logger.LogDebug(
                                "Core.Startup",
                                $"Can not see runner at {runnerUrl}/api/health/runner. Waiting 1 second and trying again.");
                            Thread.Sleep(1000);
                            continue;
                        }
                    }
                    catch (Exception)
                    {
                        Logger.LogDebug(
                            "Core.Startup",
                            $"Can not see runner at {runnerUrl}/api/health/runner. Waiting 1 second and trying again.");
                        Thread.Sleep(1000);
                        continue;
                    }

                    Logger.LogDebug("Core.Startup", $"Can see runner at {runnerUrl}");
                    canSeeRunner = true;
                }
            }

            Logger.LogDebug("SignalR.Startup", $"Connecting SignalR to {runnerUrl}");
            connection = new HubConnectionBuilder().WithUrl($"{runnerUrl}/runnerhub")
                .AddMessagePackProtocol()
                .ConfigureLogging(logging => { logging.SetMinimumLevel(LogLevel.Information); })
                .Build();
            try
            {
                await connection.StartAsync()
                    .ContinueWith(
                        async task =>
                        {
                            Logger.LogDebug("SignalR.Startup", "SignalR Started");
                            Logger.LogDebug("SignalR.Startup", $"Connection State: {connection.State}");

                            await connection.InvokeAsync("RegisterGameEngine");

                            SetUpEndpoints(); // does this need to happen here or can it happen before starting the connection?

                            try
                            {
                                engineService.SetHubConnection(ref connection);
                                await engineService.RunGameLoop();
                            }
                            catch (Exception e)
                            {
                                Logger.LogError("Core", $"Failed to run GameRunLoop with error: {e.Message}");
                                Logger.LogDebug("Core", e.StackTrace);
                                await ShutdownWithError(e);
                            }
                        });
            }
            catch (Exception e)
            {
                Logger.LogError("Core", $"Failed to run SignalR with error: {e.Message}");
                await ShutdownWithError(e);
            }
        }

        public HubConnection GetHubConnection()
        {
            return connection;
        }

        private void SetUpEndpoints()
        {
            connection.On<Guid, string>("RegisterBot", OnRegisterBot);
            
            /* Disconnect Request Handler */
            connection.On<Guid>("Disconnect", OnDisconnect);

            /* Bot Command Handler*/
            gameController.Setup(connection);
            
            connection.On("StartGame", OnStartGame);
            connection.On<int>("TickAck", OnTickAck);
        }

        private void OnTickAck(int arg)
        {
            engineService.TickAcknowledged = arg;
        }

        private void OnStartGame()
        {
            if (engineService.PendingStart ||
                engineService.GameStarted)
            {
                return;
            }

            Logger.LogDebug("Core.StartGame", "Waiting 5 seconds before game start");
            Logger.LogDebug("Core.ConnectionState", connection.State);

            // var publishedState = worldStateService.GetPublishedState();
            // connection.SendAsync("PublishGameState", publishedState);

            connection.SendAsync("ReceiveConfigValues", engineConfig);
            
            engineService.PendingStart = true;
            for (var i = 5; i > 1; i--)
            {
                Logger.LogInfo("Core.StartGame", $"Game starting in {i}");
                Thread.Sleep(1000);
            }

            Logger.LogDebug("Core.ConnectionState", connection.State);

            engineService.GameStarted = true;
            engineService.PendingStart = false;
        }

        private void OnRegisterBot(Guid id, string nickName)
        {
            Logger.LogDebug("Core", "Registering new Bot");
            bots.Add(new Bot(id, nickName));
        }

        private void OnDisconnect(Guid id)
        {
            Logger.LogInfo("Core", "Disconnecting...");
            connection.StopAsync();
            Logger.LogInfo("Core", "Disconnected from SignalR.");
            Environment.Exit(0);
        }

        private async Task ShutdownWithError(Exception exception)
        {
            Logger.LogError("Shutdown", "Shutting down due to a critical error");
            if (engineService.HasWinner)
            {
                Logger.LogInfo("Shutdown",
                    "Shutdown called with critical error, but a winner was already found. Moving on.");
                OnDisconnect(new Guid());
                return;
            }

            Logger.LogError("Shutdown", "Shutting down before a winner was found. Informing the runner.");

            using var httpClient = new HttpClient();
            try
            {
                var connectionInformation = new ConnectionInformation
                {
                    Reason = "Shutdown called before a winner was found",
                    Status = ConnectionStatus.Disconnected
                };
                var content = new StringContent(JsonConvert.SerializeObject(connectionInformation), Encoding.UTF8,
                    "application/json");
                var result = await httpClient.PostAsync($"{runnerUrl}/api/connections/engine", content);
                if (result.StatusCode != HttpStatusCode.OK)
                {
                    Logger.LogError("Shutdown",
                        "Tried to inform runner of a disconnect but could not reach the runner.");
                }
            }
            catch (Exception)
            {
                Logger.LogDebug("Shutdown", "Tried to inform runner of a disconnect but could not reach the runner.");
            }

            OnDisconnect(new Guid());
        }
    }
}