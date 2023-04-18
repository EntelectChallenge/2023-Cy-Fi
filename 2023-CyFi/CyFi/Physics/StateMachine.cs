using Domain.Components;
using Domain.Enums;

namespace CyFi.Physics;

public class StateMachine
{
    public BaseState? currentState { get; set; }
    
    public void Start()
    {
        currentState = GetInitialState();
        currentState?.Enter();
    }

    protected virtual BaseState? GetInitialState()
    {
        return null;
    }

    public void UpdateInput(InputCommand inputCommand)
    {
        Console.WriteLine("update input");
        currentState?.UpdateInput(inputCommand);
    }

    public void LateUpdate()
    {
        Console.WriteLine("update physics");
        currentState?.UpdatePhysics();
    }

    public void ChangeState(BaseState newState)
    {
        currentState?.Exit();
        
        currentState = newState;
        currentState?.Enter();
    }

    public Type GetStateType()
    {
        return currentState?.GetType();
    }
}