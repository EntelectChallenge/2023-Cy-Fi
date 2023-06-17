using CyFi.Entity;
using Domain.Enums;
using Domain.Objects;

namespace CyFi.Runner
{
    public class BotStateDTO
    {
        public int CurrentLevel { get; set; }
        public string CurrentState { get; set; }
        public string ConnectionId { get; set; }

        public int Collected { get; set; }

        public string ElapsedTime { get; set; }
        public int GameTick { get; set; }

        public int[][] HeroWindow { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        //  public Dictionary<int,int> radarData { get; set; }
        public int[][] RadarData { get; set; }

        public BotStateDTO()
        {

        }

        public BotStateDTO(Bot bot, List<Bot> opposingBots, HeroEntity hero, WorldObject world, int gameTick)
        {
            this.ConnectionId = bot.ConnectionId;
            this.CurrentLevel = bot.CurrentLevel;
            this.CurrentState = hero.MovementSm.CurrentState.name;
            this.Collected = hero.Collected;
            this.ElapsedTime = bot.LastUpdated.Subtract(hero.Start).ToString("g");
            this.GameTick = gameTick;

            X = hero.XPosition;
            Y = hero.YPosition;

            this.RadarData = hero.radarData.Select(data => new int[] {
                data.DirectionFromOpponent,
                data.PercentageDistance
            }).ToArray();

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
                    }
                    else
                    {
                        this.HeroWindow[x][y] = world.map[xPos][yPos];

                        opposingBots.ForEach(oBot =>
                        {
                            if (oBot.Hero.XPosition == xPos && oBot.Hero.YPosition == yPos ||
                                oBot.Hero.XPosition == xPos - 1 && oBot.Hero.YPosition == yPos ||
                                oBot.Hero.XPosition == xPos && oBot.Hero.YPosition + 1 == yPos ||
                                oBot.Hero.XPosition == xPos - 1 && oBot.Hero.YPosition + 1 == yPos)
                            {
                                if ((x > 0 && x < windowLength - 1) &&
                                    (y > 0 && y < windowHeight - 1))
                                {
                                    this.HeroWindow[x][y] = (int)ObjectType.Opponent;
                                }
                            }
                        });
                    }
                }
            }
        }
    }
}
