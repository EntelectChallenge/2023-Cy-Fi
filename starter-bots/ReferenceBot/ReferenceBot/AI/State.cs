using Domain.Enums;
using Domain.Models;
using System.Collections.Generic;
using System;
using System.Linq;
using ReferenceBot.AI.DataStructures.Spatial;
using ReferenceBot.AI.DataStructures.Pathfinding;

namespace ReferenceBot.AI
{
    abstract class State
    {

        protected BotStateMachine StateMachine;

        protected const int HeroWidth = 2;
        protected const int HeroHeight = 2;

        protected State(BotStateMachine _stateMachine)
        {
            StateMachine = _stateMachine;
        }

        public abstract void EnterState(State PreviousState);
        public abstract void ExitState(State NextState);

        protected void ChangeState(State NewState)
        {
            StateMachine.ChangeState(NewState);
        }

        public abstract InputCommand Update(BotStateDTO BotState, BotStateDTO? LastKnownState);

        protected BoundingBox GetPlayerBoundingBox(BotStateDTO state)
        {
            var x = (state.HeroWindow.Length / 2) - 1;
            var y = state.HeroWindow[0].Length / 2;
            return new BoundingBox(x, y, HeroWidth, HeroHeight);
        }



        protected bool IsOnPlatform(BotStateDTO state)
        {
            var playerBounds = GetPlayerBoundingBox(state);
            for (int x = playerBounds.Left; x <= playerBounds.Right; x++)
            {
                if (state.HeroWindow[x][playerBounds.Bottom - 2] == (int)ObjectType.Platform)
                    return true;
            }
            return false;
        }

        protected bool IsOnGround(BotStateDTO state)
        {
            var playerBounds = GetPlayerBoundingBox(state);
            for (int x = playerBounds.Left; x <= playerBounds.Right; x++)
            {
                if (state.HeroWindow[x][playerBounds.Bottom - 2] == (int)ObjectType.Solid)
                    return true;
            }
            return false;
        }

        protected bool CanClimb(BotStateDTO state)
        {
            var playerBounds = GetPlayerBoundingBox(state);
            for (int x = playerBounds.Left; x < playerBounds.Right; x++)
            {
                for (int y = playerBounds.Bottom - 1; y <= playerBounds.Top; y++)
                {
                    if (state.HeroWindow[x][y] == (int)ObjectType.Ladder) return true;
                }
            }
            return false;
        }

