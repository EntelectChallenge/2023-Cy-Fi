using Domain.Components.StateChanges;
using Logger;

namespace Domain.Components;

public static class Tracker
{
    public static List<StateChange> StateChanges = new();

    public static void TrackChange(StateChange stateChange)
    {
        StateChanges.Add(stateChange);
    }

    public static string SerializeStateChanges()
    {
        return StateChanges.Aggregate("START", (current, stateChange) => current + ("\n" + stateChange.Serialize()));
    }
/*    public static void TrackChange(Ci stateChange)
    {


    }    */   

    // For example, we could get the changes at the end of each tick
    // and then clear the list so that the new tick start afresh.
    // Another option would be to log each change as it happens,
    // which might fit better with the purpose of the logger 
    // public static string SerializeTick()
    // {
    //     var changeList = SerializeStateChanges();
    //     StateChanges.Clear();
    //     return changeList;
    // }
}
