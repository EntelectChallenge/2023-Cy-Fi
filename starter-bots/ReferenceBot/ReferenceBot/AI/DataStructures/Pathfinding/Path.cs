using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceBot.AI.DataStructures.Pathfinding
{
    class Path
    {
        public List<Node> Nodes;

        public Path()
        {
            Nodes = new();
        }

        public Path(List<Node> nodes)
        {
            Nodes = nodes.ToList();
        }

        public void Add(Node node)
        {
            Nodes.Add(node);
        }

        public int Length => Nodes.Count;
    }

}
