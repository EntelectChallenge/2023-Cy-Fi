using Domain.Objects;

namespace Domain.Components
{
    public abstract class PhysicsComponent<T> where T : GameObject<T>
    {
        public abstract void Update(T gameObject, List<T> players, WorldObject worldObject);
    }
}
