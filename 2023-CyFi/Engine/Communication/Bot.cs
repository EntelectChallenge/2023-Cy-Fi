namespace Engine.Communication;

public class Bot
{
    public Bot(Guid id, string nickName)
    {
        Id = id;
        NickName = nickName;
    }
    public Guid Id { get; set; }
    public string NickName { get; set; }
}