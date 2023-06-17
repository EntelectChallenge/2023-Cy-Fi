using System;

namespace ReferenceBot.AI.DataStructures.Spatial
{
    struct Point : IEquatable<Point>
    {
        public int X;
        public int Y;

        public Point(int _X, int _Y)
        {
            X = _X;
            Y = _Y;
        }

        public readonly bool Equals(Point other)
        {
            return X == other.X && Y == other.Y;
        }

        public override readonly bool Equals(object obj)
        {
            return obj is Point point && Equals(point);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static Point operator -(Point a, Point b)
        {
            return new(a.X - b.X, a.Y - b.Y);
        }
    }
}
