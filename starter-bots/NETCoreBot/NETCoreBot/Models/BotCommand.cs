using NETCoreBot.Enums;
using System;

namespace NETCoreBot.Models
{
    public class BotCommand
    {
        public Guid BotId { get; set; }
        public InputCommand Action { get; set; }
    }
}
