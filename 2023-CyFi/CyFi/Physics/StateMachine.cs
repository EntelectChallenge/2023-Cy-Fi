using Domain.Components;
using Domain.Enums;

namespace CyFi.Physics;

public class StateMachine
{
    public BaseState? CurrentState { get; set; }
    
    public void Start()
    {
        CurrentState = GetInitialState();
        CurrentState?.Enter();
    }

    protected virtual BaseState? GetInitialState()
    {
        return null;
    }

    public void UpdateInput(InputCommand inputCommand)
    {
        Console.WriteLine("update input");
        CurrentState?.UpdateInput(inputCommand);
    }

    public void LateUpdate()
    {
        Console.WriteLine("update physics");
        CurrentState?.UpdatePhysics();
    }

    public void ChangeState(BaseState newState)
    {
        CurrentState?.Exit();
        
        CurrentState = newState;
        CurrentState?.Enter();
    }

    public Type GetStateType()
    {
        return CurrentState?.GetType();
    }
}