using CyFi.Entity;
using Domain.Components;
using Domain.Enums;
using Domain.Objects;
using static CyFi.Settings.GameSettings;

namespace CyFi.Physics
{
    public class HeroPhysics : PhysicsComponent<HeroEntity>
    {
        private WorldObject world;

        private HeroEntity hero;

        private List<HeroEntity> players;

        public override void Update(HeroEntity hero, List<HeroEntity> players, WorldObject world)
        {
            this.hero = hero;
            this.players = players;
            this.world = world;

            // State update order
            hero.MovementSm.LateUpdate(); // Movement
            // Collecting
            Collect(); // todo: move to moving state? Is there a better state for this to go to?
            // Hazards
            Hazards(); // todo: move to moving state? Is there a better state for this to go to?
        }

        private void Collect()
        {
            hero.BoundingBox().ToList().ForEach(pos =>
            {
                if (world.map[pos.X][pos.Y] == (int)ObjectType.Collectible)
                {
                    hero.Collected++;
                    world.map[pos.X][pos.Y] = (int)ObjectType.Air;
                }
            });
        }

        private void Hazards()
        {
            hero.Collected -= (int)Math.Floor(hero.BoundingBox().Count(pos => world.map[pos.X][pos.Y] == (int)ObjectType.Hazard) * hazardLosePercentage * hero.Collected);
            // todo: put a conservation law in place and replace the lost collectibles throughout the map
        }
    }
}
