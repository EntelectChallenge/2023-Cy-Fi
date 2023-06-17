using Domain.Components.StateChanges;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Domain.Components;

public class State : INotifyPropertyChanged
{
    public static int latestId = 1;

    private static int GetId()
    {
        return latestId++;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public int Id = GetId();

    public State(String state) { }

    protected State()
    {
        //Add logger here
        Tracker.TrackChange(new Create
        {
            ObjectTypeName = GetType().ToString(),
            ObjectId = Id
        });
    }

    protected void OnPropertyChanged<T>(T oldValue, T newValue, [CallerMemberName] string propertyName = "")
    {
        Console.WriteLine($"propertyName: {propertyName} : new value {newValue}");

        Tracker.TrackChange(new Update
        {
            ObjectId = Id,
            PropertyName = propertyName,
            OldValue = oldValue,
            NewValue = newValue
        });
    }
}
