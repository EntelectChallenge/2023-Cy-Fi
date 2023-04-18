using CyFi.RootState;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using Logger;
using System.Windows;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Mono.Unix.Native;
using Newtonsoft.Json;
using Runner.Services;
using System;
using static IronPython.SQLite.PythonSQLite;

namespace CyFi.Runner
{
    public class RunnerHub : Hub
    {
        private readonly CyFiEngine engine;
        private readonly ICloudIntegrationService _cloudIntegrationService;
        private readonly IGameLogger<RunnerHub> _logger;
        private readonly IHubContext<RunnerHub> context;
        private StateObject visualizer;

        public RunnerHub(CyFiEngine engine, ICloudIntegrationService cloudIntegrationService, ILogger<RunnerHub> logger, IHubContext<RunnerHub> context)
        {
            this.engine = engine;
            _cloudIntegrationService = cloudIntegrationService;
            _logger = new GameLogger<RunnerHub>(logger);
            context = context;
        }

        #region Runner endpoints

        /// <summary>
        ///     New client connected
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            try
            {
                _logger.Console(LogLevel.Debug, "New Connection");
                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnConnectedAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// De-regsiter a bot on disconnect
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.Console(LogLevel.Debug, exception?.Message, exception);
            //TODO: Implement any game specific logic to remove a bot
            return base.OnDisconnectedAsync(exception);
        }

        #endregion

        #region Game Engine endpoints

        /// <summary>
        /// Publish GameObject state
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        /// 
        public async Task PublishBotStates(List<BotStateDTO> botStates)
        {
            foreach (var botState in botStates)
            {
                await Clients.Client(botState.ConnectionId).SendAsync("ReceiveBotState", botState);
            }
        }

        /// <summary>
        /// When the game is complete.
        /// </summary>
        /// <returns></returns>
        public async Task GameComplete()
        {
            _logger.Console(LogLevel.Information, "Game Complete");



            await Clients.All.SendAsync("ReceiveGameComplete");
            await _cloudIntegrationService.Announce(CloudCallbackType.Finished);

        }

        #endregion

        #region Bot endpoints
        /// <summary>
        /// Allows bot to register for game with given token
        /// </summary>
        /// <param name="nickName"></param>
        /// <returns></returns>
        public async Task Register(string nickName)
        {
            _logger.Log(LogLevel.Information, $"Registering Bot with nickname: {nickName}");
            try
            {
                Guid botId = engine.RegisterBot(nickName, Context.ConnectionId);
                _logger.Log(LogLevel.Debug, $"Successfully registered bot with nickname {nickName} and id {botId}");

                await Clients.Caller.SendAsync("Registered", botId);
                await CheckStartConditions();
            }
            catch (BotCapacityReachedException ex)
            {
                _logger.Log(LogLevel.Information, ex.Message);
                await Clients.Caller.SendAsync("Disconnect", ex.Message);
                return;
            }
        }

        /// <summary>
        ///     Allow bots to send actions to Engine
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task SendPlayerCommand(BotCommand command)
        {
            if (command == null)
            {
                return;
            }

            //Check if bot has already sent a command within the given tick
            if (!engine.HasBotMoved(command))
            {
                engine.CommandQueue.Enqueue(command);
            }
            else
            {
                Console.WriteLine($"{command.BotId} has moved for this tick");
            }
        }
        #endregion

        # region Visualizer Endpoints
        /// <summary>
        ///     Register game engine on runner.
        /// </summary>
        /// <returns></returns>
        public async Task RegisterVisualizer()
        {
            visualizer = new StateObject()
            {
                ConnectionId = Context.ConnectionId,
                Client = Clients.Client(Context.ConnectionId)
            };
        }

        public void VisualizeGame(CyFiState gameState)
        {
            visualizer.Client.SendAsync("PlotGame", gameState);
        }

        #endregion

        #region Private methods

        private async Task CheckStartConditions()
        {
            if (engine.cyFiState.Bots.Count == engine.GameSettings.NumberOfPlayers)
            {
                await _cloudIntegrationService.AnnounceNoOp(CloudCallbackType.Started);

                //Task.Factory.StartNew(() => engine.StartGame());
                engine.StartGame();
                //TODO: Do we want to let all clients know the game has started?

            }
        }
        #endregion
    }
}