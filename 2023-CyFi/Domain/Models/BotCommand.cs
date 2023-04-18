using Domain.Enums;

namespace Domain.Models
{
    public class BotCommand
    {
        public Guid BotId { get; set; }
        public InputCommand Action { get; set; }
    }
}
