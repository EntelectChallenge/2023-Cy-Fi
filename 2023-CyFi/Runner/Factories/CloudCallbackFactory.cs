using Domain.Enums;
using Domain.Models;
using Newtonsoft.Json;

namespace Runner.Factories
{
    public class CloudCallbackFactory : ICloudCallbackFactory
    {
        private readonly AppSettings _appSettings;

        public CloudCallbackFactory(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public CloudCallback Build(CloudCallbackType callbackType)
        {
            return callbackType switch
            {
                CloudCallbackType.Initializing => new CloudCallback
                {
                    MatchId = _appSettings.MatchId,
                    MatchStatus = "initializing",
                    MatchStatusReason = "Startup"
                },
                CloudCallbackType.Ready => new CloudCallback
                {
                    MatchId = _appSettings.MatchId,
                    MatchStatus = "ready",
                    MatchStatusReason = $"All Components connected and ready for bots. Waiting for bots to connect."
                },
                CloudCallbackType.Started => new CloudCallback
                {
                    MatchId = _appSettings.MatchId,
                    MatchStatus = "started",
                    MatchStatusReason = $"Match has started with bots"
                },
                CloudCallbackType.Failed => new CloudCallback
                {
                    MatchId = _appSettings.MatchId,
                    MatchStatus = "failed",
                    MatchStatusReason = "Failure reason", //TODO
                    Players = MakeFailedPlayerList()
                },
                CloudCallbackType.Finished => new CloudCallback
                {
                    MatchId = _appSettings.MatchId,
                    MatchStatus = "finished",
                    MatchStatusReason = "Game Complete.",
                    Seed = "123", //TODO
                    Ticks = "1", //TODO
                    Players = MakePlayerList()
                },
                CloudCallbackType.LoggingComplete => new CloudCallback
                {
                    MatchId = _appSettings.MatchId,
                    MatchStatus = "logging_complete",
                    MatchStatusReason = "Game Complete. Logging Complete.",
                    Seed = "123", //TODO
                    Ticks = "1", //TODO
                    Players = MakePlayerList()
                },
                _ => throw new ArgumentOutOfRangeException(nameof(callbackType), callbackType, "Unknown Cloud Callback Type")
            };
        }

        private List<CloudPlayer> MakeFailedPlayerList()
        {
            throw new NotImplementedException();
        }

        private List<CloudPlayer> MakePlayerList()
        {
            throw new NotImplementedException();
        }
    }
}
