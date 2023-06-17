using Domain.Enums;
using Logger;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;

namespace Domain.Objects
{
    public class WorldObject
    {
        private readonly IGameLogger<WorldObject> Logger;

        private readonly Random random;

        private readonly int pathWidthInt;

        public int[][] map { get; set; }

        private List<ChangeLogItem> changeLog;
        public List<ChangeLogItem> ChangeLog
        {
            get
            {
                //  var changeLogTemp = changeLog;
                //changeLog.Clear();
                return changeLog;

            }
            set
            {
                changeLog = value;
            }
        }

        private bool[][] pathMask;

        private decimal totalPlatformLength;

        public readonly int width;
        public readonly int height;

        private Tuple<int, int>[][] pathHeights;
        private Tuple<int, int>[] pathEndPoints;

        public int level { get; set; }

        public List<List<Tuple<int, int>>> randomPaths = new();

        private readonly Dictionary<int, int> pathLookup = new()
        {
            {0, 5},
            {1, 4},
            {2, 3},
            {3, 2}
        };

        public Point start;

        public struct ChangeLogItem
        {
            public int pointX { get; set; }
            public int pointY { get; set; }
            public int tileType { get; set; }
        }

        public WorldObject(List<ChangeLogItem> changeLog)
        {
            this.ChangeLog = changeLog;
        }

        public WorldObject(
            int width,
            int height,
            object? seed,
            int fillThreshold,
            float minPathWidth,
            float maxPathWidth,
            float minPathHeight,
            float maxPathHeight,
            float pathCleanupWidth,
            int level,
            int minConnections,
            int maxConnections
        )
        {
            this.ChangeLog = new List<ChangeLogItem>();

            pathMask = new bool[width][];
            this.width = width;
            this.height = height;
            this.totalPlatformLength = 0;

            // Generate seed value
            seed = seed is null ? (int)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() : seed;

            // Set seed
            random = new Random(GetSeedFromObject(seed));

            // Initialize arrays
            map = new int[width][];

            for (int x = 0; x < width; x++)
            {
                map[x] = new int[height];
                pathMask[x] = new bool[height];
            }

            pathWidthInt = (int)(width * pathCleanupWidth);

            this.level = level;

            int numberOfPaths = pathLookup[level];

            GeneratePaths(numberOfPaths, minConnections, maxConnections, minPathWidth, maxPathWidth, minPathHeight, maxPathHeight);

            GenerateNoise(fillThreshold);

            Cleanup();

            GenerateObjectsOnPath(level);

            GetStartingPoint(level);
        }

        /// <summary>
        /// Converts an object into an integer seed for RNG.
        /// </summary>
        /// <param name="seedObj">The seed object.</param>
        /// <returns>The 32 bit integer representation</returns>
        private Int32 GetSeedFromObject(object seedObj)
        {
            string seedStr = seedObj.ToString() ?? string.Empty;
            var algo = SHA1.Create();
            var hash = BitConverter.ToInt32(algo.ComputeHash(Encoding.UTF8.GetBytes(seedStr)));
            return hash;
        }

