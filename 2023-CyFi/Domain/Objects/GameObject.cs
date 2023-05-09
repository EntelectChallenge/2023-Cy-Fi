using Domain.Components;
using Domain.Models;
using PropertyChanged.SourceGenerator;
using System.Drawing;

namespace Domain.Objects
{
    public partial class GameObject : State
    {
        [Notify]
        private int xPosition;
        [Notify]
        private int yPosition;
        public Guid ObjectId;

        public int proposedX;
        public int proposedY;

        public int deltaX;
        public int deltaY;

        public int Width { init; get; }
        public int Height { init; get; }
        public int NextXPosition => XPosition + deltaX;
        public int NextYPosition => YPosition + deltaY;

        public Point[] BoundingBox() => new Point[]
        {
            new Point(XPosition, YPosition),
            new Point(XPosition, YPosition + Height),
            new Point(XPosition + Width, YPosition),
            new Point(XPosition + Width, YPosition + Height)
        };

        public Point[] BelowBoundingBox() => new Point[]
        {
            new Point(XPosition, YPosition -1),
            new Point(XPosition + Width, YPosition- 1),
        };

        public Point[] ProposedBoundingBox() => new Point[]
        {
            new Point(NextXPosition, NextYPosition),
            new Point(NextXPosition, NextYPosition + Height),
            new Point(NextXPosition + Width, NextYPosition),
            new Point(NextXPosition + Width, NextYPosition + Height)
        };
    }

    public class GameObject<T> : GameObject where T : GameObject<T>
    {
        public InputComponent<T> InputComponent;
        public PhysicsComponent<T> PhysicsComponent;

        public GameObject(Guid objectId)
        {
            this.ObjectId = objectId;
        }

        public GameObject(InputComponent<T> InputComponent, PhysicsComponent<T> PhysicsComponent, Guid objectId)
        {
            this.ObjectId = objectId;
            this.InputComponent = InputComponent;
            this.PhysicsComponent = PhysicsComponent;
        }

        public void UpdateInput(BotCommand Command)
        {

            InputComponent.Update((T)this, Command.Action);
        }
        public void Update(WorldObject World, List<T> players)
        {
            PhysicsComponent.Update((T)this, players, World);
        }
    }
}
