using CyFi.Entity;
using CyFi.Factories;
using CyFi.Models;
using CyFi.Physics;
using CyFi.Physics.Movement;
using Domain.Enums;
using Domain.Objects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Drawing;

namespace CyFiTests.Physics
{
    [TestFixture]
    internal class HeroPhysicsTests
    {
        HeroPhysics heroPhysicsUnderTest;

        HeroEntity heroEntity;

        CyFiGameSettings testSettings;

        WorldObject testWorld;

        List<HeroEntity> otherPlayers;

        Mock<ILoggerFactory> mockLoggerFactory;

        Guid Id = Guid.NewGuid();

        WorldFactory WorldFactory;

        [SetUp]
        public void Setup()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .AddEnvironmentVariables()
                .Build();

            mockLoggerFactory = new Mock<ILoggerFactory>();
            WorldFactory = new WorldFactory(mockLoggerFactory.Object);
            testSettings = config.GetSection("GameSettings").Get<CyFiGameSettings>();

            testWorld = WorldFactory.CreateWorld(testSettings.Levels.First(), 1);
            testWorld.map = new int[5][];

            for (int x = 0; x < 4; x++)
            {
                testWorld.map[x] = new int[2] { (int)ObjectType.Air, (int)ObjectType.Air };
            }
            testWorld.map[4] = new int[2] { (int)ObjectType.Solid, (int)ObjectType.Air };

            heroEntity = new HeroEntity(Id);
            otherPlayers = new List<HeroEntity>();
            heroPhysicsUnderTest = (HeroPhysics)heroEntity.PhysicsComponent;

            heroEntity.MovementSm.World = testWorld;
            heroEntity.MovementSm.CollidableObjects = otherPlayers;
        }

        [TestCase(-1, 0)] // LEFT
        [TestCase(0, -1)] // DOWN
        public void GivenOutOfBoundsMovement_ShouldNotMoveHero(int nextXPosition, int nextYPosition)
        {
            int originalXPosition = heroEntity.XPosition;
            int originalYPosition = heroEntity.YPosition;

            heroEntity.deltaX = nextXPosition;
            heroEntity.deltaY = nextYPosition;
            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.AreEqual(originalXPosition, heroEntity.XPosition);
            Assert.AreEqual(originalYPosition, heroEntity.YPosition);
        }

        [Test]
        public void GivenProposedPosition_CheckCollision_UpdatePositionAccordingly()
        {
            heroEntity.MovementSm.UpdateInput(InputCommand.RIGHT);

            int initialPosition = heroEntity.XPosition;
            // [ X ][ X ][ 1 ][ 1 ][ 1 ]
            // [ X ][ X ][ 1 ][ 1 ][ 2 ]
            //     ^       ^    ^    ^
            //   start    air  air solid

            // Should be able to take 3 steps then collide
            for (int step = 1; step < 3; step++)
            {
                heroEntity.MovementSm.UpdateInput(InputCommand.RIGHT);
                heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
                Assert.AreEqual(initialPosition + step, heroEntity.XPosition);
            }
            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            // Should not have moved to last block because of collision
            Assert.AreEqual(2, heroEntity.XPosition);
        }

