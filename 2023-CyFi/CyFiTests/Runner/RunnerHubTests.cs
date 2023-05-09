using CyFi;
using CyFi.Entity;
using CyFi.Factories;
using CyFi.Models;
using CyFi.Runner;
using Domain.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Runner.Services;

namespace CyFiTests.Runner
{
    [TestFixture]
    internal class RunnerHubTests
    {
        RunnerHub runnerHubUnderTest;

        IOptions<CyFiGameSettings> testSettings;
        Mock<ILogger<CyFiEngine>> mockEngineLogger;
        Mock<ILoggerFactory> mockLoggerFactory;
        Mock<BotFactory> mockBotFactory;
        Mock<WorldFactory> mockWorldFactory;
        Mock<Queue<BotCommand>> mockQueue;

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
                        Height = 10,
                        Width = 10,
                    }
                }

            });

            mockEngineLogger = new Mock<ILogger<CyFiEngine>>();
            mockLoggerFactory = new Mock<ILoggerFactory>();
            mockBotFactory = new Mock<BotFactory>(testSettings, mockLoggerFactory.Object);
            mockWorldFactory = new Mock<WorldFactory>(mockLoggerFactory.Object);
            mockQueue = new Mock<Queue<BotCommand>>();

            mockCloudIntegrationService = new Mock<ICloudIntegrationService>();
            mockLogger = new Mock<ILogger<RunnerHub>>();
            mockCaller = new Mock<IHubCallerClients>();
            mockClientProxy = new Mock<IClientProxy>();
            mockHubCallerContext = new Mock<HubCallerContext>();

            mockContext = new Mock<IHubContext<RunnerHub>>();

            mockCaller.Setup(x => x.Caller).Returns(mockClientProxy.Object);

            engine = new CyFiEngine(testSettings, mockContext.Object, mockQueue.Object, mockEngineLogger.Object, mockBotFactory.Object, mockWorldFactory.Object);

            runnerHubUnderTest = new RunnerHub(engine, mockCloudIntegrationService.Object, mockLogger.Object, mockContext.Object)
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

            mockBot.Object.Hero = new HeroEntity(Id, testSettings.Value);
            mockBotFactory.Setup(f => f.CreateBot("testBot", connectionId.ToString())).Returns(mockBot.Object);

            mockHubCallerContext.Setup(c => c.ConnectionId).Returns(connectionId.ToString());

            await runnerHubUnderTest.Register("testBot");

            mockCaller.Verify(clients => clients.Caller, Times.Once());

            mockClientProxy.Verify(clientProxy => clientProxy.SendCoreAsync("Registered", It.Is<object[]>(o => Guid.Parse(o[0].ToString()) == botId), default));
        }
    }
}
