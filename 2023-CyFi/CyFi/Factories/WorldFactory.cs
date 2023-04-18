using CyFi.Models;
using Domain.Objects;
using static CyFi.Settings.MapSettings;

namespace CyFi.Factories
{
    public class WorldFactory
    {
        public static WorldObject CreateWorld(Map mapSettings, int level)
        {
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
                maxConnections
            );
        }
    }
}
