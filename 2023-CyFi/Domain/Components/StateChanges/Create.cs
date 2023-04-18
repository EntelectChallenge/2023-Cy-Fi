namespace Domain.Components.StateChanges;

public class Create : StateChange
{
    public string ObjectTypeName;
    private string changeIdentifier = "Created";
    public override string Serialize()
    {
        return $"{changeIdentifier} {ObjectTypeName} - ID: {ObjectId}";
    }

    public override Create Deserialize(string change)
    {
        change = change.Remove(0, changeIdentifier.Length);
        var sections = change.Split(" - ID: ");
        
        ObjectTypeName = sections[0].Trim();
        ObjectId = int.Parse(sections.Last().Trim());
        
        return this;
    }

    public override void Apply(StateManager stateManager)
    {
        if (stateManager.StateDict.ContainsKey(ObjectId))
            return;

        var assemblyName = ObjectTypeName.Split(".").First();

        var newObject = Activator.CreateInstance(assemblyName, ObjectTypeName);
        if (newObject is null) return;
        
        var newState = newObject.Unwrap() switch
        {
            null => throw new InvalidDataException($"Cannot create object of class {ObjectTypeName}"),
            State state => state ,
            _ => stateManager.StateDict[ObjectId]
        };

        newState.Id = ObjectId;

        stateManager.StateDict[ObjectId] = newState;
    }
}