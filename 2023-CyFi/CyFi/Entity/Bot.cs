using CyFi.Models;
using Logger;
using Microsoft.Extensions.Logging;

namespace CyFi.Entity
{
    public class Bot
    {
        public HeroEntity Hero;

        public int CurrentLevel;

        public IGameLogger<Bot> Logger;

        public Guid Id;

        public string NickName;

        public string ConnectionId;

        public DateTime LastUpdated;

        public int TotalPoints { get; set; }

        //Needed for SignalR unit test
        public Bot()
        {
        }

        public Bot(CyFiGameSettings settings, ILogger<Bot> logger, string? nickName, string connectionId)
        {
            Id = Guid.NewGuid();
            Logger = new GameLogger<Bot>(logger);
            Hero = new HeroEntity(settings);
            NickName = nickName ?? "BotTheBuilder";
            ConnectionId = connectionId;
        }
    }
}
