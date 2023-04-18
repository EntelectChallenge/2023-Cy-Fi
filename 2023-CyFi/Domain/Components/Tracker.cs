using Domain.Components.StateChanges;

namespace Domain.Components;

public static class Tracker
{
    public static List<StateChange> StateChanges = new ();

    public static void TrackChange(StateChange stateChange)
    {
        StateChanges.Add(stateChange);
    }

    public static string SerializeStateChanges()
    {
        return StateChanges.Aggregate("START", (current, stateChange) => current + ("\n" + stateChange.Serialize()));
    }
}
