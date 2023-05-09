using CyFi.Entity;
using Domain.Components;
using Domain.Objects;
using Logger;
using Microsoft.Extensions.Logging;
using PropertyChanged.SourceGenerator;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CyFi.RootState
{
    public partial class CyFiState : State
    {
        [Notify]
        public object Seed;

        public int Tick { get; set; }

        public List<WorldObject> Levels { get; set; }

        public List<Bot> Bots { get; set; }

        private readonly IGameLogger<CyFiEngine> Logger;

        public CyFiState()
        {
            Levels = new List<WorldObject>();
        }

        public CyFiState(List<WorldObject> Levels,
            List<Bot> Bots,
            ILogger<CyFiEngine> Logger)
        {
            // this.Seed = Seed;
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

            //Save to file
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected static void OnPropertyChanged<T>(CyFiState state, T value, [CallerMemberName] string propertyName = "")
        {
            Console.WriteLine($"propertyName: {propertyName} : new value {value}");

            state.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

    }
}


