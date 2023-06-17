using Domain.Enums;
using Domain.Models;
using ReferenceBot.AI.DataStructures.Pathfinding;
using ReferenceBot.AI.DataStructures.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace ReferenceBot.AI.States
{
    class SearchingState : State
    {
        // -1 for left, 1 for right
        private int direction;
        private int SearchCooldown;
        private bool IsSearching;
        public SearchingState(BotStateMachine StateMachine) : base(StateMachine)
        { }

        public override void EnterState(State PreviousState)
        {
            SearchCooldown = 0;
            direction = 1;
            Console.WriteLine("Entering Searching");
        }

        public override void ExitState(State NextState)
        {
            direction = 0;
        }

        private class CollectibleSorter : IComparer<Point>
        {
            Point Goal;

            public CollectibleSorter(Point goal)
            {
                Goal = goal;
            }

            public int Compare(Point x, Point y)
            {
                return ManhattanDistance(x, Goal) - ManhattanDistance(y, Goal);
            }
        }

        private void SearchForCollectibles(BotStateDTO BotState)
        {
            // Prevent accidentally searching multiple times.
            if (IsSearching)
            {
                return;
            }
            Console.WriteLine("Searching for collectibles");
            IsSearching = true;
            var playerBounds = GetPlayerBoundingBox(BotState);

            // Find all collectibles
            List<Point> collectibles = FindCollectibles(BotState);
            Console.WriteLine($"Search found {collectibles.Count()} collectibles");
            if (collectibles.Count() == 0)
            {
                SearchCooldown = 5;
                IsSearching = false;
                return;
            }
            List<Point> closestCollectibles;

            // Get closest half collectibles
            if (collectibles.Count > 5)
            {
                SortedSet<Point> sortedCollectibles = new SortedSet<Point>(new CollectibleSorter(playerBounds.Position));
                foreach (var collectible in collectibles)
                {
                    sortedCollectibles.Add(collectible);
                }
                closestCollectibles = sortedCollectibles.Take(collectibles.Count / 2).ToList();
            }
            else
            {
                closestCollectibles = collectibles;
            }

            // Calculate which collectible has the shortest path
            Point closestCollectibleByPath = closestCollectibles.First();
            Path? closestPath = PathfindToPoint(BotState, closestCollectibleByPath);
            foreach (var collectible in closestCollectibles.Skip(1))
            {
                int closestPathDistance = closestPath is Path path ? path.Length : Int32.MaxValue;
                Console.WriteLine("Finding path");
                var newPath = PathfindToPoint(BotState, collectible);
                if (newPath != null)
                {
                    Console.WriteLine($"Found path of length {newPath.Length}");
                } else
                {
                    Console.WriteLine($"Failed to find path");
                }

                if (newPath is Path newP && newP.Length < closestPathDistance)
                {
                    closestCollectibleByPath = collectible;
                    closestPath = newPath;
                }
            }

            // If closestPath is null, we haven't managed to pathfind to any collectibles, so keep searching.
            if (closestPath is Path)
            {
                Console.WriteLine($"Closest path found of length {closestPath.Length}");
                var newState = new Collecting(StateMachine, closestCollectibleByPath, closestPath);
                ChangeState(newState);
            } else
            {
                Console.WriteLine("Failed to find a suitable path");
                SearchCooldown = 5;
                IsSearching = false;
            }
        }

        public override InputCommand Update(BotStateDTO BotState, BotStateDTO? LastKnownState)
        {
            Console.WriteLine($"IsOnGround: {IsOnGround(BotState)}, " +
                $"IsOnPlatform: {IsOnPlatform(BotState)}, " +
                $"IsHazardInFrontOfPlayer: {IsHazardInFrontOfPlayer(BotState, direction)}, " +
                $"IsWallInFrontOfPlayer: {IsWallInFrontOfPlayer(BotState, direction)}");


            if (SearchCooldown-- <= 0)
            {
                SearchForCollectibles(BotState);
            }
              

            if (ShouldJump(BotState))
            {
                return direction == -1 ? InputCommand.UPLEFT : InputCommand.UPRIGHT;
            }
            else if (CanClimb(BotState))
            {
                return InputCommand.UP;
            }
            else
            {
                return direction == -1 ? InputCommand.LEFT : InputCommand.RIGHT;
            }
        }
        private bool ShouldJump(BotStateDTO state)
        {

            return (IsOnGround(state) || IsOnPlatform(state)) &&
                (IsHazardInFrontOfPlayer(state, direction) || IsWallInFrontOfPlayer(state, direction));
        }

        private static List<Point> FindCollectibles(BotStateDTO state)
        {
            var collectibles = new List<Point>();
            for (int x = 0; x < state.HeroWindow.Length; x++)
            {
                var col = state.HeroWindow[x];
                for (int y = 0; y < col.Length; y++)
                {
                    if (state.HeroWindow[x][y] == (int)ObjectType.Collectible)
                    {
                        collectibles.Add(new Point(x, y));
                    }
                }
            }
            return collectibles;
        }
    }
}