        /// <summary>
        /// Generate a number of random paths through the map.
        /// </summary>
        /// <param name="numberOfPaths">Number of paths to generate</param>
        /// <param name="pathHeight">Height of the paths</param>
        /// <param name="minConnections">Minimum connections between paths</param>
        /// <param name="maxConnections">Maximum connections between paths</param>
        /// <param name="minPathLength">
        /// Minimum length of a horizontal platform, as a percentage of map width
        /// </param>
        /// <param name="maxPathLength">
        /// Maximum length of a horizontal platform, as a percentage of map width
        /// </param>
        private void GeneratePaths(int numberOfPaths, int minConnections, int maxConnections, float minPathLength, float maxPathLength, float minPathHeight, float maxPathHeight)
        {
            int minPlatformLength = (int)(width * minPathLength);
            int maxPlatformLength = (int)(width * maxPathLength);

            // Divide the map into numberOfPaths paths.
            int pathHeight = height / numberOfPaths;
            int lastX = 10; // Last X position.

            // Minimum and maximum ladder heights are based on the height of each path.
            int minPlatformHeight = (int)(pathHeight * minPathHeight);
            int maxPlatformHeight = (int)(pathHeight * maxPathHeight);

            pathHeights = new Tuple<int, int>[numberOfPaths][];
            pathEndPoints = new Tuple<int, int>[numberOfPaths];

            // Iterate over each path
            for (int path = 0; path < numberOfPaths; path++)
            {
                pathHeights[path] = new Tuple<int, int>[width];

                int yPadding = (int)(pathHeight * 0.05);
                int xPadding = (int)(width * 0.01);

                int pathStartY = (path * pathHeight) + yPadding + 8;
                int pathEndY = ((path + 1) * pathHeight) - yPadding;

                List<Tuple<int, int>> randomPath = new(); // Generate new path.
                                                          // Add some padding so paths aren't right on top of each other.


                // Random Y position within the slice.
                int startY = random.Next(pathStartY, pathEndY);

                // Generate start and end points.
                int startX;
                int endX;
                // If we're in the right half of the map.
                if (lastX >= width / 2)
                {
                    // Generate an end point in the first 10% of the map.
                    endX = random.Next(xPadding, (int)(width * 0.1));
                    // Generate a start point in the last 10% of the map.
                    startX = random.Next((int)(width * 0.9), width - xPadding);
                    pathEndPoints[path] = new(endX, startX);
                }
                else                         // We're in the left half.
                {
                    // Generate a start point in the first 10% of the map.
                    startX = random.Next(xPadding, (int)(width * 0.1));
                    // Generate an end point in the last 10% of the map.
                    endX = random.Next((int)(width * 0.9), width - xPadding);
                    pathEndPoints[path] = new(startX, endX);
                }

                // Generate a path leading to the end.
                int xDiff = endX > startX ? 1 : -1;

                // Check if we've reached (or passed) the end.
                bool isXAtEnd(int x)
                {
                    return xDiff == -1 ? x <= endX : x >= endX;
                }

                //Use the current path lenght
                int currentPathLength = 0;
                int currentY = startY;
                int currentPathStartX = startX;
                //Path Generation
                for (int x = startX; !isXAtEnd(x); x += xDiff)
                {
                    map[x][currentY] = (int)ObjectType.Platform;

                    randomPath.Add(new(x, currentY));
                    currentPathLength++;
                    float dropChance = 0.0f;
                    //Do we need this becuase all the platforms will tecnically be the same length if we adhear to the minimum platform
                    //length 
                    if (currentPathLength >= minPlatformLength)
                    {
                        // We've reached minimum platform length, we randomly
                        // decide if we go up or down.
                        dropChance = 0.5f;
                    }
                    else if (currentPathLength >= maxPlatformLength)
                    {
                        // We've reached maximum platform length, we need to go up
                        // or down.
                        dropChance = 1.0f;
                    }

                    // Check if we need to drop.
                    if (random.NextDouble() > dropChance)
                    {
                        // We're not dropping so continue generating platform
                        pathHeights[path][x] = new(currentY, currentY); // should this be currentX?
                        continue;
                    }

                    GenerateHorizontalPathMask(currentPathStartX, x, currentY);

                    totalPlatformLength += currentPathLength;
                    // Reset path length.
                    //IS it necessary to set this twice??
                    currentPathLength = 0;

                    // Calculate if we go up or down.
                    int distanceToTopOfPath = pathEndY - currentY;
                    int distanceToBottomOfPath = currentY - pathStartY;
                    float chanceToGoUp;
                    if (distanceToTopOfPath < minPlatformHeight)
                    {
                        // If we're too close to the top, go down.
                        chanceToGoUp = 0.0f;
                    }
                    else if (distanceToBottomOfPath < minPlatformHeight)
                    {
                        // If we're too close to the bottom, go up.
                        chanceToGoUp = 1.0f;
                    }
                    else
                    {
                        // Otherwise 50-50
                        chanceToGoUp = 0.5f;
                    }

                    bool goingUp = random.NextDouble() < chanceToGoUp;
                    int nextY;
                    if (goingUp)
                    {
                        int minimumNextY = currentY + minPlatformHeight;
                        int maximumNextY = currentY + maxPlatformHeight;
                        // Generate ladder end point.
                        nextY = random.Next(minimumNextY, maximumNextY);
                    }
                    else
                    {
                        int maximumNextY = currentY - minPlatformHeight;
                        int minimumNextY = currentY - maxPlatformHeight;
                        // Generate ladder end point.
                        nextY = random.Next(minimumNextY, maximumNextY);
                    }
                    // Clamp to make sure we don't go above/below path.
                    nextY = Math.Clamp(nextY, pathStartY, pathEndY);

                    GenerateVerticalPathMask(x, currentY, nextY);

                    pathHeights[path][x] = goingUp ? new(currentY, nextY) : new(nextY, currentY);

                    // Generate the ladder.
                    bool isAtEnd(int y)
                    {
                        return goingUp ? y >= nextY : y <= nextY;
                    }
                    int yDiff = goingUp ? 1 : -1;
                    for (; !isAtEnd(currentY); currentY += yDiff)
                    {
                        // Generate the stuff.
                        map[x][currentY] = (int)ObjectType.Ladder;
                        randomPath.Add(new(x, currentY));
                    }

                    map[x][currentY] = (int)ObjectType.Ladder;
                    randomPath.Add(new(x, currentY));

                    currentPathStartX = x;
                }

                GenerateHorizontalPathMask(currentPathStartX, endX, currentY);
                lastX = endX;
                pathHeights[path][endX] ??= new(currentY, currentY); // '??' left operand is never null according to nullable reference types' annotations - according to rider
                randomPaths.Add(randomPath);
            }

            GenerateConnectingPaths(numberOfPaths, minConnections, maxConnections, minPlatformLength);
        }

