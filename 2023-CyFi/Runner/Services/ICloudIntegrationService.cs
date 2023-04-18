using Domain.Enums;

namespace Runner.Services
{
    public interface ICloudIntegrationService
    {
        Task Announce(CloudCallbackType callbackType);

        Task AnnounceNoOp(CloudCallbackType callbackType);
    }
}