        protected bool IsHazardInFrontOfPlayer(BotStateDTO state, int direction)
        {
            var position = GetPlayerBoundingBox(state);
            int startX = position.Left;
            int endX = position.Right + 1;
            if (direction == -1)
            {
                startX = position.Left - 1;
                endX = position.Right;
            }
            for (int x = startX; x < endX; x++)
            {
                for (int y = position.Bottom - 2; y <= position.Top + 1; y++)
                {
                    if (state.HeroWindow[x][y] == (int)ObjectType.Hazard)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected bool IsWallInFrontOfPlayer(BotStateDTO state, int direction)
        {
            var playerBounds = GetPlayerBoundingBox(state);
            int startX = playerBounds.Left;
            int endX = playerBounds.Right + 2;
            if (direction == -1)
            {
                startX = playerBounds.Left - 2;
                endX = playerBounds.Right;
            }
            for (int x = startX; x <= endX + 2; x++)
            {
                if (state.HeroWindow[x][playerBounds.Bottom - 1] == (int)ObjectType.Solid)
                    return true;
            }
            return false;
        }


        protected Path? PathfindToPoint(BotStateDTO state, Point point)
        {
            var playerBounds = GetPlayerBoundingBox(state);

            if (!IsPointWalkable(state, point))
            {
                return null;
            }

            Point nearestSolidGround = new Point(playerBounds.Left, playerBounds.Bottom - 1);
            
            // If player is falling
            if (!(IsOnGround(state) || IsOnPlatform(state) || CanClimb(state)))
            {
                for (int x = playerBounds.Left; x < playerBounds.Right; x++)
                {
                    for (int y = playerBounds.Bottom - 1; y >= 0; y--)
                    {
                        ObjectType tile = (ObjectType)state.HeroWindow[x][y];
                        if (tile == ObjectType.Solid || tile == ObjectType.Platform || tile == ObjectType.Ladder)
                        {
                            nearestSolidGround = new(x, y);
                            break;
                        }
                    }
                }
            }

            return PerformAStarSearch(state, nearestSolidGround, point);
        }

        // A* algorithm
        protected static Path? PerformAStarSearch(BotStateDTO state, Point start, Point end)
        {
            var startNode = new Node(start.X, start.Y, true, null);
            var endNode = new Node(end.X, end.Y, true, null);

            startNode.HCost = ManhattanDistance(start, end);

            HashSet<Node> openSet = new();
            HashSet<Node> closedSet = new();

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                var currentNode = openSet.First();
                Console.WriteLine($"Processing point: (X: {currentNode.X}, Y: {currentNode.Y}, FCost: {currentNode.FCost})");
                if (currentNode.Equals(endNode))
                {
                    endNode.parent = currentNode.parent;
                    return ConstructPath(endNode);
                }
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                var neighbours = Neighbours(state, currentNode);

                foreach (var neighbour in Neighbours(state, currentNode))
                {
                    if (closedSet.Contains(neighbour))
                    {
                        continue;
                    }
                    neighbour.HCost = ManhattanDistance(neighbour, end);
                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                        continue;
                    }
                    var openNeighbour = openSet.Where(neighbour.Equals).First();
                    if (neighbour.GCost < openNeighbour.GCost)
                    {
                        openNeighbour.GCost = neighbour.GCost;
                        openNeighbour.parent = neighbour.parent;
                    }
                }
                openSet = openSet.OrderBy((node) => node.FCost).ToHashSet();
            }

            return null;
        }

        private static HashSet<Node> Neighbours(BotStateDTO state, Node node)
        {
            var neighbours = new HashSet<Node>();
            for (int x = node.X - 1; x <= node.X + 1; x++)
            {
                for (int y = node.Y - 1; y <= node.Y + 1; y++)
                {
                    // Check that point isn't out of bounds.
                    if (x < 0 || y < 0 || x >= state.HeroWindow.Length || y >= state.HeroWindow[0].Length)
                    {
                        continue;
                    }
                    if ((y == node.Y && x == node.X) || !IsPointWalkable(state, new Point(x, y)))
                    {
                        continue;
                    }
                    neighbours.Add(new(x, y, true, node));
                }
            }
            return neighbours;
        }

        private static Path ConstructPath(Node node)
        {
            Path path = new();
            path.Add(node);

            while (node.parent != null)
            {
                node = node.parent;
                path.Add(node);
            }
            return path;
        }

        private static bool IsPointWalkable(BotStateDTO state, Point point)
        {
            var tile = (ObjectType)state.HeroWindow[point.X][point.Y];
            var tileBelow = (ObjectType)state.HeroWindow[point.X][point.Y - 1];

            if (tile == ObjectType.Hazard || tile == ObjectType.Solid)
            {
                return false;
            }
            else if (tile == ObjectType.Ladder || tileBelow == ObjectType.Platform)
            {
                return true;
            }
            else {
                // Within jump height distance (~3 blocks)
                /*for (int y = point.Y; y >= point.Y - 3; y--)
                {
                    var underneathTile = (ObjectType)state.HeroWindow[point.X][y];
                    if (underneathTile == ObjectType.Solid || underneathTile == ObjectType.Ladder || underneathTile == ObjectType.Platform)
                    {
                        return true;
                    }
                }*/
            }

            return false;
        }

        // Calculate cost using manhattan distance as a heuristic.
        protected static int ManhattanDistance(Point currentPoint, Point goal)
        {
            return Math.Abs(currentPoint.X -  goal.X) + Math.Abs(currentPoint.Y - goal.Y);
        }
    }
}