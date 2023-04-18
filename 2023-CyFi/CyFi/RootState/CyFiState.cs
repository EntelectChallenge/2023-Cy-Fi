using CyFi.Entity;
using Domain.Components;
using Domain.Objects;
using PropertyChanged.SourceGenerator;

namespace CyFi.RootState
{
    public partial class CyFiState : State
    {
        public object Seed;

        [Notify] private int tick;

        public List<WorldObject> Levels;

        public List<Bot> Bots;

        public CyFiState(List<WorldObject> Levels, List<Bot> Bots)
        {
            this.Levels = Levels;
            this.Bots = Bots;
        }

        public void Update()
        {

            //What order do we want to update the bots in?
            //Fastest bot? 
            Bots.ForEach((bot) =>
            {
                Console.WriteLine("Running Physics component");

                bot.Hero.PhysicsComponent.Update(bot.Hero, Bots.Except(new List<Bot>() { bot }).Select((bot) => bot.Hero).ToList(), Levels[bot.CurrentLevel]);
                bot.LastUpdated = DateTime.Now;
            });
        }
    }
}
