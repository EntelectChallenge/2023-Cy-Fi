using CyFi.Entity;
using CyFi.Physics.Utils;

namespace CyFi.Physics.Movement
{
    public class Stealing : BaseState
    {
        private MovementSM movementSm;

        private HeroEntity hero;

        private List<HeroEntity> opposingPlayers;

        public Stealing(MovementSM stateMachine) : base("Stealing", stateMachine)
        {
            movementSm = stateMachine;
        }


        public override void UpdatePhysics()
        {
            base.UpdatePhysics();

            Movements.Steal(movementSm, hero, opposingPlayers);

            movementSm.ChangeState(movementSm.Idle);
        }

        public void UpdateHero(HeroEntity hero)
        {
            this.hero = hero;
        }

        public void UpdateOpposingPlayers(List<HeroEntity> opposingPlayers)
        {
            this.opposingPlayers = opposingPlayers;
        }


        public override void Exit()
        {
            base.Exit();
        }
    }
}
