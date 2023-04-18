using System.Drawing;
using Domain.Components;
using Domain.Models;
using PropertyChanged.SourceGenerator;

namespace Domain.Objects
{
    public partial class GameObject : State
    {
        [Notify] private int velocity { get; set; }
        [Notify] private int xPosition { get; set; }
        [Notify] private int yPosition { get; set; }

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

        public GameObject(InputComponent<T> InputComponent, PhysicsComponent<T> PhysicsComponent)
        {
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
