using Domain.Components;
using Domain.Enums;
using CyFi.Entity;

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
