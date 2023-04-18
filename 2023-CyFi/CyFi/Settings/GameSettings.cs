namespace CyFi.Settings
{
    // This file is for game settings that we don't really want to mess with
    public static class GameSettings
    {
        public const int heroWindowSizeX = 16;
        public const int heroWindowSizeY = 10;
        public const float hazardLosePercentage = 0.1f;
        public static Dictionary<int, int> collectables = new()
        {
            {3, 100}, {2, 90}, {1, 60}, {0, 20}
        };
        public const int collectibleDigCount = 10;
    }
}
