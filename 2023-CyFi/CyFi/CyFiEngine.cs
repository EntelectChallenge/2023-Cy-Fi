using CyFi.Entity;
using CyFi.Factories;
using CyFi.Models;
using CyFi.RootState;
using CyFi.Runner;
using Domain.Components;
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
using Bot = CyFi.Entity.Bot;
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
            Queue<BotCommand> CommandQueue,
            ILogger<CyFiEngine> Logger,
            BotFactory botFactory,
            WorldFactory worldFactory
            )
        {
            GameSettings = settings.Value;

            levels = new();
            for (int level = 0; level < GameSettings.Levels.Count; level++)
            {
                levels.Add(worldFactory.CreateWorld(GameSettings.Levels[level], level));
            }

            cyFiState = new CyFiState(
                Levels: levels,
                Bots: new List<Bot>(),
                Logger
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
                Point startPosition = cyFiState.Levels[bot.CurrentLevel].start;
                bot.Hero.XPosition = startPosition.X;
                bot.Hero.YPosition = startPosition.Y;
                bot.Hero.Start = DateTime.Now;
                bot.CurrentLevel = 0;
                // perhaps move this to somewhere else further down the line?
                bot.Hero.MovementSm.World = cyFiState.Levels.First();

                this.cyFiState.Bots.Add(bot);

                if (cyFiState.Bots.Count == GameSettings.NumberOfPlayers)
                {
                    IGameLogger<CyFiState>.File(cyFiState, 0);
                }

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

                var oppositionBotsOnSameLevel = cyFiState.Bots.Except(new List<Bot> { bot }).Where(b => b.CurrentLevel == bot.CurrentLevel).ToList();

                botStates.Add(new BotStateDTO(bot, oppositionBotsOnSameLevel, bot.Hero, cyFiState.Levels[bot.CurrentLevel]));
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

                    Console.WriteLine($"Current state of bot: {playerObject.Hero.MovementSm.CurrentState}");

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


                    if (playerObject.Hero.Collected >= GameSettings.Collectables[playerObject.CurrentLevel])
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

                //Format state
                var state = new CyFiState()
                {
                    Tick = cyFiState.Tick,
                    Bots = cyFiState.Bots
                };

                cyFiState.Levels.ForEach(level => state.Levels.Add(new WorldObject(level.ChangeLog)));

                IGameLogger<CyFiState>.File(state, 1);
            }
        }

        public void AdvanceToLevel(Bot bot)
        {
            if (bot.CurrentLevel < GameSettings.Levels.Count -1)
            {                

                bot.CurrentLevel++;
                bot.TotalPoints += bot.Hero.Collected;
                bot.Hero.Collected = 0;
                Point startPosition = cyFiState.Levels[bot.CurrentLevel].start;
                bot.Hero.XPosition = startPosition.X;
                bot.Hero.YPosition = startPosition.Y;

                //TallyPoints
                foreach (Bot otherBot in cyFiState.Bots.Except(new[] { bot }))
                {
                    //Override to log change
                    otherBot.TotalPoints += (5 * otherBot.Hero.Collected);
                }
                bot.TotalPoints += bot.Hero.Collected;

                bot.Hero.MovementSm.World = cyFiState.Levels[bot.CurrentLevel];

            }
            else
            {

                TickTimer.Stop();
                bot.TotalPoints += 20;
                IGameLogger<CyFiState>.File(null, 2);
                EndGame();
            }

            if (cyFiState.Tick > GameSettings.MaxTicks)
            {
                GracefulShutdown();
            }
        }

        private void EndGame()
        {

            var rankedBots = cyFiState.Bots.OrderBy(bot => bot.TotalPoints);


            var gameComplete = new GameComplete
            {
                TotalTicks = cyFiState.Tick,
                Players = rankedBots.Select((bot, index) =>
                    new PlayerResult
                    {
                        Placement = index + 1,
                        Score = bot.TotalPoints,
                        Id = bot.Id.ToString(),
                        Nickname = bot.NickName,
                        MatchPoints = (cyFiState.Bots.Count - (index + 1)) * 2
                    }
                ).ToList(),
                WorldSeeds = GameSettings.Levels.Select(l => l.Seed).ToList(),
                WinngingBot = rankedBots.First()
            };


            IGameLogger<GameComplete>.File(gameComplete, 5, "GameComplete");
            //Disconnect all bots
            hubConnection.InvokeAsync("GameComplete", gameComplete);
        }

        private void GracefulShutdown()
        {
            //TODO implement
            //Disconnect all bots
            hubConnection.InvokeAsync("EndGame");
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
