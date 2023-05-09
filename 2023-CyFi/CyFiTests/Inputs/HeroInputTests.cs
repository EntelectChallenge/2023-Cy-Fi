using CyFi.Entity;
using CyFi.Factories;
using CyFi.Inputs;
using CyFi.Models;
using CyFi.Physics.Movement;
using Domain.Enums;
using Domain.Objects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace CyFiTests.Inputs
{
    [TestFixture]
    internal class HeroInputTests
    {
        HeroInput heroInputUnderTest;

        HeroEntity heroEntity;

        CyFiGameSettings testSettings;
        WorldObject testWorld;

        WorldFactory WorldFactory;

        Guid Id = Guid.NewGuid();

        [SetUp]
        public void Setup()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .AddEnvironmentVariables()
                .Build();

            WorldFactory = new WorldFactory(new LoggerFactory());

            testSettings = config.GetSection("GameSettings").Get<CyFiGameSettings>();
            testWorld = WorldFactory.CreateWorld(testSettings.Levels.First(), 1);

            heroEntity = new HeroEntity(Id, testSettings);
            heroEntity.MovementSm.World = testWorld;
            heroInputUnderTest = (HeroInput)heroEntity.InputComponent;
        }

        [TestCase(InputCommand.LEFT, -1, 0)]
        [TestCase(InputCommand.RIGHT, 1, 0)]
        [TestCase(InputCommand.UP, 0, 1)]
        [TestCase(InputCommand.DOWN, 0, -1)]
        public void GivenInputCommand_ShouldUpdateProposedPosition(InputCommand inputCommand, int xPosition, int yPosition)
        {
            for (int x = 0; x < testWorld.width; x++)
            {
                for (int y = 0; y < testWorld.height; y++)
                {
                    testWorld.map[x][y] = (int)ObjectType.Air;
                }
            }
            // Add platform to stand on.
            int platformHeight = testWorld.height / 2;
            for (int x = 0; x < testWorld.width; x++)
            {
                testWorld.map[x][platformHeight] = (int)ObjectType.Platform;
            }

            // Add ladder to climb.
            int ladderX = testWorld.width / 2;
            for (int y = 0; y < testWorld.height; y++)
            {
                testWorld.map[ladderX][y] = (int)ObjectType.Ladder;
            }

            heroEntity.XPosition = ladderX;
            heroEntity.YPosition = platformHeight + 1;

            heroEntity.MovementSm.ChangeState(heroEntity.MovementSm.Idle);
            heroEntity.MovementSm.UpdateInput(inputCommand);
            Assert.AreEqual(xPosition, heroEntity.deltaX);
            Assert.AreEqual(yPosition, heroEntity.deltaY);
        }

        [Test]
        public void GivenUpCommand_ShouldJump()
        {
            for (int x = 0; x < testWorld.width; x++)
            {
                for (int y = 0; y < testWorld.height; y++)
                {
                    testWorld.map[x][y] = (int)ObjectType.Air;
                }
            }
            // Add platform to stand on.
            int platformHeight = testWorld.height / 2;
            for (int x = 0; x < testWorld.width; x++)
            {
                testWorld.map[x][platformHeight] = (int)ObjectType.Platform;
            }

            // Place hero on platform.
            heroEntity.XPosition = 0;
            heroEntity.YPosition = platformHeight + 1;

            heroEntity.MovementSm.UpdateInput(InputCommand.UP);
            Assert.AreEqual(typeof(Jumping), heroEntity.MovementSm.GetStateType());
        }
    }
}
