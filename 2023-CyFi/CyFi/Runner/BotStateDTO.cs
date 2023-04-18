using CyFi.Entity;
using Domain.Enums;
using Domain.Objects;

namespace CyFi.Runner
{
    public class BotStateDTO
    {
        public int CurrentLevel { get; set; }
        public string ConnectionId { get; set; }

        public int Collected { get; set; }

        public string ElapsedTime { get; set; }

        public int[][] HeroWindow { get; set; }

        public int X { get; set; }
        public int Y { get; set; }

        public BotStateDTO()
        {

        }

        public BotStateDTO(Bot bot, HeroEntity hero, WorldObject world)
        {
            this.ConnectionId = bot.ConnectionId;
            this.CurrentLevel = bot.CurrentLevel;
            this.Collected = hero.Collected;
            this.ElapsedTime = hero.End.Subtract(hero.Start).ToString("g");

            X = hero.XPosition;
            Y = hero.YPosition;

            var coordinates = hero.HeroWindow();
            int windowLength = Math.Abs(coordinates[1].X - coordinates[0].X);
            int windowHeight = Math.Abs(coordinates[1].Y - coordinates[0].Y);
            this.HeroWindow = new int[windowLength][];
            for (int x = 0; x < windowLength; x++)
            {
                this.HeroWindow[x] = new int[windowHeight];
                for (int y = 0; y < windowHeight; y++)
                {
                    var xPos = coordinates[0].X + x;
                    var yPos = coordinates[0].Y + y;
                    if (xPos < 0 || yPos < 0 || xPos >= world.width || yPos >= world.height)
                    {
                        this.HeroWindow[x][y] = (int)ObjectType.Solid;
                    } else
                    {
                        this.HeroWindow[x][y] = world.map[xPos][yPos];
                    }
                }
            }
        }
    }
}
