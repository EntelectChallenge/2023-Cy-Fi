using CyFi;
using CyFi.Entity;
using CyFi.Factories;
using CyFi.Models;
using CyFi.RootState;
using CyFi.Runner;
using Domain.Components;
using Domain.Enums;
using Domain.Models;
using Logger;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Runner.Factories;
using Runner.Services;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace CyFiTests
{
    [TestFixture]
    internal class CyFiEngineTests
    {
        IOptions<CyFiGameSettings> testSettings;
        private Mock<IHubContext<RunnerHub>> mockContext;
        Mock<WorldFactory> mockWorldFactory;
        Mock<CloudCallbackFactory> mockCloudCallbackFactory;
        Mock<CloudIntegrationService> mockCloudIntegrationService;
        Mock<ILoggerFactory> mockLoggerFactory;
        HubConnection hubConnection;

        CyFiEngine cyFiEngineUnderTest;
        Bot testBot;

        ConcurrentQueue<BotCommand> botCommandQueue;

        ILogger<CyFiEngine> engineNullLogger = new NullLogger<CyFiEngine>();
        ILogger<CyFiState> stateNullLogger = new NullLogger<CyFiState>();
        ILogger<GameComplete> gameCompleteNullLogger = new NullLogger<GameComplete>();

        [SetUp]
        public void Setup()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .AddEnvironmentVariables()
                .Build();

            var cyFiGameSettings = new CyFiGameSettings();
            config.GetSection("GameSettings").Bind(cyFiGameSettings);

            testSettings = Options.Create(cyFiGameSettings);

            mockContext = new Mock<IHubContext<RunnerHub>>();
            mockLoggerFactory = new Mock<ILoggerFactory>();
            mockWorldFactory = new Mock<WorldFactory>(mockLoggerFactory.Object);
            mockCloudCallbackFactory = new Mock<CloudCallbackFactory>(new AppSettings());
            mockCloudIntegrationService = new Mock<CloudIntegrationService>(new AppSettings(), new NullLogger<CloudIntegrationService>(), mockCloudCallbackFactory.Object);
            hubConnection = new HubConnectionBuilder().WithUrl("http://127.0.0.1:5000/runnerhub").Build();

            botCommandQueue = new ConcurrentQueue<BotCommand>();
            
            cyFiEngineUnderTest = new(testSettings, mockContext.Object, botCommandQueue, engineNullLogger, stateNullLogger, gameCompleteNullLogger, null, mockWorldFactory.Object, mockCloudIntegrationService.Object);

            ILogger<Bot> botNullLogger = new NullLogger<Bot>();
            testBot = new(botNullLogger, "testBot", "123");
            cyFiEngineUnderTest.cyFiState.Bots.Add(testBot);

            cyFiEngineUnderTest.SetHubConnection(ref hubConnection);
        }

        [Test]
        public void GivenGameSettings_VerifyConstructor()
        {
            //   Assert.AreEqual(cyFiEngineUnderTest.GameState.Seed, testSettings.Value.Map.Seed);
            //  Assert.IsTrue(cyFiEngineUnderTest.GameState.Levels.All((level) => level.map.Length.Equals(testSettings.Value.Map.Width)));
            //  Assert.IsTrue(cyFiEngineUnderTest.GameState.Levels.All((level) => level.map.GetLength(0).Equals(testSettings.Value.Map.Height)));
        }

        [Test]
        public async Task GivenGameEngine_VerifyProcessesPerMilliSecond()
        {
            var test = InputCommand.RIGHT;

            //  CyFiCommand mockCommand = new CyFiCommand(new Guid(), ));
            for (int i = 0; i < 3; i++)
            {
                //     botCommandQueue.Enqueue(mockCommand);
            }

            Stopwatch timer = Stopwatch.StartNew();
            //   await cyFiEngineUnderTest.GameLoop();
            timer.Stop();
            Assert.LessOrEqual(timer.ElapsedMilliseconds, 6);
            Console.WriteLine(Tracker.SerializeStateChanges());
        }

        [Test]
        public async Task GivenGameEngine_VerifyCorrectTotalPoints_AtGameEnd()
        {
            Mock<IGameLogger<GameComplete>> mockGameCompleteLogger = new();
            cyFiEngineUnderTest.GameCompleteLogger = mockGameCompleteLogger.Object;

            Bot testBot2 = new(new NullLogger<Bot>(), "testBot2", "1234");
            cyFiEngineUnderTest.cyFiState.Bots.Add(testBot2);
            for (int i = 1;i <= 3;i++)
            {
                testBot.Hero.Collected = 20;
                Assert.AreEqual(i-1, testBot.CurrentLevel);
                cyFiEngineUnderTest.AdvanceToLevel(testBot);
                Assert.AreEqual(0, testBot.Hero.Collected);
                Assert.AreEqual(i*20, testBot.TotalPoints);
                Assert.AreEqual(i, testBot.CurrentLevel);
                Assert.AreEqual(0, testBot2.CurrentLevel);
                testBot2.Hero.Collected += 15;
                Assert.AreEqual(0, testBot2.TotalPoints);
            }
            cyFiEngineUnderTest.AdvanceToLevel(testBot);

            mockGameCompleteLogger.Verify(logger => logger.File(It.Is<GameComplete>(gc => 
            gc.WinngingBot.NickName == "testBot" && gc.WinngingBot.TotalPoints == 80), null, "GameComplete"), Times.Once);
        }

        [Test]
        public async Task GivenGameEngine_VerifyAdvanceToNextLevel()
        {
            Bot testBot2 = new(new NullLogger<Bot>(), null, "1234");
            cyFiEngineUnderTest.cyFiState.Bots.Add(testBot2);
            testBot.Hero.Collected = 20;
            Assert.AreEqual(0, testBot.CurrentLevel);
            cyFiEngineUnderTest.AdvanceToLevel(testBot);
            Assert.AreEqual(0, testBot.Hero.Collected);
            Assert.AreEqual(20, testBot.TotalPoints);
            Assert.AreEqual(1, testBot.CurrentLevel);
            Assert.AreEqual(0, testBot2.CurrentLevel);
            testBot.Hero.Collected = 10;
            testBot2.Hero.Collected = 15;
            cyFiEngineUnderTest.AdvanceToLevel(testBot);
            Assert.AreEqual(30, testBot.TotalPoints);
            Assert.AreEqual(15, testBot2.Hero.Collected);
        }

        [Test]
        public async Task GivenGameEngine_VerifyReducingCollectableRequirement_WhenAdvance()
        {
            // Arrange
            Bot testBot2 = new(new NullLogger<Bot>(), "testBot2", "1234");
            cyFiEngineUnderTest.cyFiState.Bots.Add(testBot2);
            cyFiEngineUnderTest.CommandQueue.Enqueue(new() { Action = InputCommand.None, BotId = testBot.Id });

            // Act
            testBot.Hero.Collected = 20;
            cyFiEngineUnderTest.GameLoop();

            // Assert
            Assert.AreEqual(1, testBot.CurrentLevel);
            Assert.AreEqual(0, testBot2.CurrentLevel);

            cyFiEngineUnderTest.CommandQueue.Enqueue(new() { Action = InputCommand.None, BotId = testBot.Id });

            // Act
            testBot.Hero.Collected = 20;
            cyFiEngineUnderTest.GameLoop();

            // Assert
            Assert.AreEqual(2, testBot.CurrentLevel);
            Assert.AreEqual(0, testBot2.CurrentLevel);

            // Act
            cyFiEngineUnderTest.CommandQueue.Enqueue(new() { Action = InputCommand.None, BotId = testBot2.Id });
            testBot2.Hero.Collected = 10;
            cyFiEngineUnderTest.GameLoop();

            // Assert
            Assert.AreEqual(2, testBot.CurrentLevel);
            Assert.AreEqual(1, testBot2.CurrentLevel);
        }
    }
}