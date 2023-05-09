using CyFi.Physics.Utils;
using Domain.Enums;
using Microsoft.Scripting.Runtime;

namespace CyFi.Physics.Movement;

public class Idle : BaseState
{
    private MovementSM movementSm;
    public Idle(MovementSM stateMachine) : base("Idle", stateMachine)
    {
        movementSm = stateMachine;
    }

    public override void UpdateInput(InputCommand inputCommand)
    {
        base.UpdateInput(inputCommand);
        switch (inputCommand)
        {
            case InputCommand.UP:
                Movements.UpDecision(movementSm);
                return;
            case InputCommand.DOWN:
                movementSm.GameObject.deltaY = -1;
                break;
            case InputCommand.LEFT:
                movementSm.GameObject.deltaX = -1;
                break;
            case InputCommand.RIGHT:
                movementSm.GameObject.deltaX = 1;
                break;
            case InputCommand.UPRIGHT:
                Movements.UpDecision(movementSm);
                movementSm.GameObject.deltaY = 1;
                movementSm.GameObject.deltaX = 1;
                return;
            case InputCommand.UPLEFT:
                Movements.UpDecision(movementSm);
                movementSm.GameObject.deltaY = 1;
                movementSm.GameObject.deltaX = -1;
                return;
            case InputCommand.DOWNLEFT:
                movementSm.GameObject.deltaX = -1;
                movementSm.GameObject.deltaY = -1;
                break;
            case InputCommand.DOWNRIGHT:
                movementSm.GameObject.deltaX = 1;
                movementSm.GameObject.deltaY = -1;
                break;
            case InputCommand.DIGDOWN:
            case InputCommand.DIGRIGHT:
            case InputCommand.DIGLEFT:
                Movements.DigDecision(movementSm, inputCommand);
                return;
            case InputCommand.STEAL:
                Movements.StealDecision(movementSm);
                return;
            case InputCommand.RADAR:
                Movements.RadarDecision(movementSm);
                return;

        }

        movementSm.ChangeState(movementSm.Moving);
    }

    public override void UpdatePhysics()
    {
        base.UpdatePhysics();
        if (Movements.ShouldStartFalling(movementSm))
        {
            movementSm.ChangeState(movementSm.Falling);
            movementSm.LateUpdate();
        }
    }
}