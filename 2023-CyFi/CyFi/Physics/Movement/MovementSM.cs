using CyFi.RootState;
using Domain.Components;
using Domain.Objects;

namespace CyFi.Physics.Movement;

public class MovementSM : StateMachine
{

    public Idle Idle;
    public Moving Moving;
    public Falling Falling;
    public Jumping Jumping;
    public Digging Digging;
    public Stealing Stealing;
    public ActivateRadar ActivateRadar;
    public GameObject GameObject;
    public WorldObject World;
    public IEnumerable<GameObject> CollidableObjects;
    
    public MovementSM(GameObject gameObject)
    {
        GameObject = gameObject;
        Idle = new Idle(this);
        Moving = new Moving(this);
        Falling = new Falling(this);
        Jumping = new Jumping(this);
        Digging = new Digging(this);
        Stealing = new Stealing(this);
        ActivateRadar = new ActivateRadar(this);
        CollidableObjects = new List<GameObject>();
    }    

    private List<GameObject>? GetCollidableObjects(CyFiState cyFiState)
    {
        // Other players are collidable
        // mobs? hazards? collision is effectively hitting into something and losing momentum.
        // Walls are collidable
        // platforms stop the hero from falling
        throw new NotImplementedException();
    }

    protected override BaseState? GetInitialState()
    {
        return Idle;
    }
}