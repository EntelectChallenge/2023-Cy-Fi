using Domain.Enums;
using Domain.Objects;
using System.Drawing;

namespace CyFi.Physics.Utils;

public static class Collisions
{

    public static bool CollidesWithObjectTypes(Point[] boundingBox, WorldObject world, ObjectType[] objectTypes)
    {
        return boundingBox.Any(point => objectTypes.Contains((ObjectType)world.map[point.X][point.Y]));
    }

    public static bool InsideWorldBounds(Point[] boundingBox, WorldObject world)
    {
        var test = boundingBox.All(point =>
            point.X >= 0 && point.X < world.width && point.Y >= 0 && point.Y < world.height);
        return test;
    }

    public static bool CollidesWithObject(Point[] boundingBox, Point[] otherBoundingBox)
    {
        var bottomLeft = boundingBox[0];
        var topRight = boundingBox[3];
        var otherBottomLeft = otherBoundingBox[0];
        var otherTopRight = otherBoundingBox[3];

        var left = bottomLeft.X;
        var right = topRight.X;
        var bottom = bottomLeft.Y;
        var top = topRight.Y;

        var otherLeft = otherBottomLeft.X;
        var otherRight = otherTopRight.X;
        var otherBottom = otherBottomLeft.Y;
        var otherTop = otherTopRight.Y;

        var xOverlap = right >= otherLeft && left <= otherRight;
        var yOverlap = top >= otherBottom && bottom <= otherTop;

        return xOverlap && yOverlap;
    }

    public static bool NoHeroCollision(GameObject gameObject, IEnumerable<GameObject> collidableObjects)
    {
        var otherBoundingBoxes = collidableObjects.Select((collidableObject) => collidableObject.BoundingBox());

        return !otherBoundingBoxes.Any(otherBoundingBox => CollidesWithObject(gameObject.ProposedBoundingBox(), otherBoundingBox));
    }

    public static bool NoWorldCollision(GameObject gameObject, WorldObject world)
    {
        var test = InsideWorldBounds(gameObject.ProposedBoundingBox(), world) &&
               !CollidesWithObjectTypes(gameObject.ProposedBoundingBox(), world, new[] { ObjectType.Solid });
        return test;
    }

    public static bool OnlyAirOrCollectableBelow(GameObject gameObject, WorldObject world)
    {
        var aboveMapBottomEdge = gameObject.YPosition > 0;
        if (!aboveMapBottomEdge) return false;

        var objectsBelowGameObject = new List<ObjectType>()
        {
            (ObjectType)world.map[gameObject.XPosition][gameObject.YPosition - 1],
            (ObjectType)world.map[gameObject.XPosition + 1][gameObject.YPosition - 1],
        };

        var onlyAirOrCollectableBelow = objectsBelowGameObject.All(obj => 
            new List<ObjectType>()
            {
                ObjectType.Air,
                ObjectType.Collectible
            }
            .Contains(obj)
        );

        bool onLadder = Intersects(gameObject, world, new[] { ObjectType.Ladder });

        return onlyAirOrCollectableBelow && !onLadder;
    }

    public static bool HazardsBelow(GameObject gameObject, WorldObject world)
    {
        var aboveMapBottomEdge = gameObject.YPosition > 0;
        if (!aboveMapBottomEdge) return false;

        var bottomRow = gameObject.BoundingBox()
            .Where(pos => pos.Y == gameObject.YPosition);

        var hazardsBelow = bottomRow.Any(pos => world.map[pos.X][pos.Y - 1] == (int)ObjectType.Hazard);

        return hazardsBelow;
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