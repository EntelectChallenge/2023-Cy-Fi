using System.ComponentModel;
using Domain.Components.StateChanges;

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

    protected State()
    {
        Tracker.TrackChange(new Create
        {
            ObjectTypeName = GetType().ToString(),
            ObjectId = Id
        });
    }

    protected void OnPropertyChanged(string propertyName, object oldValue, object newValue)
    {
        Tracker.TrackChange(new Update
        {
            ObjectId = Id,
            PropertyName = propertyName,
            OldValue = oldValue,
            NewValue = newValue
        });
    }
}
