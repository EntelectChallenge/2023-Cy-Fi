using CyFi.Inputs;
using CyFi.Physics;
using CyFi.Physics.Movement;
using CyFi.Physics.Utils;
using Domain.Enums;
using Domain.Objects;
using System.Drawing;
using static CyFi.Settings.GameSettings;

namespace CyFi.Entity
{
    public class HeroEntity : GameObject<HeroEntity>
    {
        public int Collected { get; set; }

        public DateTime Start;

        public InputCommand heroLatestAction;
        public Direction heroDirection;

        public int TimesDug { get; set; } = 0;

        public MovementSM MovementSm { get; set; }
        public List<Movements.RangeData> radarData { get; set; }


        public HeroEntity(Guid id) : base(new HeroInput(), new HeroPhysics(), id)
        {

            Width = 1;
            Height = 1;
            this.radarData = new List<Movements.RangeData>();
            MovementSm = new MovementSM(this);
            MovementSm.Start();
        }

        public Point[] HeroWindow()
        {
            int minWindowX = XPosition - heroWindowSizeX;
            int maxWindowX = XPosition + Width + 1 + heroWindowSizeX;
            int minWindowY = YPosition - heroWindowSizeY;
            int maxWindowY = YPosition + Height + 1 + heroWindowSizeY;

            return new Point[]
            {
                new Point(minWindowX, minWindowY),
                new Point(maxWindowX, maxWindowY),
            };
        }
        public Point[] HeroStealWindow(int stealRangeX, int StealRangeY)
        {
            int minWindowX = XPosition - stealRangeX;
            int maxWindowX = XPosition + Width + 1 + stealRangeX;
            int minWindowY = YPosition - StealRangeY;
            int maxWindowY = YPosition + Height + 1 + StealRangeY;

            return new Point[]
            {
                new Point(minWindowX, minWindowY),
                new Point(maxWindowX, maxWindowY),
            };
        }
    }
}