using Logger;

namespace Engine
{
    public abstract class GameEngine
    {
        public IGameLogger<GameEngine> Logger;

        public GameEngine()
        {
        }

        public abstract void GameLoop();
    }
}
