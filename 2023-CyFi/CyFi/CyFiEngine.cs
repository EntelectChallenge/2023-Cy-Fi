using CyFi.Entity;
using CyFi.Factories;
using CyFi.Models;
using CyFi.RootState;
using CyFi.Runner;
using Domain.Exceptions;
using Domain.Models;
using Domain.Objects;
using Engine;
using Logger;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Python.Core;
using System.Drawing;
using System.Timers;
using static CyFi.Settings.GameSettings;
using Timer = System.Timers.Timer;

namespace CyFi
{
    public class CyFiEngine : GameEngine
    {
        public CyFiState cyFiState;

        public CyFiGameSettings GameSettings;

        private readonly BotFactory BotFactory;
        public static Timer TickTimer;
        public IGameLogger<CyFiEngine> Logger;

        public Queue<BotCommand> CommandQueue;

        public HubConnection hubConnection;
        public IHubContext<RunnerHub> context;

        private List<WorldObject> levels;

        public CyFiEngine(
            IOptions<CyFiGameSettings> settings,
            IHubContext<RunnerHub> context,
            Queue<BotCommand> CommandQueue, ILogger<CyFiEngine> Logger,
            BotFactory botFactory)
        {
            GameSettings = settings.Value;

            levels = new();
            for (int level = 0; level < GameSettings.Levels.Count; level++)
            {
                levels.Add(WorldFactory.CreateWorld(GameSettings.Levels[level], level));
            }

            cyFiState = new CyFiState(
                Levels: levels,
                Bots: new List<Bot>()
            );

            this.CommandQueue = CommandQueue;

            this.Logger = new GameLogger<CyFiEngine>(Logger);
            this.BotFactory = botFactory;

            this.context = context;
        }

        public HubConnection SetHubConnection(ref HubConnection connection) => hubConnection = connection;

        public Guid RegisterBot(string nickName, string connectionId)
        {
            if (cyFiState.Bots.Count < GameSettings.NumberOfPlayers)
            {
                Bot bot = BotFactory.CreateBot(nickName, connectionId);
                this.cyFiState.Bots.Add(bot);
                bot.CurrentLevel = 0;
                Point startPosition = cyFiState.Levels[bot.CurrentLevel].start;
                bot.Hero.XPosition = startPosition.X;
                bot.Hero.YPosition = startPosition.Y;
                bot.Hero.Start = DateTime.Now;

                // perhaps move this to somewhere else further down the line?
                bot.Hero.MovementSm.World = cyFiState.Levels.First();

                return bot.Id;
            }
            else
            {
                throw new BotCapacityReachedException("Game already full");
            }
        }

        public async Task StartGame()
        {
            if (hubConnection.State != HubConnectionState.Connected)
            {
                await hubConnection.StartAsync();
            }
            SetTimer(GameSettings.TickTimer);
            if (cyFiState.Bots.Count >= GameSettings.NumberOfPlayers)
            {
                try
                {
                    TickTimer.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception was thrown: " + ex.Message);
                }
            }
        }


        private void GameLoop()
        {
            //Send updated bot state

            Logger.Log(LogLevel.Information, $"Tick: {cyFiState.Tick} ************************************************* ");

            List<BotStateDTO> botStates = new List<BotStateDTO>();

            foreach (var bot in cyFiState.Bots)
            {
                botStates.Add(new BotStateDTO(bot, bot.Hero, cyFiState.Levels[bot.CurrentLevel]));
                Logger.Log(LogLevel.Information, $"bot States: X {bot.Hero.XPosition}, Y {bot.Hero.YPosition}");
            }

            hubConnection.InvokeAsync("PublishBotStates", botStates);

            Bot? playerObject = null;

            for (int i = 0; i < 3; i++)
            {
                if (!CommandQueue.IsNullOrEmpty()) // perhaps only process commands for a certain duration of time, after that process the physics updates. Do this instead of the tick timer
                {
                    // Get the first bot command in the queue
                    BotCommand playerAction = CommandQueue.Dequeue();

                    // Get the bot it belongs too
                    playerObject = cyFiState.Bots.FirstOrDefault((bot) => bot.Id.Equals(playerAction.BotId));

                    // If there is not bot, continue
                    if (playerObject == null)
                    {
                        Logger.Log(LogLevel.Error, $"Bot not found for ID {playerAction.BotId}");
                        return;
                    }

                    List<HeroEntity> otherPlayers = cyFiState.Bots.Where((bot) => bot.CurrentLevel == playerObject.CurrentLevel).Except(new List<Bot>() { playerObject }).Select((bot) => bot.Hero).ToList();

                    Logger.Log(LogLevel.Information, $"Bot: {playerObject.Id} send command {playerAction.Action}");

                    Console.WriteLine($"Current state of bot: {playerObject.Hero.MovementSm.currentState}");

                    // Update the bot based on the bot command and the movement state?

                    playerObject.Hero.UpdateInput(
                        playerAction
                    );

                    if (playerObject.Hero.TimesDug >= collectibleDigCount)
                    {
                        playerObject.Hero.TimesDug = 0;
                        playerObject.Hero.Collected++;
                    }

                    int numOnLevel = cyFiState.Bots.Count((bot) => bot.CurrentLevel == playerObject.CurrentLevel);
                    if (playerObject.Hero.Collected >= collectables[playerObject.CurrentLevel])
                    {
                        AdvanceToLevel(playerObject);
                        playerObject.Hero.Collected = 0;
                    }
                }
            }

            if (TickTimer.Enabled)
            {
                cyFiState.Update();
                cyFiState.Tick++;
                CommandQueue.Clear();
            }
        }

        public void AdvanceToLevel(Bot bot)
        {
            bot.CurrentLevel++;
            if (bot.CurrentLevel < GameSettings.Levels.Count)
            {
                bot.TotalPoints += bot.Hero.Collected;
                bot.Hero.Collected = 0;
                Point startPosition = cyFiState.Levels[bot.CurrentLevel].start;
                bot.Hero.XPosition = startPosition.X;
                bot.Hero.YPosition = startPosition.Y;

                bot.Hero.MovementSm.World = cyFiState.Levels[bot.CurrentLevel];
            }
            else
            {
                foreach (Bot otherBot in cyFiState.Bots.Except(new[] { bot }))
                {
                    otherBot.TotalPoints += otherBot.Hero.Collected;
                }
                bot.TotalPoints += bot.Hero.Collected;
                EndGame();
            }
        }

        private void EndGame()
        {
            //Disconnect all bots
            hubConnection.InvokeAsync("GameComplete");
            TickTimer.Stop();
            EndGame();
        }

        private void SetTimer(int timeLimit)
        {
            // Create a timer with a givin interval
            TickTimer = new Timer(timeLimit);
            // Hook up the Elapsed event for the timer. 
            TickTimer.Elapsed += OnTimedEvent;
            TickTimer.AutoReset = true;
        }



        private void OnTimedEvent(object? sender, ElapsedEventArgs e)
        {
            Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);
            GameLoop();
        }

        internal bool HasBotMoved(BotCommand command)
        {
            return CommandQueue.Contains(command);
        }
    }
}
