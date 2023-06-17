using System;

namespace ReferenceBot.AI.DataStructures.Spatial
{
    struct BoundingBox : IEquatable<BoundingBox>
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public readonly int Top => Y + Height;
        public readonly int Bottom => Y;
        public readonly int Left => X;
        public readonly int Right => X + Width;

        public readonly Point Position => new Point(X, Y);
        public readonly Point Size => new Point(Width, Height);

        public BoundingBox(int _X, int _Y, int _Width, int _Height)
        {
            X = _X;
            Y = _Y;
            Width = _Width;
            Height = _Height;
        }

        public BoundingBox(int _X, int _Y)
        {
            X = _X;
            Y = _Y;
            Width = 1;
            Height = 1;
        }

        public readonly bool Equals(BoundingBox other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   Width == other.Width &&
                   Height == other.Height;
        }

        public override readonly bool Equals(object obj)
        {
            return obj is BoundingBox box && Equals(box);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Y, Width, Height);
        }
    }
}
