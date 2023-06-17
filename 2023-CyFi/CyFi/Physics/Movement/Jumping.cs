using CyFi.Physics.Utils;
using Domain.Enums;

namespace CyFi.Physics.Movement;

public class Jumping : BaseState
{
    private MovementSM movementSm;

    private int jumpHeight;
    private int maxHeight = 3;

    public Jumping(MovementSM stateMachine) : base("Jumping", stateMachine)
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
        movementSm.GameObject.deltaY = 1;

        // TODO: check for stopping condition - consider reuse between jumping and falling states
        // maxHeight reached
        // collision
        // or floor
        // or ladder
        var maxHeightReached = jumpHeight >= maxHeight;
        var successfulMove = Movements.AttemptMove(movementSm);
        if (!successfulMove)
        {
            movementSm.ChangeState(movementSm.Idle);
            jumpHeight = 0;
            return;
        }
        if (maxHeightReached)
        {
            movementSm.ChangeState(movementSm.Falling);
            jumpHeight = 0;
            return;
        }

        jumpHeight++;
        Movements.UpdateHeroPositions(movementSm);
    }
}