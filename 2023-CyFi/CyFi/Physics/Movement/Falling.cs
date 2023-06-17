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
            case InputCommand.STEAL:
            case InputCommand.RADAR:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(inputCommand), inputCommand, null);
        }
    }

    public override void UpdatePhysics()
    {
        base.UpdatePhysics();
        // Try both movements first
        movementSm.GameObject.deltaY = -1;

        var onlyAir = Collisions.OnlyAirOrCollectableBelow(movementSm.GameObject, movementSm.World);
        var noCollisions = Collisions.NoWorldCollision(movementSm.GameObject, movementSm.World);
        var attemptMove = Movements.AttemptMove(movementSm);

        if (onlyAir && noCollisions && attemptMove)
        {
            Movements.UpdateHeroPositions(movementSm);
            return;
        }

        // If failed just fall
        movementSm.GameObject.deltaX = 0;
        onlyAir = Collisions.OnlyAirOrCollectableBelow(movementSm.GameObject, movementSm.World);
        noCollisions = Collisions.NoWorldCollision(movementSm.GameObject, movementSm.World);
        attemptMove = Movements.AttemptMove(movementSm);

        if (onlyAir && noCollisions && attemptMove)
        {
            Movements.UpdateHeroPositions(movementSm);
            return;
        }

        // Otherwise stop falling
        movementSm.GameObject.deltaY = 0;
        movementSm.ChangeState(movementSm.Idle);
    }
}