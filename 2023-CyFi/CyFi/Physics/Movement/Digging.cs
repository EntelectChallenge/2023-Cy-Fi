using CyFi.Entity;
using CyFi.Physics.Utils;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyFi.Physics.Movement
{
    public class Digging : BaseState
    {
        private MovementSM movementSm;
        public Digging(MovementSM stateMachine) : base("Digging", stateMachine)
        {
            this.movementSm = stateMachine;
        }

        public override void UpdatePhysics()
        {
            base.UpdatePhysics();

            // Dig and then move.
            if (Movements.AttemptDig(this.movementSm))
            {
                if (typeof(HeroEntity).IsInstanceOfType(movementSm.GameObject))
                {
                    ((HeroEntity)movementSm.GameObject).TimesDug++;
                }
            }
            int deltaX = movementSm.GameObject.deltaX;
            int deltaY = movementSm.GameObject.deltaY;
            movementSm.ChangeState(movementSm.Moving);
            movementSm.GameObject.deltaX = deltaX;
            movementSm.GameObject.deltaY = deltaY;
            movementSm.LateUpdate();
        }

        public override void Exit()
        {
            base.Exit();
            movementSm.GameObject.deltaX = 0;
            movementSm.GameObject.deltaY = 0;
        }
    }
}
