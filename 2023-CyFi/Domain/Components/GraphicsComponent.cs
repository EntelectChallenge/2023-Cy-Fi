using Domain.Objects;

namespace Domain.Components
{
    public abstract class GraphicsComponent<T> where T : GameObject<T>
    {
        public abstract void Update(T gameObject, GraphicsObject graphicsObject);
    }
}
