/*namespace Domain.Components.StateChanges;

// todo: add functionality to update an item array based on the index
public class UpdateArray : StateChange
{
    public string PropertyName { get; set; }
    public object OldValue { get; set; }
    public object NewValue { get; set; }

    private string serializedValue;

    private string changeIdentifier = "Updated";

    public override string Serialize()
    {
        // save the index and the value of the thing being updated
        return $"{changeIdentifier} {PropertyName} = {NewValue} - ID: {ObjectId}";
    }

*//*    public override Update Deserialize(string change)
    {
        change = change.Remove(0, changeIdentifier.Length);
        var sections = change.Split(" - ID: ");
        
        ObjectId = int.Parse(sections.Last().Trim());

        var propertyInfo = sections[0].Split(" = ");
        var propertyName = propertyInfo[0].Trim();
        var propertyValue = propertyInfo[1].Trim();
        
        PropertyName = propertyName;
        NewValue = propertyValue;
        
        return this;
    }*//*

    public override void Apply(StateManager stateManager)
    {
        var state = stateManager.StateDict[ObjectId];
        var type = state.GetType();
        var property = type.GetProperty(PropertyName);

        if (property is null) throw new InvalidDataException();
        
        var newObj = Convert.ChangeType(NewValue, property.PropertyType); 
        property.SetValue(state, newObj);
    }
}*/