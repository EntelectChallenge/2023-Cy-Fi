using CyFi.Entity;
using Domain.Components;
using Domain.Enums;

namespace CyFi.Inputs
{
    public class HeroInput : InputComponent<HeroEntity>
    {
        public override void Update(HeroEntity hero, InputCommand inputCommand)
        {
            hero.MovementSm.UpdateInput(inputCommand);
        }
    }
}