        private void GenerateObjectsOnPath(int level)
        {
            //How many collectibles should be placed
            int collectibleAmount;
            //How many hazards should be placed
            int hazardAmount;

            if (width >= 500 && height >= 200)
            {
                //Calculating how many collectibles should be placed
                collectibleAmount = PlaceObjects.CalculateLevelCollectableTotal(level);
                //Calculating how many hazards should be placed
                hazardAmount = PlaceObjects.CalculateLevelHazardsTotal(totalPlatformLength, level);
            }
            else
            {
                //Calculating how many collectibles should be placed
                collectibleAmount = PlaceObjects.CalculateBasicLevelCollectableTotal(totalPlatformLength, level);
                //Calculating how many hazards should be placed
                hazardAmount = PlaceObjects.CalculateBasicLevelHazardTotal(totalPlatformLength, level);
            }

            //Determine how far apart the collectibles should be placed
            var collectibleSpacing = (int)Math.Floor(totalPlatformLength / collectibleAmount);

            //Determine how far apart the hazards should be placed
            var hazardSpacing = (int)Math.Floor(totalPlatformLength / hazardAmount);

            //Keeping track of objects placed
            var totalCollectablesAmountPlaced = 0;
            var totalHazardsAmountPlaced = 0;

            var spaceConsideration = collectibleSpacing / 2;

            while ((totalCollectablesAmountPlaced < collectibleAmount) && (totalHazardsAmountPlaced < hazardAmount))
            {
                foreach (List<Tuple<int, int>> paths in randomPaths)
                {
                    if (totalCollectablesAmountPlaced <= collectibleAmount)
                    {
                        for (int i = 1;
                            (i < paths.Count - 1) && (totalCollectablesAmountPlaced < collectibleAmount);
                             i += collectibleSpacing)
                        {
                            //Place Collectible 
                            //Check you are not placing the object on the ladders
                            
                            if (
                                map[paths[i].Item1][paths[i].Item2] != (int)ObjectType.Ladder && paths[i].Item1 <= this.width && paths[i].Item2 + 2 < this.height
                                 )
                            {
                                map[paths[i].Item1][paths[i].Item2 + 2] = (int)ObjectType.Collectible;
                                totalCollectablesAmountPlaced += 1;
                            }
                        }

                    }
                    if (totalHazardsAmountPlaced <= hazardAmount)
                    {
                        for (int i = 1 + spaceConsideration;
                            (i < paths.Count - spaceConsideration - 1) && (totalHazardsAmountPlaced < hazardAmount);
                             i += hazardSpacing)
                        {
                            //Place Hazard 
                            //Check you are not placing the object on the ladders
                            if (
                                paths.Contains(paths[i + spaceConsideration]) &&
                                paths.Contains(paths[i - spaceConsideration]) &&
                                paths[i].Item2 == paths[i + 1 + spaceConsideration].Item2 &&
                                paths[i].Item2 == paths[i - 1 - spaceConsideration].Item2 &&
                                paths[i].Item1 != paths[i + 1 + spaceConsideration].Item1 &&
                                paths[i].Item1 != paths[i - 1 - spaceConsideration].Item1
                                )
                            {
                                if (random.Next(0, 1) != 0)
                                {
                                    map[paths[i + spaceConsideration].Item1][paths[i].Item2] = (int)ObjectType.Hazard;
                                }
                                else
                                {
                                    map[paths[i - spaceConsideration].Item1][paths[i].Item2] = (int)ObjectType.Hazard;
                                }

                                totalHazardsAmountPlaced += 1;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generate ladders connecting each path together.
        /// </summary>
        /// <param name="numberOfPaths">The number of paths we have generated</param>
        /// <param name="minConnections">The minimum number of connections between paths</param>
        /// <param name="maxConnections">The maximum number of connections between paths</param>
        /// <param name="minPlatformLength">The minimum length of each platform</param>
        private void GenerateConnectingPaths(int numberOfPaths, int minConnections, int maxConnections, int minPlatformLength)
        {
            // Generate connecting paths.
            for (int path = 0; path < numberOfPaths - 1; path++)
            {
                // Choose number of connecting paths.
                int numberOfConnections = random.Next(minConnections, maxConnections);

                Tuple<int, int> thisPathEndPoints = pathEndPoints[path];
                Tuple<int, int> upperPathEndPoints = pathEndPoints[path + 1];

                int bothPathsStartX = Math.Max(thisPathEndPoints.Item1, upperPathEndPoints.Item1);
                int bothPathsEndX = Math.Min(thisPathEndPoints.Item2, upperPathEndPoints.Item2);
                int bothPathsWidth = bothPathsEndX - bothPathsStartX;

                // Divide the map into numberOfConnections sections.
                int sectionWidth = bothPathsWidth / numberOfConnections;
                for (int section = 0; section < numberOfConnections; section++)
                {
                    int sectionStartX = bothPathsStartX + (section * sectionWidth);
                    int sectionEndX = Math.Clamp(bothPathsStartX + ((section + 1) * sectionWidth), sectionStartX, bothPathsEndX);
                    int bottomConnectionX = random.Next(sectionStartX, sectionEndX);
                    int topConnectionX = random.Next(sectionStartX, sectionEndX);

                    // Make sure that the connections are at least some distance away from each other.
                    while (Math.Abs(topConnectionX - bottomConnectionX) < minPlatformLength / numberOfConnections)
                    {
                        topConnectionX = random.Next(sectionStartX, sectionEndX);
                    }

                    Tuple<int, int>[] thisPath = pathHeights[path];
                    Tuple<int, int>[] upperPath = pathHeights[path + 1];

                    int bottomPathY = thisPath[bottomConnectionX].Item2;
                    int topPathY = upperPath[topConnectionX].Item1;

                    int bottomPathYAtTopConnectionX = thisPath[topConnectionX].Item2;
                    int topPathYAtBottomConnectionX = upperPath[bottomConnectionX].Item1;


                    int lowestYOfTopPath = Math.Min(topPathY, topPathYAtBottomConnectionX);
                    int highestYOfBottomPath = Math.Max(bottomPathY, bottomPathYAtTopConnectionX);
                    int distanceBetweenClosestPoints = lowestYOfTopPath - highestYOfBottomPath;

                    int minPlatformY = (int)(highestYOfBottomPath + (distanceBetweenClosestPoints * 0.3));
                    int maxPlatformY = (int)(lowestYOfTopPath - (distanceBetweenClosestPoints * 0.3));

                    // Generate a point somewhere in between the two paths
                    // to make a horizontal platform.
                    int platformYPosition = random.Next(minPlatformY, maxPlatformY);

                    List<Tuple<int, int>> randomPath = new();
                    int currentX = bottomConnectionX;
                    int currentY = bottomPathY;

                    GenerateVerticalPathMask(currentX, currentY, platformYPosition);

                    // Generate the first ladder.
                    for (; currentY < platformYPosition; currentY++)
                    {
                        map[currentX][currentY] = (int)ObjectType.Ladder;
                        randomPath.Add(new(currentX, currentY));
                    }
                    map[currentX][currentY] = (int)ObjectType.Ladder;

                    int xDiff = (currentX < topConnectionX) ? 1 : -1;

                    GenerateHorizontalPathMask(currentX, topConnectionX, currentY);

                    // Generate platform in the middle.
                    for (; currentX != topConnectionX; currentX += xDiff)
                    {
                        totalPlatformLength++;

                        map[currentX][currentY] = (int)ObjectType.Platform;
                        randomPath.Add(new(currentX, currentY));
                    }

                    GenerateVerticalPathMask(currentX, currentY, topPathY);

                    // Generate the second ladder.
                    for (; currentY < topPathY; currentY++)
                    {
                        map[currentX][currentY] = (int)ObjectType.Ladder;
                        randomPath.Add(new(currentX, currentY));
                    }
                    map[currentX][currentY] = (int)ObjectType.Ladder;
                    randomPath.Add(new(currentX, currentY));
                    randomPaths.Add(randomPath);
                }
            }

        }

        /// <summary>
        /// Glob together filled and unfilled pixels
        /// </summary>
        private void Cleanup()
        {
            // Left to right pass, bottom to top
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    if (pathMask[x][y])
                    {
                        continue;
                    }
                    int neighbors = CountFilledNeighbors(x, y);
                    if (neighbors > 4)
                        map[x][y] = (int)ObjectType.Solid;
                    else if (neighbors < 3)
                        map[x][y] = (int)ObjectType.Air;
                }
            }

            // Right to left, top to bottom
            for (int x = width - 2; x > 0; x--)
            {
                for (int y = height - 2; y > 0; y--)
                {
                    if (pathMask[x][y])
                    {
                        continue;
                    }
                    int neighbors = CountFilledNeighbors(x, y);
                    if (neighbors > 4)
                        map[x][y] = (int)ObjectType.Solid;
                    else if (neighbors < 3)
                        map[x][y] = (int)ObjectType.Air;
                }
            }

            // Clean orphaned pixels
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    if (pathMask[x][y])
                    {
                        continue;
                    }
                    if (map[x][y] == 0) map[x][y] = (CountFilledNeighbors(x, y) == 8) ? (int)ObjectType.Solid : map[x][y];
                }
            }
        }

        /// <summary>
        /// Generate a noise filled map
        /// </summary>
        /// <param name="fillThreshold"></param>
        private void GenerateNoise(int fillThreshold)
        {
            // Randomly fill map with noise
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    map[x][y] = (pathMask[x][y] == true || random.Next(0, 100) >= fillThreshold) ? map[x][y] : (int)ObjectType.Solid;
                }
            }
        }

