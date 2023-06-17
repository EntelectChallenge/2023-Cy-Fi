using Domain.Enums;
using Domain.Models;
using System;
using ReferenceBot.AI.DataStructures.Pathfinding;
using ReferenceBot.AI.DataStructures.Spatial;

namespace ReferenceBot.AI.States
{

    class Collecting : State
    {
        Point Collectible;
        Path PathToCollectible;
        Point CurrentPoint;
        int CurrentPointIndex;

        public Collecting(BotStateMachine _stateMachine, Point collectible, Path pathToCollectible) : base(_stateMachine)
        {
            Collectible = collectible;
            PathToCollectible = pathToCollectible;
        }

        public override void EnterState(State PreviousState)
        {
            CurrentPointIndex = 0;
            CurrentPoint = NextPoint();
            Console.WriteLine("Entered Collecting");
        }

        public override void ExitState(State NextState)
        {
        }

        public override InputCommand Update(BotStateDTO BotState, BotStateDTO LastKnownState)
        {
            if (CurrentPointIndex == PathToCollectible.Length - 1)
            {
                ChangeState(new SearchingState(StateMachine));
            }

            Point nextMoveDelta = DeltaToNextNode(BotState);
            switch((nextMoveDelta.X, nextMoveDelta.Y))
            {
                case (1, 1):
                    return InputCommand.DOWNRIGHT;
                case (1, 0):
                    return InputCommand.RIGHT;
                case (1, -1):
                    return InputCommand.UPRIGHT;
                case (0, 1):
                    return InputCommand.DOWN;
                case (0, -1):
                    return InputCommand.UP;
                case (-1, 1):
                    return InputCommand.DOWNLEFT;
                case (-1, 0):
                    return InputCommand.LEFT;
                case (-1, -1):
                    return InputCommand.UPLEFT;
                default:
                    return InputCommand.RIGHT;
            }
        }

        private Point NextPoint()
        {
            return PathToCollectible.Nodes[CurrentPointIndex++];
        }

        private Point DeltaToNextNode(BotStateDTO state)
        {
            var nextPoint = NextPoint();
            var X = nextPoint.X - CurrentPoint.X;
            var Y = nextPoint.Y - CurrentPoint.Y;

            return new(X, Y);
        }
    }
}