        [Test]
        public void GivenFallingHero_UpdatePositionAccordingly()
        {
            testWorld.map = new int[2][];

            testWorld.map[0] = new int[6] { (int)ObjectType.Air, (int)ObjectType.Solid, (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air };
            testWorld.map[1] = new int[6] { (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air };

            heroEntity.XPosition = 0;
            heroEntity.YPosition = 4;

            //    start
            //      v  
            // [ X ][ X ]
            // [ X ][ X ]
            // [ 1 ][ 1 ] < air
            // [ 1 ][ 1 ] < air
            // [ 2 ][ 1 ] < air
            // [ 1 ][ 1 ]
            //   ^    ^  
            //  air  air

            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Idle));
            Assert.AreEqual(4, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);

            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Falling));
            Assert.AreEqual(3, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);

            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            // Assert.IsFalse(heroEntity.MovementSm.GetStateType() == typeof(Falling));
            Assert.AreEqual(2, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);
        }

        [Test]
        public void GivenFallingHeroOntoCollectible_UpdatePositionAccordingly()
        {
            testWorld.map = new int[2][];

            testWorld.map[0] = new int[6] { (int)ObjectType.Air, (int)ObjectType.Solid, (int)ObjectType.Collectible, (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air };
            testWorld.map[1] = new int[6] { (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Collectible, (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air };

            heroEntity.XPosition = 0;
            heroEntity.YPosition = 4;

            //    start
            //      v  
            // [ X ][ X ]
            // [ X ][ X ]
            // [ 1 ][ 1 ] < air
            // [ 1 ][ 1 ] < air
            // [ 2 ][ 1 ] < air
            // [ 1 ][ 1 ]
            //   ^    ^  
            //  air  air

            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Idle));
            Assert.AreEqual(4, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);

            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Falling));
            Assert.AreEqual(3, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);

            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            // Assert.IsFalse(heroEntity.MovementSm.GetStateType() == typeof(Falling));
            Assert.AreEqual(2, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);
        }

        [Test]
        public void GivenClimbingHero_UpdatePositionAccordingly()
        {
            testWorld.map = new int[2][];

            testWorld.map[0] = new int[6] { (int)ObjectType.Ladder, (int)ObjectType.Ladder, (int)ObjectType.Air, (int)ObjectType.Ladder, (int)ObjectType.Air, (int)ObjectType.Air };
            testWorld.map[1] = new int[6] { (int)ObjectType.Air, (int)ObjectType.Ladder, (int)ObjectType.Ladder, (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air };

            heroEntity.XPosition = 0;
            heroEntity.YPosition = 0;

            //    start
            //      v  
            // [ 1 ][ 1 ] < air
            // [ 1 ][ 1 ] < air
            // [ 5 ][ 1 ] < Ladder on left
            // [ 1 ][ 5 ] < Ladder on right
            // [ 5 ][ 5 ] < Ladder both sides
            // [ 5 ][ 1 ] < Ladder on left

            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Idle));
            Assert.AreEqual(0, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);

            heroEntity.MovementSm.UpdateInput(InputCommand.UP);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Moving));

            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Idle));
            Assert.AreEqual(1, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);

            heroEntity.MovementSm.UpdateInput(InputCommand.UP);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Moving));

            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Idle));
            Assert.AreEqual(2, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);

            heroEntity.MovementSm.UpdateInput(InputCommand.UP);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Moving));

            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Idle));
            Assert.AreEqual(3, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);

            heroEntity.MovementSm.UpdateInput(InputCommand.UP);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Moving));

            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Idle));
            Assert.AreEqual(4, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);

            // Hero doesn't fall because the ladder is underneath
            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Idle));
            Assert.AreEqual(4, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);
        }

        [Test]
        public void GivenJumpingHero_UpdatePositionAccordingly()
        {
            testWorld.map = new int[2][];

            testWorld.map[0] = new int[6] { (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air };
            testWorld.map[1] = new int[6] { (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air };

            heroEntity.XPosition = 0;
            heroEntity.YPosition = 0;

            //    start
            //      v  
            // [ 1 ][ 1 ] < air
            // [ 1 ][ 1 ] < air
            // [ 1 ][ 1 ] < air
            // [ 1 ][ 1 ] < air
            // [ X ][ X ]
            // [ X ][ X ]
            //   ^    ^  
            //  air  air

            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Idle));
            Assert.AreEqual(0, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);

            heroEntity.MovementSm.UpdateInput(InputCommand.UP);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Jumping));

            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Jumping));
            Assert.AreEqual(1, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);

            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Jumping));
            Assert.AreEqual(2, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);

            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Jumping));
            Assert.AreEqual(3, heroEntity.YPosition); //Max jump height is 3
            Assert.AreEqual(0, heroEntity.XPosition);

            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Falling));
            Assert.AreEqual(3, heroEntity.YPosition); //Max jump height is 3
            Assert.AreEqual(0, heroEntity.XPosition);

            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Falling));
            Assert.AreEqual(2, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);

            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Falling));
            Assert.AreEqual(1, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);

            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Falling));
            Assert.AreEqual(0, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);

            // should the hero stop falling after they move into the new position or should they stop falling on the next tick?
            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Idle));
            Assert.AreEqual(0, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);
        }

        [Test]
        public void GivenJumpingHero_WhenColliding_UpdatePositionAccordingly()
        {
            testWorld.map = new int[2][];

            testWorld.map[0] = new int[6] { (int)ObjectType.Solid, (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air };
            testWorld.map[1] = new int[6] { (int)ObjectType.Solid, (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Solid, (int)ObjectType.Air };

            heroEntity.XPosition = 0;
            heroEntity.YPosition = 1;

            //    start
            //      v  
            // [ 1 ][ 1 ] < air
            // [ 1 ][ 2 ] < air
            // [ 1 ][ 1 ] < air
            // [ X ][ X ]
            // [ X ][ X ]
            // [ 2 ][ 2 ] < solid

            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Idle));
            Assert.AreEqual(1, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);

            heroEntity.MovementSm.UpdateInput(InputCommand.UP);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Jumping));

            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Jumping));
            Assert.AreEqual(2, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);

            // hits solid ceiling here
            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Idle));
            Assert.AreEqual(2, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);

            // starts falling
            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Falling));
            Assert.AreEqual(1, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);

            // hits solid floor here
            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Idle));
            Assert.AreEqual(1, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);

            // remains in place on solid floor
            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Idle));
            Assert.AreEqual(1, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);
        }

        [Test]
        public void GivenPlayerCollision_UpdatePositionAccordingly()
        {
            int initialPosition = heroEntity.XPosition;

            //    start   air  air  air  air   player
            //      v      v    v    v    v      v
            // [ X ][ X ][ 1 ][ 1 ][ 1 ][ 1 ][ Y ][ Y ]
            // [ X ][ X ][ 1 ][ 1 ][ 1 ][ 1 ][ Y ][ Y ]

            testWorld.map = new int[8][];

            for (int i = 0; i < 8; i++)
            {
                testWorld.map[i] = new int[2] { (int)ObjectType.Air, (int)ObjectType.Air };
            }

            HeroEntity otherPlayer = new(Id)
            {
                XPosition = 6
            };
            otherPlayer.MovementSm.World = testWorld;
            otherPlayer.MovementSm.CollidableObjects = new List<GameObject> { heroEntity };
            otherPlayers.Add(otherPlayer);

            // Should be able to take 4 steps then collide
            for (int step = 1; step < 5; step++)
            {
                heroEntity.MovementSm.UpdateInput(InputCommand.RIGHT);
                heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
                Assert.AreEqual(initialPosition + step, heroEntity.XPosition);
            }
            heroEntity.MovementSm.UpdateInput(InputCommand.RIGHT);
            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            // Should not have moved to last block because of collision
            Assert.AreEqual(4, heroEntity.XPosition);
        }

        [Test]
        public void GivenFallingHero_FallsOnTopOfAnotherPlayer_UpdatePositionAccordingly()
        {
            testWorld.map = new int[2][];

            testWorld.map[0] = new int[6] { (int)ObjectType.Air, (int)ObjectType.Solid, (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air };
            testWorld.map[1] = new int[6] { (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air, (int)ObjectType.Air };

            heroEntity.XPosition = 0;
            heroEntity.YPosition = 4;

            HeroEntity otherPlayer = new(Id);
            otherPlayers.Add(otherPlayer);

            //    start
            //      v  
            // [ X ][ X ]
            // [ X ][ X ]
            // [ 1 ][ 1 ] < air
            // [ 1 ][ 1 ] < air
            // [ Y ][ Y ]
            // [ Y ][ Y ]
            //     ^
            //   player

            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Idle));
            Assert.AreEqual(4, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);

            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.IsTrue(heroEntity.MovementSm.GetStateType() == typeof(Falling));
            Assert.AreEqual(3, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);

            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            // Assert.IsFalse(heroEntity.MovementSm.GetStateType() == typeof(Falling));
            Assert.AreEqual(2, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);

            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            // Assert.IsFalse(heroEntity.MovementSm.GetStateType() == typeof(Falling));
            Assert.AreEqual(2, heroEntity.YPosition);
            Assert.AreEqual(0, heroEntity.XPosition);
        }

        [Test]
        public void GivenCollectables_ShouldIncrementCollected()
        {
            //    start   air  air  air  air
            //      v      v    v    v    v 
            // [ X ][ X ][ 1 ][ 1 ][ 1 ][ 1 ]
            // [ X ][ X ][ 3 ][ 1 ][ 1 ][ 3 ]
            //             ^              ^
            //         collectible   collectible

            testWorld.map = new int[6][];

            for (int i = 0; i < 6; i++)
            {
                testWorld.map[i] = new int[2] { (int)ObjectType.Air, (int)ObjectType.Air };
            }
            testWorld.map[2][0] = (int)ObjectType.Collectible;
            testWorld.map[5][0] = (int)ObjectType.Collectible;

            heroEntity.MovementSm.UpdateInput(InputCommand.RIGHT);
            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.AreEqual(1, heroEntity.Collected);

            heroEntity.MovementSm.UpdateInput(InputCommand.RIGHT);
            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.AreEqual(1, heroEntity.Collected);

            heroEntity.MovementSm.UpdateInput(InputCommand.RIGHT);
            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.AreEqual(1, heroEntity.Collected);

            heroEntity.MovementSm.UpdateInput(InputCommand.RIGHT);
            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.AreEqual(2, heroEntity.Collected);
        }

        [Test]
        public void GivenCollectables_AlreadyCollected_ShouldNotIncrementCollected()
        {
            // heroEntity.MovementSm.deltaX = 1;

            //    start   air  air  air  air  air  air
            //      v      v    v    v    v    v    v
            // [ X ][ X ][ 1 ][ Y ][ Y ][ 1 ][ 1 ][ 1 ]
            // [ X ][ X ][ 1 ][ Y ][ Y ][ 3 ][ 1 ][ 1 ]
            //                            ^
            //                       collectible

            testWorld.map = new int[8][];

            for (int i = 0; i < 8; i++)
            {
                testWorld.map[i] = new int[2] { (int)ObjectType.Air, (int)ObjectType.Air };
            }
            testWorld.map[5][0] = (int)ObjectType.Collectible;

            HeroEntity otherPlayer = new(Id);
            otherPlayer.MovementSm.World = testWorld;
            otherPlayer.MovementSm.CollidableObjects = new List<GameObject> { heroEntity };
            otherPlayer.XPosition = 3;
            otherPlayers.Add(otherPlayer);

            heroEntity.MovementSm.UpdateInput(InputCommand.RIGHT);
            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.AreEqual(0, heroEntity.Collected);

            otherPlayer.MovementSm.UpdateInput(InputCommand.RIGHT);
            heroPhysicsUnderTest.Update(otherPlayer, new List<HeroEntity>(), testWorld);
            Assert.AreEqual(1, otherPlayer.Collected);

            heroEntity.MovementSm.UpdateInput(InputCommand.RIGHT);
            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.AreEqual(0, heroEntity.Collected);

            otherPlayer.MovementSm.UpdateInput(InputCommand.RIGHT);
            heroPhysicsUnderTest.Update(otherPlayer, new List<HeroEntity>(), testWorld);
            Assert.AreEqual(1, otherPlayer.Collected);

            heroEntity.MovementSm.UpdateInput(InputCommand.RIGHT);
            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.AreEqual(0, heroEntity.Collected);

            otherPlayer.MovementSm.UpdateInput(InputCommand.RIGHT);
            heroPhysicsUnderTest.Update(otherPlayer, new List<HeroEntity>(), testWorld);
            Assert.AreEqual(1, otherPlayer.Collected);

            heroEntity.MovementSm.UpdateInput(InputCommand.RIGHT);
            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.AreEqual(0, heroEntity.Collected);
        }

        [Test]
        public void GivenHazard_ShouldResetPlayer()
        {
            //    start   air  air  air  air
            //      v      v    v    v    v 
            // [ X ][ X ][ 0 ][ 0 ][ 0 ][ 0 ]
            // [ X ][ X ][ 0 ][ 0 ][ 0 ][ 0 ]
            // [ 1 ][ 1 ][ 1 ][ 1 ][ 3 ][ 1 ] < solid
            //   ^         ^         ^    
            // solid     solid     hazard

            testWorld.map = new int[6][];

            for (int i = 0; i < 6; i++)
            {
                testWorld.map[i] = new int[3] { (int)ObjectType.Solid, (int)ObjectType.Air, (int)ObjectType.Air };
            }
            testWorld.map[4][0] = (int)ObjectType.Hazard;
            testWorld.start = new Point(0,1);
            heroEntity.YPosition = 1;
            heroEntity.Collected = 100;

            heroEntity.MovementSm.UpdateInput(InputCommand.RIGHT);
            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.AreEqual(1, heroEntity.XPosition);
            Assert.AreEqual(1, heroEntity.YPosition);

            heroEntity.MovementSm.UpdateInput(InputCommand.RIGHT);
            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.AreEqual(2, heroEntity.XPosition);
            Assert.AreEqual(1, heroEntity.YPosition);

            heroEntity.MovementSm.UpdateInput(InputCommand.RIGHT);
            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.AreEqual(0, heroEntity.XPosition);
            Assert.AreEqual(1, heroEntity.YPosition);

            heroEntity.MovementSm.UpdateInput(InputCommand.RIGHT);
            heroPhysicsUnderTest.Update(heroEntity, otherPlayers, testWorld);
            Assert.AreEqual(1, heroEntity.XPosition);
            Assert.AreEqual(1, heroEntity.YPosition);
        }


        private void TestFalling(int mapWidth, int mapHeight, int platformHeight)
        {
            heroEntity.XPosition = 0;
            heroEntity.YPosition = platformHeight + 1;


            // Make sure hero is settled on the platform.
            for (int i = 0; i < 3; i++)
            {
                heroEntity.PhysicsComponent.Update(heroEntity, otherPlayers, testWorld);
            }

            int expectedYPosition = platformHeight + 1;
            Assert.AreEqual(expectedYPosition, heroEntity.YPosition);

            // Move all the way to the right.
            while (heroEntity.XPosition < mapWidth - (heroEntity.Width + 1))
            {
                heroEntity.MovementSm.UpdateInput(InputCommand.RIGHT);
                heroEntity.PhysicsComponent.Update(heroEntity, otherPlayers, testWorld);
            }
            // Should be at end of map.
            Assert.AreEqual(mapWidth - (heroEntity.Width + 1), heroEntity.XPosition);
            // Should not have fallen.
            Assert.AreEqual(expectedYPosition, heroEntity.YPosition);

            // Move back to the left.
            while (heroEntity.XPosition > 0)
            {
                heroEntity.MovementSm.UpdateInput(InputCommand.LEFT);
                heroEntity.PhysicsComponent.Update(heroEntity, otherPlayers, testWorld);
            }
            // Should be at end of map.
            Assert.AreEqual(0, heroEntity.XPosition);
            // Should not have fallen.
            Assert.AreEqual(expectedYPosition, heroEntity.YPosition);
        }

        [Test]
        public void GivenFlatPlatform_HeroShouldNotFallOff()
        {
            // Initialise map
            int mapWidth = testWorld.width, mapHeight = testWorld.height;
            testWorld.map = new int[mapWidth][];
            for (int x = 0; x < mapWidth; x++)
            {
                testWorld.map[x] = new int[mapHeight];
            }

            // Add a platform across the map at a set height.
            int platformHeight = mapHeight / 2;
            for (int x = 0; x < mapWidth; x++)
            {
                testWorld.map[x][platformHeight] = (int)ObjectType.Platform;
            }

            TestFalling(mapWidth, mapHeight, platformHeight);
        }

        [Test]
        public void GivenPlatformWithLadders_HeroShouldNotFallOff()
        {
            // Initialise map
            int mapWidth = testWorld.width, mapHeight = testWorld.height;
            testWorld.map = new int[mapWidth][];
            for (int x = 0; x < mapWidth; x++)
            {
                testWorld.map[x] = new int[mapHeight];
            }

            // Add a platform across the map at a set height.
            int platformHeight = mapHeight / 2;
            for (int x = 0; x < mapWidth; x++)
            {
                testWorld.map[x][platformHeight] = (int)ObjectType.Platform;
            }

            // Add a ladder going across the map.
            int ladderPosition = mapWidth / 2;
            for (int y = 0; y < mapHeight; y++)
            {
                testWorld.map[ladderPosition][y] = (int)ObjectType.Ladder;
            }

            TestFalling(mapWidth, mapHeight, platformHeight);
        }

        [Test]
        public void GivenHeroJumpedOnPlatform_HeroShouldNotFall()
        {
            // Initialise map
            int mapWidth = testWorld.width, mapHeight = testWorld.height;
            testWorld.map = new int[mapWidth][];
            for (int x = 0; x < mapWidth; x++)
            {
                testWorld.map[x] = new int[mapHeight];
            }

            // Add a platform across the map at a set height.
            int platformHeight = mapHeight / 2;
            for (int x = 0; x < mapWidth; x++)
            {
                testWorld.map[x][platformHeight] = (int)ObjectType.Platform;
            }

            // Place hero on top of the platform.
            heroEntity.XPosition = 0;
            heroEntity.YPosition = platformHeight + 1;

            // Wait for the hero to be settled.
            for (int i = 0; i < 3; i++)
            {
                heroEntity.PhysicsComponent.Update(heroEntity, otherPlayers, testWorld);
            }

            int expectedYPosition = platformHeight + 1;
            Assert.AreEqual(expectedYPosition, heroEntity.YPosition);

            // Jump
            heroEntity.MovementSm.UpdateInput(InputCommand.UP);
            heroEntity.PhysicsComponent.Update(heroEntity, otherPlayers, testWorld);
            while (heroEntity.YPosition > expectedYPosition)
            {
                heroEntity.PhysicsComponent.Update(heroEntity, otherPlayers, testWorld);
            }

            // Move right
            for (int i = 0; i < 5; i++)
            {
                heroEntity.MovementSm.UpdateInput(InputCommand.RIGHT);
                heroEntity.PhysicsComponent.Update(heroEntity, otherPlayers, testWorld);
            }
            Assert.AreEqual(expectedYPosition, heroEntity.YPosition);
        }

        [Test]
        public void GivenWorldSize_ShouldNotAllowLeavingWorldBounds()
        {
            // Initialise empty map.
            testWorld.map = new int[testWorld.width][];
            for (int x = 0; x < testWorld.width; x++)
            {
                testWorld.map[x] = new int[testWorld.height];
                for (int y = 0; y < testWorld.height; y++)
                {
                    testWorld.map[x][y] = (int)ObjectType.Air;
                }
            }

            heroEntity.XPosition = 0;
            heroEntity.YPosition = 0;
            heroEntity.MovementSm.UpdateInput(InputCommand.LEFT);
            heroEntity.PhysicsComponent.Update(heroEntity, otherPlayers, testWorld);
            Assert.AreEqual(0, heroEntity.XPosition);
            Assert.AreEqual(0, heroEntity.YPosition);

            heroEntity.XPosition = testWorld.width - (heroEntity.Width + 1);
            heroEntity.MovementSm.UpdateInput(InputCommand.RIGHT);
            Assert.AreEqual(0, heroEntity.YPosition);
            Assert.AreEqual(testWorld.width - (heroEntity.Width + 1), heroEntity.XPosition);

            // Add platform below player.
            int platformHeight = testWorld.height - (heroEntity.Height + 2);
            for (int x = 0; x < testWorld.width; x++)
            {
                testWorld.map[x][platformHeight] = (int)ObjectType.Platform;
            }

            // Try jumping
            heroEntity.YPosition = platformHeight + 1;
            heroEntity.MovementSm.UpdateInput(InputCommand.UP);
            for (int i = 0; i < 3; i++)
            {
                heroEntity.PhysicsComponent.Update(heroEntity, otherPlayers, testWorld);
            }
            Assert.AreEqual(platformHeight + 1, heroEntity.YPosition);
        }
    }
}