        /// <summary>
        /// Generate a path mask around a horizontal path.
        /// </summary>
        /// <param name="startX">The starting x coordinate of the path</param>
        /// <param name="endX">The ending x coordinate of the path</param>
        /// <param name="platformY">The y coordinate of the path</param>
        private void GenerateHorizontalPathMask(int startX, int endX, int platformY)
        {
            //TODO: count total platforms

            // Normalise to left -> right.
            if (endX < startX)
            {
                (startX, endX) = (endX, startX);
            }

            int clampedStartX = Math.Clamp(startX - pathWidthInt, 0, width - 1);
            int clampedEndX = Math.Clamp(endX + pathWidthInt, 0, width - 1);

            int clampedStartY = Math.Clamp(platformY - pathWidthInt, 0, height - 1);
            int clampedEndY = Math.Clamp(platformY + pathWidthInt, 0, height - 1);
            for (int x = clampedStartX; x <= clampedEndX; x++)
            {
                for (int y = clampedStartY; y <= clampedEndY; y++)
                {
                    pathMask[x][y] = true;
                }
            }
        }

        /// <summary>
        /// Generate a path mask around a vertical path.
        /// </summary>
        /// <param name="platformX">The x coordinate of the path</param>
        /// <param name="startY">The starting y coordinate of the path</param>
        /// <param name="endY">The ending y coordinate of the path</param>
        private void GenerateVerticalPathMask(int platformX, int startY, int endY)
        {
            // Normalise to top -> bottom.
            if (endY < startY)
            {
                (startY, endY) = (endY, startY);
            }

            int clampedStartY = Math.Clamp(startY - pathWidthInt, 0, height - 1);
            int clampedEndY = Math.Clamp(endY + pathWidthInt, 0, height - 1);

            int clampedStartX = Math.Clamp(platformX - pathWidthInt, 0, width - 1);
            int clampedEndX = Math.Clamp(platformX + pathWidthInt, 0, width - 1);
            for (int y = clampedStartY; y <= clampedEndY; y++)
            {
                for (int x = clampedStartX; x <= clampedEndX; x++)
                {
                    pathMask[x][y] = true;
                }
            }
        }

