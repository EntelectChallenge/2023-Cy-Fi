using CyFi.RootState;
using Domain.Objects;

namespace CyFi.Physics.Movement;

public class MovementSM : StateMachine
{

    public Idle Idle;
    public Moving Moving;
    public Falling Falling;
    public Jumping Jumping;
    public Digging Digging;
    public GameObject GameObject { get; }
    public WorldObject World { get; set; }
    public IEnumerable<GameObject> CollidableObjects { get; set; }
    
    public MovementSM(GameObject gameObject)
    {
        GameObject = gameObject;
        Idle = new Idle(this);
        Moving = new Moving(this);
        Falling = new Falling(this);
        Jumping = new Jumping(this);
        Digging = new Digging(this);
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