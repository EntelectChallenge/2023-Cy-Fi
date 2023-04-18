using CyFi.Models;
using Domain.Objects;
using System.Drawing;
using CyFi.Inputs;
using CyFi.Physics;
using CyFi.Physics.Movement;
using static CyFi.Settings.GameSettings;

namespace CyFi.Entity
{
    public class HeroEntity : GameObject<HeroEntity>
    {
        public int Collected;
        public DateTime Start;
        public DateTime End;

        private readonly CyFiGameSettings settings;

        public int TimesDug = 0;

        public MovementSM MovementSm;

        public HeroEntity(CyFiGameSettings settings) : base(new HeroInput(), new HeroPhysics())
        {
            Width = 1;
            Height = 1;
            this.settings = settings;
            MovementSm = new MovementSM(this);
            MovementSm.Start();
        }

        public Point[] HeroWindow()
        {
            int minWindowX = XPosition - heroWindowSizeX;
            int maxWindowX = XPosition + Width + heroWindowSizeX;
            int minWindowY = YPosition - heroWindowSizeY;
            int maxWindowY = YPosition + heroWindowSizeY;

            return new Point[]
            {
                new Point(minWindowX, minWindowY),
                new Point(maxWindowX, maxWindowY),
            };
        }
    }
}