        /// <summary>
        /// Get a pixel's eight surrounding neighbors
        /// </summary>
        /// <param name="x">Pixel's X position</param>
        /// <param name="y">Pixel's Y position</param>
        /// <returns></returns>
        private int CountFilledNeighbors(int x, int y)
        {
            int neighbors = 0;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    neighbors += map[i + x][j + y];
                }
            }
            neighbors -= map[x][y];

            return neighbors;
        }

        /// <summary>
        /// Gets the starting point for a level based on each level's configuration and the 
        /// nearest path to that point
        /// </summary>
        /// <param name="level"></param>
        private void GetStartingPoint(int level)
        {
            Point location = new(0, 0);
            switch (level)
            {
                case 2:
                    location = new Point(width, 0);
                    break;
                case 3:
                case 4:
                    location = new Point(width / 2, height / 2);
                    break;
                default:
                    break;
            }

            Tuple<int, int> startingPoint = randomPaths.SelectMany(p => p).OrderBy(pathPoint => Math.Pow(location.X - pathPoint.Item1, 2) + Math.Pow(location.Y - pathPoint.Item2, 2)).First();

            if (map[startingPoint.Item1][startingPoint.Item2] == (int)ObjectType.Hazard ||
                map[startingPoint.Item1 + 1][startingPoint.Item2] == (int)ObjectType.Hazard)
            {
                map[startingPoint.Item1][startingPoint.Item2] = (int)ObjectType.Platform;
                map[startingPoint.Item1 + 1][startingPoint.Item2] = (int)ObjectType.Platform;
            }

            start = new(startingPoint.Item1, startingPoint.Item2 + 1);

            Console.WriteLine($"Start position {start} ");
        }

        /// <summary>
        /// Digs the tile at the given coordinates
        /// </summary>
        /// <param name="boundingBox">The bounding box to clear out</param>
        /// <returns>Whether or not the digging was successful</returns>
        public bool Dig(Point[] boundingBox)
        {
            var bottomLeft = boundingBox[0];
            var topRight = boundingBox[3];

            var hasGround = false;
            for (int x = bottomLeft.X; x <= topRight.X; x++)
            {
                for (int y = bottomLeft.Y; y <= topRight.Y; y++)
                {
                    int tile = map[x][y];
                    if (tile == (int)ObjectType.Solid)
                        hasGround = true;
                }
            }

            if (!hasGround)
                return false;

            for (int x = bottomLeft.X; x <= topRight.X; x++)
            {
                for (int y = bottomLeft.Y; y <= topRight.Y; y++)
                {
                    if (map[x][y] == (int)ObjectType.Solid)
                    {
                        map[x][y] = (int)ObjectType.Air;
                        ChangeLog.Add(new ChangeLogItem { pointX = x, pointY = y, tileType = map[x][y] });
                    }
                }
            }
            return true;
        }

        public static void SaveSate(int[][] value)
        {
            Console.WriteLine("Value changed");
        }
    }
}
