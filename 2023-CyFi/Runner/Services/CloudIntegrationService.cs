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

        public CloudIntegrationService(AppSettings appSettings, ILogger<CloudIntegrationService> logger, ICloudCallbackFactory cloudCallbackFactory)
        {
            _appSettings = appSettings;
            _logger = new GameLogger<CloudIntegrationService>(logger);
            _cloudCallbackFactory = cloudCallbackFactory;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _appSettings.ApiKey);
        }

        public async Task Announce(CloudCallbackType callbackType)
        {
            CloudCallback cloudPayload = _cloudCallbackFactory.Build(callbackType);
            _logger.Log(LogLevel.Information, $"Cloud Callback Initiated, Status: {cloudPayload.MatchStatus}, Callback player Count: {cloudPayload.Players?.Count}");

            try
            {
                var result = await _httpClient.PostAsync(_appSettings.ApiUrl, cloudPayload, new JsonMediaTypeFormatter());
                if (!result.IsSuccessStatusCode)
                {
                    _logger.Log(LogLevel.Warning, $"Received non-success status code from cloud callback. Code: {result.StatusCode}");
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Warning, $"Failed to make cloud callback with error: {e.Message}");
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

    }
}
