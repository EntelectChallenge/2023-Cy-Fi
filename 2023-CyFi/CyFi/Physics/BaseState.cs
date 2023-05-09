using Domain.Enums;

namespace CyFi.Physics;

public class BaseState
{
    public string name { get; }
    protected StateMachine stateMachine;

    public BaseState(string name, StateMachine stateMachine)
    {
        this.name = name;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() {}
    public virtual void UpdateInput(InputCommand inputCommand) {} // do we need this one?
    public virtual void UpdatePhysics() {}
    public virtual void Exit() {}
}