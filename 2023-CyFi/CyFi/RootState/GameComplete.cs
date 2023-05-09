using CyFi.Entity;
using CyFi.RootState;
using Domain.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Components
{
    public struct GameComplete
    {
        public int TotalTicks { get; set; }
        public List<PlayerResult> Players { get; set; }

        public List<int> WorldSeeds { get; set; }

        public Bot WinngingBot { get; set; }
    }
}
