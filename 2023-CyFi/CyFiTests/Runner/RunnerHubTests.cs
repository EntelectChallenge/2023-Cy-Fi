using CyFi;
using CyFi.Entity;
using CyFi.Factories;
using CyFi.Models;
using CyFi.RootState;
using CyFi.Runner;
using Domain.Components;
using Domain.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Runner.Services;
using System.Collections.Concurrent;

namespace CyFiTests.Runner
{
    [TestFixture]
    internal class RunnerHubTests
    {
        RunnerHub runnerHubUnderTest;

        IOptions<CyFiGameSettings> testSettings;
        Mock<ILogger<CyFiEngine>> mockEngineLogger;
        Mock<ILogger<CyFiState>> mockStateLogger;
        Mock<ILogger<GameComplete>> mockGameCompleteLogger;
        Mock<ILoggerFactory> mockLoggerFactory;
        Mock<BotFactory> mockBotFactory;
        Mock<WorldFactory> mockWorldFactory;
        Mock<ConcurrentQueue<BotCommand>> mockQueue;

        CyFiEngine engine;
        Mock<ICloudIntegrationService> mockCloudIntegrationService;
        Mock<ILogger<RunnerHub>> mockLogger;
        Mock<IHubCallerClients> mockCaller;
        Mock<IClientProxy> mockClientProxy;
        Mock<HubCallerContext> mockHubCallerContext;
        Mock<IHubContext<RunnerHub>> mockContext;

        Guid Id = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {

            testSettings = Options.Create<CyFiGameSettings>(new()
            {
                Levels = new List<Map>()
                {
                    new Map()
                    {
                        Seed = 12345,
                        Height = 500,
                        Width = 200,
                    }
                },
                TickTimer = 100,
                NumberOfPlayers = 4
            });

            mockEngineLogger = new Mock<ILogger<CyFiEngine>>();
            mockStateLogger = new Mock<ILogger<CyFiState>>();
            mockGameCompleteLogger = new Mock<ILogger<GameComplete>>();
            mockLoggerFactory = new Mock<ILoggerFactory>();
            mockBotFactory = new Mock<BotFactory>(testSettings, mockLoggerFactory.Object);
            mockWorldFactory = new Mock<WorldFactory>(mockLoggerFactory.Object);
            mockQueue = new Mock<ConcurrentQueue<BotCommand>>();

            mockCloudIntegrationService = new Mock<ICloudIntegrationService>();
            mockLogger = new Mock<ILogger<RunnerHub>>();
            mockCaller = new Mock<IHubCallerClients>();
            mockClientProxy = new Mock<IClientProxy>();
            mockHubCallerContext = new Mock<HubCallerContext>();

            mockContext = new Mock<IHubContext<RunnerHub>>();

            mockCaller.Setup(x => x.Caller).Returns(mockClientProxy.Object);

            engine = new CyFiEngine(testSettings, mockContext.Object, mockQueue.Object, mockEngineLogger.Object, mockStateLogger.Object, mockGameCompleteLogger.Object, mockBotFactory.Object, mockWorldFactory.Object, mockCloudIntegrationService.Object);

            runnerHubUnderTest = new RunnerHub(engine, mockCloudIntegrationService.Object, mockLogger.Object)
            {
                Clients = mockCaller.Object,
                Context = mockHubCallerContext.Object,
            };
        }

        [Test]
        public async Task GivenBotLimitNotReached_OnRegister_ShouldRegisterSuccessfully()
        {
            var botId = Guid.NewGuid();
            var connectionId = Guid.NewGuid();


            var mockBot = new Mock<Bot>();
            mockBot.Object.Id = botId;
            mockBot.Object.NickName = "testBot";

            mockBot.Object.Hero = new HeroEntity(Id);
            mockBotFactory.Setup(f => f.CreateBot(botId, "testBot", connectionId.ToString())).Returns(mockBot.Object);

            mockHubCallerContext.Setup(c => c.ConnectionId).Returns(connectionId.ToString());

            await runnerHubUnderTest.Register(botId, "testBot");

            mockCaller.Verify(clients => clients.Caller, Times.Once());

            mockClientProxy.Verify(clientProxy => clientProxy.SendCoreAsync("Registered", It.Is<object[]>(o => Guid.Parse(o[0].ToString()) == botId), default));
        }
    }
}
