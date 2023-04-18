using CyFi.Entity;
using CyFi.Physics.Movement;
using Domain.Enums;
using Domain.Objects;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace CyFi.Physics.Utils;

public static class Collisions
{

    public static bool CollidesWithMap(Point[] boundingBox, WorldObject world, ObjectType[] objectTypes)
    {
        var bottomLeft = boundingBox[0];
        var topRight = boundingBox[1];
        for (int x = bottomLeft.X; x <= topRight.X; x++)
        {
            for (int y = bottomLeft.Y; y <= topRight.Y; y++)
            {
                if (objectTypes.Cast<int>().Contains(world.map[x][y]))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool CollidesWithObject(Point[] boundingBox, Point[] otherBoundingBox)
    {
        var bottomLeft = boundingBox[0];
        var topRight = boundingBox[1];
        var otherBottomLeft = otherBoundingBox[0];
        var otherTopRight = otherBoundingBox[1];

        return !(
            bottomLeft.X < otherBottomLeft.X ||
            bottomLeft.X > otherTopRight.X ||
            topRight.Y < otherBottomLeft.Y ||
            bottomLeft.Y > otherTopRight.Y
        );
    }

    public static bool NoHeroCollision(GameObject gameObject, IEnumerable<GameObject> collidableObjects)
    {
        var otherBoundingBoxes = collidableObjects.Select((collidableObject) => collidableObject.BoundingBox());
        return !otherBoundingBoxes.Any(otherBoundingBox => CollidesWithObject(gameObject.ProposedBoundingBox(), otherBoundingBox));
    }

    public static bool NoWorldCollision(GameObject gameObject, WorldObject world)
    {
        return !CollidesWithMap(gameObject.ProposedBoundingBox(), world, new[] { ObjectType.Solid });
    }

    public static bool OnlyAirBelow(GameObject gameObject, WorldObject world)
    {
        var aboveMapBottomEdge = gameObject.YPosition > 0;
        if (!aboveMapBottomEdge) return false;

        var bottomRow = gameObject.BoundingBox()
            .Where(pos => pos.Y == gameObject.YPosition);
        var onlyAirBelow = bottomRow.All(pos => world.map[pos.X][pos.Y - 1] == (int)ObjectType.Air);

        Console.Write($"{onlyAirBelow}");


        return onlyAirBelow;
    }

    public static bool OnlyGroundBelow(GameObject gameObject, WorldObject world)
    {
        ObjectType[] otherTypes = new[]
        {
            ObjectType.Ladder,
            ObjectType.Platform,
            ObjectType.Hazard
        };
        var groundBelow = WillIntersect(gameObject, world, new(0, -1), new[] { ObjectType.Solid });
        var otherBelow = WillIntersect(gameObject, world, new(0, -1), otherTypes);
        return groundBelow && !otherBelow;
    }

    private static Point[] GetBoundingBox(GameObject gameObject)
    {
        return gameObject.BoundingBox();
    }

    private static bool Intersects(GameObject gameObject, WorldObject world, ObjectType[] types)
    {
        return GetBoundingBox(gameObject).Any(point => types.Cast<int>().Contains(world.map[point.X][point.Y]));
    }

    private static bool WillIntersect(GameObject gameObject, WorldObject world, Point delta, ObjectType[] types)
    {
        return GetBoundingBox(gameObject).Select(point => point + ((Size)delta)).Any(point => types.Cast<int>().Contains(world.map[point.X][point.Y]));
    }
}