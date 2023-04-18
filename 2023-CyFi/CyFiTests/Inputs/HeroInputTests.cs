using CyFi.Entity;
using Domain.Enums;
using CyFi.Inputs;
using NUnit.Framework;
using CyFi.Models;
using Microsoft.Extensions.Configuration;

namespace CyFiTests.Inputs
{
    [TestFixture]
    internal class HeroInputTests
    {
        HeroInput heroInputUnderTest;

        HeroEntity heroEntity;

        CyFiGameSettings testSettings;

        [SetUp]
        public void Setup()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .AddEnvironmentVariables()
                .Build();

            testSettings = config.Get<CyFiGameSettings>();

            heroEntity = new HeroEntity(testSettings);
            heroInputUnderTest = (HeroInput)heroEntity.InputComponent;
        }

        [TestCase(InputCommand.LEFT, -1, 0)]
        [TestCase(InputCommand.RIGHT, 1, 0)]
        [TestCase(InputCommand.UP, 0, 1)]
        [TestCase(InputCommand.DOWN, 0, -1)]
        public void GivenInputCommand_ShouldUpdateProposedPosition(InputCommand inputCommand, int xPosition, int yPosition)
        {
            heroInputUnderTest.Update(heroEntity, inputCommand);
            Assert.AreEqual(heroEntity.deltaX, xPosition);
            Assert.AreEqual(heroEntity.deltaY, yPosition);
        }
    }
}
