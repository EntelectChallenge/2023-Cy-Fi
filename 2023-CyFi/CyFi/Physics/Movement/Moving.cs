using CyFi.Physics.Utils;
using Domain.Components;
using Domain.Models;

namespace CyFi.Physics.Movement;

public class Moving : BaseState
{
    private MovementSM movementSm;

    public Moving(MovementSM stateMachine) : base("Moving", stateMachine)
    {
        movementSm = stateMachine;
    }

    public override void UpdatePhysics()
    {
        base.UpdatePhysics();
        movementSm.GameObject.proposedX = movementSm.GameObject.XPosition;
        movementSm.GameObject.proposedY = movementSm.GameObject.YPosition;

        Console.WriteLine("Attempt to move");
        Movements.AttemptMove(movementSm);
        Movements.UpdateHeroPositions(movementSm);

        // check if new position is falling and change the state
        if (Movements.ShouldStartFalling(movementSm))
        {
            movementSm.ChangeState(movementSm.Falling);
        }
        else
        {
            movementSm.ChangeState(movementSm.Idle);
        }
    }

    public override void Exit()
    {
        base.Exit();
        movementSm.GameObject.deltaX = 0;
        movementSm.GameObject.deltaY = 0;
    }
}