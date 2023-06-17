using CyFi.Entity;
using CyFi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CyFi.Factories
{
    public class BotFactory
    {
        public ILoggerFactory loggerFactory;

        public BotFactory(IOptions<CyFiGameSettings> gameSettings,
            ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        public virtual Bot CreateBot(string nickName, string connectionId)
        {
            ILogger<Bot> botLogger = loggerFactory.CreateLogger<Bot>();

            return new Bot(botLogger, nickName, connectionId);
        }
    }
}
