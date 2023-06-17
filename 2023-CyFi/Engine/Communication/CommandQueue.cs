using Domain.Models.Communication;

namespace Engine.Communication;

public class CommandQueue
{
    private Queue<BotCommand> commands;
    private ICommandToActionConverter converter;

    public CommandQueue(ICommandToActionConverter converter)
    {
        this.converter = converter;
        commands = new Queue<BotCommand>();
    }

    public void Add(BotCommand command)
    {
        commands.Enqueue(command);
    }

    public BotAction Pop()
    {
        var command = commands.Dequeue();
        return converter.Convert(command);
    }

    public void Clear()
    {
        commands.Clear();
    }

    public bool IsEmpty()
    {
        return commands.Count == 0;
    }

    public IList<BotAction> ToList()
    {
        return commands.Select(command => converter.Convert(command)).ToList();
    }
}