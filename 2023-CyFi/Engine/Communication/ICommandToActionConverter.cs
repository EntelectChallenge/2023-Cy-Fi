using Domain.Models.Communication;

namespace Engine.Communication;

public interface ICommandToActionConverter
{
    public BotAction Convert(BotCommand botCommand);
}
