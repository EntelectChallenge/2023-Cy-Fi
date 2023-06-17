using CyFi.Entity;
using CyFi.RootState;

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
