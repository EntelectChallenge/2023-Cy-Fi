namespace Domain.Components.StateChanges;

public class EmptyStateChange : StateChange
{
    public override string Serialize()
    {
        throw new NotImplementedException();
    }

    public override StateChange Deserialize(string change)
    {
        throw new NotImplementedException();
    }

    public override void Apply(StateManager stateManager)
    {
        throw new NotImplementedException();
    }
}