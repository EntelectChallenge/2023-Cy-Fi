using System;

namespace Domain.Models.Communication;

public class BotCommand
{
    public Guid BotId { get; set; }
    public int Type { get; set; }
    public string Payload { get; set; }
    // public Dictionary<string, string> Payload { get; set; }
}