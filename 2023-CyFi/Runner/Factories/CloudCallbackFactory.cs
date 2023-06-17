using Domain.Enums;
using Domain.Models;

namespace Runner.Factories
{
    public class CloudCallbackFactory : ICloudCallbackFactory
    {
        private readonly AppSettings _appSettings;

        public CloudCallbackFactory(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public CloudCallback Build(CloudCallbackType callbackType, Exception? e, int? seed, int? ticks)
        {
            return callbackType switch
            {
                CloudCallbackType.Initializing => new CloudCallback
                {
                    MatchId = _appSettings.MatchId ?? "",
                    MatchStatus = "initializing",
                    MatchStatusReason = "Startup"
                },
                CloudCallbackType.Ready => new CloudCallback
                {
                    MatchId = _appSettings.MatchId ?? "",
                    MatchStatus = "ready",
                    MatchStatusReason = $"All Components connected and ready for bots. Waiting for bots to connect."
                },
                CloudCallbackType.Started => new CloudCallback
                {
                    MatchId = _appSettings.MatchId ?? "",
                    MatchStatus = "started",
                    MatchStatusReason = $"Match has started with bots",
                    Players = new List<CloudPlayer>(),
                },
                CloudCallbackType.Failed => new CloudCallback
                {
                    MatchId = _appSettings.MatchId ?? "",
                    MatchStatus = "failed",
                    MatchStatusReason = e.Message ?? "",
                    Players = new List<CloudPlayer>()
                },
                CloudCallbackType.Finished => new CloudCallback
                {
                    MatchId = _appSettings.MatchId ?? "",
                    MatchStatus = "finished",
                    MatchStatusReason = "Game Complete.",
                    Seed = seed.ToString() ?? "",
                    Ticks = ticks.ToString() ?? "",
                    Players = new List<CloudPlayer>(),
                },
                CloudCallbackType.LoggingComplete => new CloudCallback
                {
                    MatchId = _appSettings.MatchId ?? "",
                    MatchStatus = "logging_complete",
                    MatchStatusReason = "Game Complete. Logging Complete.",
                    Seed = seed.ToString() ?? "",
                    Ticks = ticks.ToString() ?? "",
                    Players = new List<CloudPlayer>(),
                },
                _ => throw new ArgumentOutOfRangeException(nameof(callbackType), callbackType, "Unknown Cloud Callback Type")
            };
        }
    }
}
