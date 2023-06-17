using Logger;
using Microsoft.Extensions.Logging;

namespace CyFi.Entity
{
    public class Bot
    {
        public HeroEntity Hero { get; set; }

        private int _currentLevel;

        public int CurrentLevel
        {
            get
            {
                return _currentLevel;
            }
            set
            {
                _currentLevel = value;
            }
        }

        public IGameLogger<Bot> Logger;

        public Guid Id { get; set; }

        public string NickName { get; set; }

        public string ConnectionId { get; set; }

        public DateTime LastUpdated;


        public int TotalPoints { get; set; }

        //Needed for SignalR unit test
        public Bot() { }


        public Bot(ILogger<Bot> logger, string? nickName, string connectionId, Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();
            Logger = new GameLogger<Bot>(logger);
            Hero = new HeroEntity(Id);
            NickName = nickName ?? Id.ToString();
            ConnectionId = connectionId;
        }

    }
}
