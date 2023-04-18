using CyFi.Physics.Movement;
using Domain.Enums;
using Microsoft.AspNetCore.Builder;
using System.Drawing;

namespace CyFi.Physics.Utils;

public static class Movements
{
    public static bool AttemptMove(MovementSM movementSm)
    {
        try
        {
            Console.WriteLine($"Any Collisions: {Collisions.NoWorldCollision(movementSm.GameObject, movementSm.World) && Collisions.NoHeroCollision(movementSm.GameObject, movementSm.CollidableObjects)}");
            if (Collisions.NoWorldCollision(movementSm.GameObject, movementSm.World) && Collisions.NoHeroCollision(movementSm.GameObject, movementSm.CollidableObjects))
            {
                movementSm.GameObject.proposedX = movementSm.GameObject.XPosition + movementSm.GameObject.deltaX;
                movementSm.GameObject.proposedY = movementSm.GameObject.YPosition + movementSm.GameObject.deltaY;
                return true;
            }
        }
        catch (IndexOutOfRangeException outOfBounds)
        {
        }

        return false;
    }

    public static bool AttemptDig(MovementSM movementSm)
    {
        try
        {
            return movementSm.World.Dig(movementSm.GameObject.ProposedBoundingBox());
        }
        catch (IndexOutOfRangeException outOfBounds)
        {
            return false;
        }
    }

    public static void UpdateHeroPositions(MovementSM movementSm)
    {
        movementSm.GameObject.XPosition = movementSm.GameObject.proposedX;
        movementSm.GameObject.YPosition = movementSm.GameObject.proposedY;
    }

    public static bool ShouldStartFalling(MovementSM movementSm)
    {
        Console.WriteLine("Check Falling");

        Console.WriteLine($"World LEVEL: {movementSm.World.level}");

        return Collisions.OnlyAirBelow(movementSm.GameObject, movementSm.World);
    }

    public static void UpDecision(MovementSM movementSm)
    {
        // if on ladder then move up.
        // else jumping state and jump;
        var onLadder = movementSm.GameObject.BoundingBox().Any(point => movementSm.World.map[point.X][point.Y] == (int)ObjectType.Ladder);
        if (onLadder)
        {
            Console.WriteLine("On Ladder");
            movementSm.GameObject.deltaY = 1;
            movementSm.ChangeState(movementSm.Moving);
        }
        else
        {

            Console.WriteLine("Jumping");
            movementSm.ChangeState(movementSm.Jumping);
        }
    }

    public static void DigDecision(MovementSM movementSm, InputCommand command)
    {
        Point delta;
        switch (command)
        {
            case InputCommand.DIGDOWN:
                delta = new(0, -1);
                break;
            case InputCommand.DIGLEFT:
                delta = new(-1, 0);
                break;
            case InputCommand.DIGRIGHT:
                delta = new(1, 0);
                break;
            default:
                throw new ArgumentException("DigDecision can only be called with one of the three dig input commands");
        }
        movementSm.GameObject.deltaX = delta.X;
        movementSm.GameObject.deltaY = delta.Y;
        movementSm.ChangeState(movementSm.Digging);
    }
}