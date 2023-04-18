using Domain.Enums;
using Domain.Models;

namespace Runner.Factories
{
    public interface ICloudCallbackFactory
    {
        CloudCallback Build(CloudCallbackType callbackType);
    }
}