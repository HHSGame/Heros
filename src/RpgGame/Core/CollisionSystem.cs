using System;

namespace RpgGame.Core
{
    public class CollisionSystem
    {
        private readonly GameWorld _world;

        public CollisionSystem(GameWorld world)
        {
            _world = world;
        }

        public bool CanMoveTo(int x, int y, GameActor actor)
        {
            // Check map boundaries
            if (x < 0 || y < 0 || x >= GameWorld.MapWidth || y >= GameWorld.MapHeight)
                return false;

            // Check if tile is walkable
            if (!_world.IsWalkable(x, y))
                return false;

            // Check for other enemies (except self)
            if (actor is Enemy)
            {
                var enemyAtTarget = _world.GetEnemyAt(x, y);
                if (enemyAtTarget != null && enemyAtTarget != actor)
                    return false;
            }

            return true;
        }

        public GameActor? GetCollisionAt(int x, int y)
        {
            // Check for enemies first
            var enemy = _world.GetEnemyAt(x, y);
            if (enemy != null)
                return enemy;

            // Could add other collision types here (items, etc)
            return null;
        }

        public string GetCollisionMessage(int x, int y, GameActor actor)
        {
            if (!_world.IsWalkable(x, y))
                return $"Collision: {actor.Name} blocked by terrain at ({x}, {y})";

            var collision = GetCollisionAt(x, y);
            if (collision != null && collision != actor)
                return $"Collision: {actor.Name} blocked by {collision.Name} at ({x}, {y})";

            return $"Movement: {actor.Name} moved to ({x}, {y})";
        }
    }
}
