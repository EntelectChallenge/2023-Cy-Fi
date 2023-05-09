namespace NETCoreBot.Models
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

        public string PrintWindow()
        {
            if (HeroWindow == null)
            {
                return "";
            }

            var window = "";
            for (int y = HeroWindow[0].Length - 1; y >= 0; y--)
            {
                for (int x = 0; x < HeroWindow.Length; x++)
                {
                    window += $"{HeroWindow[x][y]}";
                }
                window += "\n";
            }
            return window;
        }
    }
}
