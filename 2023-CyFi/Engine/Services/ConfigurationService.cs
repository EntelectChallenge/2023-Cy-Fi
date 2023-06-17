using Domain.Configs;
using Engine.Extensions;
using Microsoft.Extensions.Options;

namespace Engine.Services
{
    public class ConfigurationService : IConfigurationService
    {
        public EngineConfig Value { get; set; }

        public ConfigurationService(IOptions<EngineConfig> engineOptions)
        {
            Value = new EngineConfig();
            engineOptions.Value.CopyPropertiesTo(Value);

            var seedEnvarString = Environment.GetEnvironmentVariable("WORLD_SEED");
            if (!string.IsNullOrWhiteSpace(seedEnvarString))
            {
                Value.WorldSeed = int.Parse(seedEnvarString);
            }
        }
    }

    public interface IConfigurationService
    {
        public EngineConfig Value { get; set; }
    }
}