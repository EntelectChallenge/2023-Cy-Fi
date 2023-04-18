using CyFi;
using CyFi.Entity;
using CyFi.Models;
using CyFi.Runner;
using Domain.Components;
using Domain.Enums;
using Domain.Models;
using Domain.Objects;
using Logger;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System.Diagnostics;

namespace CyFiTests
{
    [TestFixture]
    internal class CyFiEngineTests
    {
        IOptions<CyFiGameSettings> testSettings;
        Mock<IHubContext<RunnerHub>> mockContext;

        CyFiEngine cyFiEngineUnderTest;
        Bot testBot;

        Queue<BotCommand> botCommandQueue;

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

            botCommandQueue = new Queue<BotCommand>();

            ILogger<CyFiEngine> engineNullLogger = new NullLogger<CyFiEngine>();
            cyFiEngineUnderTest = new(testSettings, mockContext.Object, botCommandQueue, engineNullLogger, null);

            ILogger<Bot> botNullLogger = new NullLogger<Bot>();
            testBot = new(testSettings.Value, botNullLogger, null, "123");
            cyFiEngineUnderTest.cyFiState.Bots.Add(testBot);
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
        public async Task GivenGameEngine_VerifyAdvanceToNextLevel()
        {
            Bot testBot2 = new(testSettings.Value, new NullLogger<Bot>(), null, "1234");
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
            Assert.AreEqual(15, testBot2.TotalPoints);
            Assert.IsNotNull(testBot.Hero.End);
        }
    }
}