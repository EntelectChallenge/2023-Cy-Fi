namespace Domain.Components.StateChanges;

public abstract class StateChange
{
    public int ObjectId;
    public abstract string Serialize();
    public abstract StateChange Deserialize(string change);
    public abstract void Apply(StateManager stateManager);
}
