using Domain.Objects;
using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;

namespace DomainTests.Objects
{
    [TestFixture]
    internal class WorldObjectTests
    {
        private WorldObject worldObjectUnderTest;

        private readonly int width = 20;
        private readonly int height = 20;
        private readonly string seed = "seed";
        private readonly int fillThreshold = 50;
        private readonly float minPathWidth = 0.025f;
        private readonly float maxPathWidth = 0.115f;
        private readonly float minPathHeight = 0.065f;
        private readonly float maxPathHeight = 0.13f;
        private readonly float pathCleanupWidth = 0.015f;
        private readonly int minimumConnections = 2;
        private readonly int maximumConnections = 3;
        private readonly int level = 1;
        private readonly int numPaths = 5;


        public Mock<ILogger<WorldObject>> mockWorldLogger;

        [SetUp]
        public void Setup()
        {
           mockWorldLogger = new Mock<ILogger<WorldObject>>();

            worldObjectUnderTest = new WorldObject(
                width,
                height,
                seed,
                fillThreshold,
                minPathWidth,
                maxPathWidth,
                minPathHeight,
                maxPathHeight,
                pathCleanupWidth,
                level,
                minimumConnections,
                maximumConnections,
                mockWorldLogger.Object
                );
        }

        [Test]
        public void TestMapSize()
        {
            Assert.AreEqual(worldObjectUnderTest?.map.Length, width);
            Assert.AreEqual(worldObjectUnderTest?.map.GetLength(0), height);
        }

        [Test]
        public void Testlevel()
        {
            int minimumTotalConnections = (numPaths - 1) * minimumConnections;
            int maximumTotalConnections = (numPaths - 1) * maximumConnections;
            int minimumTotalPaths = numPaths + minimumTotalConnections;
            int maximumTotalPaths = numPaths + maximumTotalConnections;

            int? numberOfPathsGenerated = worldObjectUnderTest?.randomPaths.Count;
            Assert.GreaterOrEqual(numberOfPathsGenerated, minimumTotalPaths);
            Assert.LessOrEqual(numberOfPathsGenerated, maximumTotalPaths);
        }

        [Test]
        public void TestFillThreshold()
        {
            float sum = worldObjectUnderTest.map.SelectMany(item => item).Sum();
            Assert.IsTrue(sum / (width * height) < fillThreshold / 100f);
        }

        [Test]
        public void TestDeterminism()
        {
            WorldObject worldObjectUnderTest2 = new WorldObject(
                width,
                height,
                seed,
                fillThreshold,
                minPathWidth,
                maxPathWidth,
                minPathHeight,
                maxPathHeight,
                pathCleanupWidth,
                level,
                minimumConnections,
                maximumConnections,
                mockWorldLogger.Object);

            Assert.AreEqual(worldObjectUnderTest.map.Length, worldObjectUnderTest2.map.Length);
            for (int x = 0; x < worldObjectUnderTest.map.Length; x++)
            {
                Assert.IsTrue(worldObjectUnderTest.map[x].SequenceEqual(worldObjectUnderTest2.map[x]));
            }
        }
    }
}
