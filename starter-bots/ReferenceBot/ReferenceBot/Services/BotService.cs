using ReferenceBot.AI;
using Domain.Enums;
using Domain.Models;
using System;

namespace ReferenceBot.Services
{
  class BotService
  {
    private Guid BotId;
    private BotStateMachine BotFSM;
    private BotStateDTO LastKnownState;

    public BotService()
    {
        BotFSM = new();
    }

    public BotCommand ProcessState(BotStateDTO BotState)
    {
      InputCommand ActionToTake = BotFSM.Update(BotState, LastKnownState);
            LastKnownState = BotState;
      return new BotCommand
      {
        BotId = BotId,
        Action = ActionToTake,
      };
    }

    public void SetBotId(Guid NewBotId)
    {
      BotId = NewBotId;
    }
  }
}