using ReferenceBot.AI.States;
using Domain.Enums;
using Domain.Models;
using System.Collections.Generic;

namespace ReferenceBot.AI
{
    class BotStateMachine
    {
        private State CurrentState;

        public BotStateMachine()
        {
            CurrentState = new SearchingState(this);
        }

        public void ChangeState(State NewState)
        {
            CurrentState.ExitState(NewState);
            NewState.EnterState(CurrentState);
            CurrentState = NewState;
        }

        public InputCommand Update(BotStateDTO BotState, BotStateDTO? LastKnownState)
        {
            return CurrentState.Update(BotState, LastKnownState);
        }
    }
}