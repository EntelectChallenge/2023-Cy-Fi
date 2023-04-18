using Domain.Enums;
using Domain.Models;

namespace CyFi.Models
{
    public class CyFiCommand : BotCommand
    {
        public CyFiCommand(Guid id, InputCommand action)
        {
            BotId = id;
            Action = action;
        }
    }
}
