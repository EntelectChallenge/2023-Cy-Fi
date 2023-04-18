namespace Engine.Game;

public interface GameLoop
{
    public void Setup();
    public bool Run();
    public void Finish();
}