using CyFi.Entity;
using CyFi.Models;
using CyFi.Settings;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System.Drawing;

namespace CyFiTests.Entity
{
    [TestFixture]
    internal class HeroEntityTests
    {
        HeroEntity heroEntityUnderTest;
        CyFiGameSettings testSettings;
        Guid Id = Guid.NewGuid();

        [SetUp]
        public void Setup()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .AddEnvironmentVariables()
                .Build();

            testSettings = config.Get<CyFiGameSettings>();

            heroEntityUnderTest = new HeroEntity(Id);
        }

        [Test]
        public void GivenHero_ReturnCorrectHeroWindow()
        {
            Point[] heroWindow = heroEntityUnderTest.HeroWindow();
            var windowSizeX = GameSettings.heroWindowSizeX;
            var windowSizeY = GameSettings.heroWindowSizeY;

            var expectedWidth = (heroEntityUnderTest.Width + 1) + (2 * windowSizeX);
            var expectedHeight = (heroEntityUnderTest.Height + 1) + (2 * windowSizeY);

            var bottomLeft = heroWindow[0];
            var topRight = heroWindow[1];

            Assert.AreEqual(expectedWidth, topRight.X - bottomLeft.X);
            Assert.AreEqual(expectedHeight, topRight.Y - bottomLeft.Y);
        }
    }
}
