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

        var onlyAir = Collisions.OnlyAirBelow(movementSm.GameObject, movementSm.World);
        var noCollisions = Collisions.NoHeroCollision(movementSm.GameObject, movementSm.CollidableObjects);
        var attemptMove = Movements.AttemptMove(movementSm);

        //   if (Collisions.OnlyAirBelow(movementSm.GameObject, movementSm.World) &&
        //   Collisions.NoHeroCollision(movementSm.GameObject, movementSm.CollidableObjects) &&
        //   Movements.AttemptMove(movementSm))
        if (onlyAir && noCollisions && attemptMove)
        {
            Movements.UpdateHeroPositions(movementSm);
        }
        else
        {
            movementSm.GameObject.deltaY = 0;
            movementSm.ChangeState(movementSm.Idle);
        }
    }
}