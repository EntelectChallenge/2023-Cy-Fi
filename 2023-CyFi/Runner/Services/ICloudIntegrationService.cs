using Domain.Enums;

namespace Runner.Services
{
    public interface ICloudIntegrationService
    {
        Task Announce(CloudCallbackType callbackType, Exception? e = null, int? seed = null, int? ticks = null);

        Task AnnounceNoOp(CloudCallbackType callbackType);

        void AddPlayer(int finalScore = 0, string playerId = "", int matchPoints = 0, int placement = 0, string participationId = "", string seed = "");

        void UpdatePlayer(string playerId, int? finalScore = null, int? matchPoints = null, int? placement = null);
    }
}