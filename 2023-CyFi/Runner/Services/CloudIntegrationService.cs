using Domain.Enums;
using Domain.Models;
using Logger;
using Microsoft.Extensions.Logging;
using Runner.Factories;
using System.Net.Http.Formatting;

namespace Runner.Services
{
    public class CloudIntegrationService : ICloudIntegrationService
    {
        private readonly AppSettings _appSettings;
        private IGameLogger<CloudIntegrationService> _logger;
        private readonly ICloudCallbackFactory _cloudCallbackFactory;
        private readonly HttpClient _httpClient;

        public List<CloudPlayer> players;

        public CloudIntegrationService(AppSettings appSettings, ILogger<CloudIntegrationService> logger, ICloudCallbackFactory cloudCallbackFactory)
        {
            _appSettings = appSettings;
            _logger = new GameLogger<CloudIntegrationService>(logger);
            _cloudCallbackFactory = cloudCallbackFactory;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _appSettings.ApiKey);
            players = new List<CloudPlayer>();
        }

        public async Task Announce(CloudCallbackType callbackType, Exception? e = null, int? seed = null, int? ticks = null)
        {
            CloudCallback cloudPayload = _cloudCallbackFactory.Build(callbackType, e, seed, ticks);
            if (cloudPayload.Players != null)
            {
                cloudPayload.Players = players;
            }
            _logger.Log(LogLevel.Information, $"Cloud Callback Initiated, Status: {cloudPayload.MatchStatus}, Callback player Count: {cloudPayload.Players?.Count ?? 0}");

            try
            {
                var result = await _httpClient.PostAsync(_appSettings.ApiUrl, cloudPayload, new JsonMediaTypeFormatter());
                if (!result.IsSuccessStatusCode)
                {
                    _logger.Log(LogLevel.Warning, $"Received non-success status code from cloud callback. Code: {result.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Warning, $"Failed to make cloud callback with error: {ex.Message}");
            }
        }

        public Task AnnounceNoOp(CloudCallbackType callbackType)
        {
            // no-op
            _logger.Log(LogLevel.Debug, $"Cloud Callback called with {callbackType}");
            var cloudCallbackPayload = _cloudCallbackFactory.Build(callbackType);
            _logger.Log(LogLevel.Debug, $"Cloud Callback No-opped, Status: {cloudCallbackPayload.MatchStatus}");
            return Task.CompletedTask;
        }

        public void AddPlayer(
            int finalScore = 0,
            string playerId = "",
            int matchPoints = 0,
            int placement = 0,
            string participationId = "",
            string seed = ""
        )
        {
            players.Add(new CloudPlayer()
            {
                FinalScore = finalScore,
                GamePlayerId = playerId,
                MatchPoints = matchPoints,
                Placement = placement,
                PlayerParticipantId = participationId,
                Seed = seed,
            });
        }

        public void UpdatePlayer(string playerId, int? finalScore = null, int? matchPoints = null, int? placement = null)
        {
            players.ForEach(player =>
            {
                if (player.GamePlayerId.Equals(playerId, StringComparison.InvariantCultureIgnoreCase))
                {
                    player.FinalScore = finalScore ?? player.FinalScore;
                    player.MatchPoints = matchPoints ?? player.MatchPoints;
                    player.Placement = placement ?? player.Placement;
                }
            });
        }
    }
}
