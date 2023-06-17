using CyFi.RootState;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using Logger;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Runner.Services;

namespace CyFi.Runner
{
    public class RunnerHub : Hub
    {
        private readonly CyFiEngine engine;
        private readonly ICloudIntegrationService _cloudIntegrationService;
        private readonly IGameLogger<RunnerHub> _logger;
        private StateObject visualizer;

        public RunnerHub(CyFiEngine engine, ICloudIntegrationService cloudIntegrationService, ILogger<RunnerHub> logger)
        {
            this.engine = engine;
            _cloudIntegrationService = cloudIntegrationService;
            _logger = new GameLogger<RunnerHub>(logger);
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
                _logger.ConsoleL(LogLevel.Debug, "New Connection");
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
            _logger.ConsoleL(LogLevel.Debug, exception?.Message, exception);
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
        public async Task GameComplete(int? seed, int? ticks)
        {
            Console.WriteLine("Announcing Game Completed");
            _logger.ConsoleL(LogLevel.Information, "Game Complete");

            await Clients.All.SendAsync("ReceiveGameComplete");
            await _cloudIntegrationService.Announce(CloudCallbackType.Finished, seed: seed, ticks: ticks);

            await _logger.FlushToS3(!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PUSH_LOGS_TO_S3")));
            await _cloudIntegrationService.Announce(CloudCallbackType.LoggingComplete, null, null);
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

            //Check if bot has already had a command lined up on the queue
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
                await _cloudIntegrationService.Announce(CloudCallbackType.Started);

                engine.StartGame();
            }
        }
        #endregion
    }
}