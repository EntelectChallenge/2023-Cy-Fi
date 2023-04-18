using CyFi.Physics.Movement;
using CyFi.Physics.Utils;
using Domain.Enums;

namespace CyFi.Physics.Movement;

public class Falling : BaseState
{
    private MovementSM movementSm;
    public Falling(MovementSM stateMachine) : base("Falling", stateMachine)
    {
        movementSm = stateMachine;
    }

    public override void UpdateInput(InputCommand inputCommand)
    {
        base.UpdateInput(inputCommand);
        switch (inputCommand)
        {
            case InputCommand.UP:
                break;
            case InputCommand.DOWN:
                break;
            case InputCommand.LEFT:
                movementSm.GameObject.deltaX = -1;
                break;
            case InputCommand.RIGHT:
                movementSm.GameObject.deltaX = 1;
                break;
            case InputCommand.UPLEFT:
                break;
            case InputCommand.UPRIGHT:
                break;
            case InputCommand.DOWNLEFT:
                break;
            case InputCommand.DOWNRIGHT:
                break;
            case InputCommand.DIGDOWN:
            case InputCommand.DIGLEFT:
            case InputCommand.DIGRIGHT:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(inputCommand), inputCommand, null);
        }
    }

    public override void UpdatePhysics()
    {
        base.UpdatePhysics();
        movementSm.GameObject.deltaY = -1;
        if (Collisions.OnlyAirBelow(movementSm.GameObject, movementSm.World) && Collisions.NoHeroCollision(movementSm.GameObject, movementSm.CollidableObjects) && Movements.AttemptMove(movementSm))
        {
            Movements.UpdateHeroPositions(movementSm);
        }
        else
        {
            movementSm.ChangeState(movementSm.Idle);
        }
    }
}