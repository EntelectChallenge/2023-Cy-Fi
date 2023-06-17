using ReferenceBot.AI.DataStructures.Spatial;
using System;
using System.Collections.Generic;

namespace ReferenceBot.AI.DataStructures.Pathfinding
{
    // A node for use in pathfinding.
    class Node : IEquatable<Node>
    {
        public int X;
        public int Y;
        public int GCost;
        public int HCost;
        public bool Walkable;
        public Node? parent;

        public int FCost => GCost + HCost;

        public Node(int _X, int _Y, bool _Walkable, Node? _parent)
        {
            X = _X;
            Y = _Y;
            Walkable = _Walkable;
            GCost = parent != null ? parent.GCost + 1 : 0;
            parent = _parent;
        }

        public static implicit operator Point(Node n) => new(n.X, n.Y);

        public bool Equals(Node other)
        {
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }

    public class NodeCostComparer : IComparer<Node>
    {
        int IComparer<Node>.Compare(Node x, Node y)
        {
            return x.FCost - y.FCost;
        }
    }

    public class NodeEqualityComparer : IEqualityComparer<Node>
    {
        bool IEqualityComparer<Node>.Equals(Node x, Node y)
        {
            return x.Equals(y);
        }

        int IEqualityComparer<Node>.GetHashCode(Node obj)
        {
            return obj.GetHashCode();
        }
    }
}
