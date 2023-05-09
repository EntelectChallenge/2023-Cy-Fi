using CyFi.Entity;
using CyFi.Physics.Utils;
using Domain.Components;
using Domain.Enums;
using Domain.Objects;

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

            hero.MovementSm.Stealing.UpdateHero(hero);
            hero.MovementSm.Stealing.UpdateOpposingPlayers(players);
            hero.MovementSm.ActivateRadar.UpdateHero(hero);
            hero.MovementSm.ActivateRadar.UpdateOpposingPlayers(players);

            // State update order
            hero.MovementSm.LateUpdate(); // Movement

            // Collecting
            Collect(); // todo: move to moving state? Is there a better state for this to go to?
            // Hazards
            Hazards(); // todo: move to moving state? Is there a better state for this to go to?
            Console.WriteLine();
        }

        private void Collect()
        {

            hero.BoundingBox().ToList().ForEach(pos =>
            {
                if (world.map[pos.X][pos.Y] == (int)ObjectType.Collectible)
                {
                    hero.Collected++;
                    world.map[pos.X][pos.Y] = (int)ObjectType.Air;

                    var chageLogItem = new WorldObject.ChangeLogItem
                    {
                        pointX = pos.X,
                        pointY = pos.Y,
                        tileType = (int)ObjectType.Air
                    };

                    world.ChangeLog.Add(chageLogItem);
                }
            });
        }

        private void Hazards()
        {
            /*            var onHazard = hero.BoundingBox().Any(
                            point => world.map[point.X][point.Y] == (int)ObjectType.Hazard || world.map[point.X][point.Y] == (int)ObjectType.Hazard
                        );*/

            var movementSm = hero.MovementSm;
            var onHazard = hero.BoundingBox().Any(point => world.map[point.X][point.Y] == (int)ObjectType.Hazard) ||
                           Collisions.HazardsBelow(movementSm.GameObject, movementSm.World);

            if (onHazard)
            {
                hero.XPosition = world.start.X;
                hero.YPosition = world.start.Y;
            }

        }
    }
}
