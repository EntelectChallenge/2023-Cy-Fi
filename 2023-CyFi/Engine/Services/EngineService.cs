using Domain.Configs;
using Engine.Game;
using Engine.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;

namespace Engine.Services
{
    public class EngineService// : IEngineService
    {
        private readonly EngineConfig engineConfig;
        private readonly GameLoop gameLoop;
        private HubConnection hubConnection;
        private int currentTick;
        private readonly EngineState engineState;
        public int TickAcknowledged { get; set; }
        public bool HasWinner
        {
            set { engineState.HasWinner = value; }
            get { return engineState.HasWinner; }
        }

        public bool PendingStart
        {
            set { engineState.PendingStart = value; }
            get { return engineState.PendingStart; }
        }

        public bool GameStarted
        {
            set { engineState.GameStarted = value; }
            get { return engineState.GameStarted; }
        }

        public EngineService(
            IConfigurationService engineConfig,
            EngineState engineState,
            GameLoop gameLoop)
        {
            this.engineConfig = engineConfig.Value;
            this.gameLoop = gameLoop;
            this.engineState = engineState;
        }

        public HubConnection SetHubConnection(ref HubConnection connection) => hubConnection = connection;

        public async Task RunGameLoop()
        {
            // -------- START STOPWATCH RULE
            var stopwatch = Stopwatch.StartNew();
            var stop2 = Stopwatch.StartNew();

            gameLoop.Setup();

            WaitForGameStart();

            await hubConnection.InvokeAsync("NotifyOfStartGame");

            do
            {
                currentTick++;

                // -------- STOPWATCH RESTART RULE
                stop2.Restart();

                HasWinner = gameLoop.Run();

                Logger.LogDebug("RunLoop", $"Processing tick took {stop2.ElapsedMilliseconds}ms");

                stop2.Restart();

                // SEND UPDATED STATE EVENT RULE
                await hubConnection.InvokeAsync("TickEnded", currentTick);

                // // PUBLISH GAME STATE
                // var gameStateDto = worldStateService.GetPublishedState();
                // await hubConnection.InvokeAsync("PublishGameState", gameStateDto);
                // Logger.LogDebug("RunLoop", $"Published game state, Time: {stop2.ElapsedMilliseconds}");


                // INFRASTRUCTURE - Wait until the game runner has processed the current tick
                Logger.LogDebug("RunLoop", "Waiting for Tick Ack");
                stop2.Restart();
                while (TickAcknowledged != currentTick) { }
                Logger.LogDebug("RunLoop", $"TickAck matches current tick, Time: {stop2.ElapsedMilliseconds}");

                // ENFORCE MINIMUM TICK DURATION RULE
                if (stopwatch.ElapsedMilliseconds < engineConfig.TickRate)
                {
                    var delay = (int)(engineConfig.TickRate - stopwatch.ElapsedMilliseconds);
                    if (delay > 0)
                    {
                        Thread.Sleep(delay);
                    }
                }

                Logger.LogInfo("TIMER", $"Game Loop Time: {stopwatch.ElapsedMilliseconds}ms");
                stopwatch.Restart();
            } while (!HasWinner &&
                     hubConnection.State == HubConnectionState.Connected);

            if (!HasWinner &&
                hubConnection.State != HubConnectionState.Connected)
            {
                Logger.LogError("RunLoop", "Runner disconnected before a winner was found");
                throw new InvalidOperationException("Runner disconnected before a winner was found");
            }

            gameLoop.Finish();

            // await hubConnection.InvokeAsync("GameComplete", worldStateService.GenerateGameCompletePayload());
        }

        private void WaitForGameStart()
        {
            while (!GameStarted)
            {
                // -------- ONLY START IF CONNECTED RULE
                if (hubConnection.State != HubConnectionState.Connected)
                {
                    throw new Exception("Can only start game if connected to runner");
                }

                // -------- WAIT FOR GAME START RULE
                if (GameStarted) continue;
                if (!PendingStart)
                {
                    Logger.LogInfo("Core", "Waiting for all bots to connect");
                }

                Thread.Sleep(1000);
            }
        }
    }
}
