using NETCoreBot.Models;
using System;

namespace NETCoreBot.Services
{
    public class BotService
    {
        private Guid botId;
        private BotStateDTO botState;

        public Guid GetBotId()
        {
            return botId;
        }

        public void SetBotId(Guid id)
        {
            botId = id;
        }

        public BotStateDTO GetBotState()
        {
            return botState;
        }

        public void SetBotState(BotStateDTO botState)
        {
            this.botState = botState;
        }
    }
}
