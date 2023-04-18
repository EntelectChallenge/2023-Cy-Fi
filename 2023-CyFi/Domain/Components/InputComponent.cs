using Domain.Enums;
using Domain.Objects;

namespace Domain.Components
{
    public abstract class InputComponent<T> where T : GameObject<T>
    {
        public abstract void Update(T gameObject, InputCommand inputCommand);
    }
}
