using Domain.Components.StateChanges;

namespace Domain.Components;

public static class Applier
{
    private static List<StateChange> StateChanges = new();
    
    public static List<StateChange> DeserializeStateChanges(string serializedChanges)
    {
        var lines = serializedChanges.Split("\n");
        if (lines[0] != "START") throw new InvalidDataException();

        StateChanges = lines.Select(CreateStateChange).Where(change => change.GetType() != typeof(EmptyStateChange)).ToList();
        return StateChanges;
    }

    private static StateChange CreateStateChange(string change)
    {
        if (change.Contains("Updated"))
        {
            return new Update().Deserialize(change);
        } if (change.Contains("Created"))
        {
            return new Create().Deserialize(change);
        }

        return new EmptyStateChange();
    }

    public static void ApplyChanges(StateManager stateManager, string serializedChanges)
    {
        StateChanges = DeserializeStateChanges(serializedChanges);
        foreach (var change in StateChanges)
        {
            change.Apply(stateManager);
        }
    }
}
