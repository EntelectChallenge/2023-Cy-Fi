using CyFi.Entity;
using CyFi.Physics.Utils;

namespace CyFi.Physics.Movement
{
    public class ActivateRadar : BaseState
    {
        private MovementSM movementSm;

        private HeroEntity hero;

        private List<HeroEntity> opposingPlayers;

        public ActivateRadar(MovementSM stateMachine) : base("ActivateRadar", stateMachine)
        {
            movementSm = stateMachine;
        }


        public override void UpdatePhysics()
        {
            base.UpdatePhysics();

            Movements.ActivateRadar(movementSm, hero, opposingPlayers);

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
