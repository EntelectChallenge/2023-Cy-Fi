using CyFi.Models;
using Domain.Objects;
using Microsoft.Extensions.Logging;
using static CyFi.Settings.MapSettings;

namespace CyFi.Factories
{
    public class WorldFactory
    {
        public ILoggerFactory loggerFactory;

        public WorldFactory(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        public WorldObject CreateWorld(Map mapSettings, int level)
        {

            ILogger<WorldObject> worldLogger = loggerFactory.CreateLogger<WorldObject>();

            //          worldGenerator = WorldObject(width, height, 'seed', 50, 0.025, 0, 0, 0.13, 0.005, 1, 6, 9)
            return new WorldObject(
                mapSettings.Width,
                mapSettings.Height,
                mapSettings.Seed,
                fillThreshold,
                minPathWidth,
                maxPathWidth,
                minPathHeight[level],
                maxPathHeight[level],
                pathCleanupWidth,
                level,
                minConnections,
                maxConnections
            );
        }
    }
}
