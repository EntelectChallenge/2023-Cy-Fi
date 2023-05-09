namespace Domain.Objects
{
    public static class PlaceObjects
    {
        public static int CalculateLevelCollectableTotal(int Level)
        {
            //TODO : Get from state
            int MaxAmount = 100;
            // Taking into consideration 4 players , each requiring a different amount 
            // (100 + 90+ 60 +20 = 270)
            int PassingValue = 270;
            //Determine how many collectibles are needed based on the level and map size
            switch (Level)
            {
                case 0:
                    return MaxAmount * (PassingValue * 2) / 100;
                case 1:
                    return (int)(MaxAmount * (PassingValue * 1.7) / 100);
                case 2:
                    return (int)(MaxAmount * (PassingValue * 1.3) / 100);
                case 3:
                    return MaxAmount * (PassingValue * 1) / 100;
                default:
                    Console.Write($"Level {Level} passed does not exist");
                    throw new Exception("Level cannot be determined");
            }
        }

        /// <summary>
        /// Used when the map is not the base map
        /// Note: This might not be enough for all players to progress to the next level, since the platform length created might not be large enough for such.
        /// </summary>
        /// <param name="totalPlatformLength">Total platform length</param>
        /// <param name="Level">Level the collectibles are created for</param>
        /// <returns>Number of collectibles to generate</returns>
        /// <exception cref="Exception"></exception>
        public static int CalculateBasicLevelCollectableTotal(decimal totalPlatformLength, int Level)
        {
            switch (Level)
            {
                case 0:
                    return (int)totalPlatformLength / 4;
                case 1:
                    return (int)totalPlatformLength / 5;
                case 2:
                    return (int)totalPlatformLength / 6;
                case 3:
                    return (int)totalPlatformLength / 8;
                default:
                    Console.Write($"Level {Level} passed does not exist");
                    throw new Exception("Level cannot be determined");
            }
        }

        public static int CalculateLevelHazardsTotal(decimal totalPlatformLength, int Level)
        {
            //Determine how many hazards are needed based on the level
            switch (Level)
            {
                case 0:
                    return (int)Math.Floor(totalPlatformLength * 3 / 100);
                case 1:
                    return (int)Math.Floor(totalPlatformLength * 5 / 100);
                case 2:
                    return (int)Math.Floor(totalPlatformLength * 7 / 100);
                case 3:
                    return (int)Math.Floor(totalPlatformLength * 10 / 100);
                default:
                    Console.Write($"Level {Level} passed does not exist");
                    throw new Exception("Level cannot be determined");
            }
        }

        public static int CalculateBasicLevelHazardTotal(decimal totalPlatformLength, int Level)
        {
            switch (Level)
            {
                case 0:
                    return (int)totalPlatformLength / 10;
                case 1:
                    return (int)totalPlatformLength / 8;
                case 2:
                    return (int)totalPlatformLength / 5;
                case 3:
                    return (int)totalPlatformLength / 3;
                default:
                    Console.Write($"Level {Level} passed does not exist");
                    throw new Exception("Level cannot be determined");
            }
        }

    }
}
