using CyFi.Entity;
using CyFi.Physics.Movement;
using Domain.Enums;
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
        Console.WriteLine($"World LEVEL: {movementSm.World.level}");

        Console.Write("Check Falling: ");

        bool falling = Collisions.OnlyAirOrCollectableBelow(movementSm.GameObject, movementSm.World);

        Console.Write((falling == true) ? "Bot is falling\n" : "Bot is not falling\n");

        return falling;
    }

    public static void UpDecision(MovementSM movementSm)
    {
        // if on ladder then move up.
        // else jumping state and jump;
        var onLadder = movementSm.GameObject.BoundingBox().Any(point => movementSm.World.map[point.X][point.Y] == (int)ObjectType.Ladder);
        //   var onGround = movementSm.GameObject.BelowBoundingBox().Any(point => movementSm.World.map[point.X][point.Y] != (int)ObjectType.Air);

        var test = movementSm.GetStateType;
        if (Collisions.OnlyAirOrCollectableBelow(movementSm.GameObject, movementSm.World))
        {
            Console.WriteLine("Not on surface");
            return;
        }

        if (onLadder)
        {
            Console.WriteLine("On Ladder");
            movementSm.GameObject.deltaY = 1;
            movementSm.ChangeState(movementSm.Moving);
        }
        else //if (onGround)
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
    public static void StealDecision(MovementSM movementSm)
    {
        // if(movementSm.GameObject.Id == 3) { 

        movementSm.ChangeState(movementSm.Stealing);
    }

    public static void Steal(MovementSM movementSm,
        HeroEntity hero, List<HeroEntity> opposingPlayers)
    {

        double stealPercentage = 0.25;//TODO get from config

        var stealWindow = hero.HeroStealWindow(2, 2);

        Console.WriteLine("STEAL action attempted:");

        foreach (HeroEntity player in opposingPlayers)
        {

            if (player.XPosition > stealWindow[0].X &&
                player.XPosition < stealWindow[1].X &&
                player.YPosition > stealWindow[0].Y &&
                player.YPosition < stealWindow[1].Y)
            {
                if (hero.Collected < player.Collected)
                {
                    Console.WriteLine($"STEAL action: SUCCESS {hero.Id} collected was: {hero.Collected} ");

                    int stolenAmount = (int)Math.Round(player.Collected * stealPercentage);
                    //TODO save these values to gamestate?

                    stolenAmount = stolenAmount == 0 ? 1 : stolenAmount;

                    player.Collected -= stolenAmount;
                    hero.Collected += stolenAmount;

                    Console.Write($"now: {hero.Collected}");
                }
            }
            else
            {
                Console.WriteLine("STEAL action: FAIL");
            }
        }
    }

    public static void RadarDecision(MovementSM movementSm)
    {
        movementSm.ChangeState(movementSm.ActivateRadar);
    }

    public static void ActivateRadar(MovementSM movementSm,
    HeroEntity hero, List<HeroEntity> opposingPlayers)
    {
        int heroXvalue = hero.XPosition;
        int heroYvalue = hero.YPosition;

        List<RangeData> radarData = new();

        int radarRange = 50;

        //Oranise from most to lease
        //    opposingPlayers.

        foreach (HeroEntity player in opposingPlayers)
        {
            if (hero.Collected < player.Collected)
            {
                var distance = CalculateDistance(heroXvalue, heroYvalue, player.NextXPosition, player.NextYPosition);
                var percentage = RangeData.CalculatePercentage(distance, radarRange);
                int directionFromOpponent = (int)RangeData.GetDirection(heroXvalue, heroYvalue, player.NextXPosition, player.NextYPosition);
                // var directionNonMainToMain = GetDirection(player.NextXPosition, player.NextYPosition, heroXvalue, heroYvalue);
                radarData.Add(new RangeData
                {
                    DirectionFromOpponent = directionFromOpponent,
                    PercentageDistance = percentage
                });
            }
        }

        hero.radarData = radarData;
    }

    static double CalculateDistance(int x1, int y1, int x2, int y2) =>
           Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));

    public class RangeData
    {
        public int DirectionFromOpponent { get; set; }
        public int PercentageDistance { get; set; }

        public static int CalculatePercentage(double distance, int contactValue)
        {
            double percentage = (distance / contactValue) * 100;
            if (percentage > 100)
                return 0;
            else if (percentage > 75)
                return 4;
            else if (percentage > 50)
                return 3;
            else if (percentage > 25)
                return 2;
            else
                return 1;
        }
        public static InputCommand GetDirection(int x1, int y1, int x2, int y2)
        {
            if (x1 > x2)
                return y1 > y2 ? InputCommand.DOWNLEFT : y1 < y2 ? InputCommand.DOWNRIGHT : InputCommand.LEFT;
            else if (x1 < x2)
                return y1 > y2 ? InputCommand.DOWNRIGHT : y1 < y2 ? InputCommand.UPRIGHT : InputCommand.RIGHT;
            else
                return y1 > y2 ? InputCommand.DOWN : y1 < y2 ? InputCommand.UP : 0;
        }
    }
}