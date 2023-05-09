using CyFi.Models;
using CyFi.RootState;
using Domain.Objects;
using Logger;
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

            return new WorldObject(
                mapSettings.Width,
                mapSettings.Height,
                mapSettings.Seed,
                fillThreshold,
                minPathWidth,
                maxPathWidth,
                minPathHeight,
                maxPathHeight,
                pathCleanupWidth,
                level,
                minConnections,
                maxConnections,
                worldLogger
            );
        }
    }
}
