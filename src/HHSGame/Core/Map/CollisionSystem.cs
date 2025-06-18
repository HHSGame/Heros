using HHSGame.Core.Enemies;
using HHSGame.Utils;

namespace HHSGame.Core.Map
{
    public class CollisionSystem(EnemyManager enemyManager, MapState mapState)
    {
        public bool CanMoveTo(int x, int y, IGameActor actor)
        {
            // Check map boundaries
            if (x < 0 || y < 0 || x >= GameWorld.MapWidth || y >= GameWorld.MapHeight)
            {
                return false;
            }

            // Check if tile is walkable
            if (!mapState.IsWalkable(x, y))
            {
                return false;
            }

            // Check for other enemies (except self)
            if (actor is Enemy)
            {
                Enemy? enemyAtTarget = enemyManager.GetEnemyAt(x, y);
                if (enemyAtTarget != null && enemyAtTarget != actor)
                {
                    return false;
                }
            }

            return true;
        }

        public IGameActor? GetCollisionAt(int x, int y)
        {
            // Check for enemies first
            Enemy? enemy = enemyManager.GetEnemyAt(x, y);
            if (enemy != null)
            {
                return enemy;
            }

            // Could add other collision types here (items, etc)
            return null;
        }

        public string GetCollisionMessage(int x, int y, IGameActor actor)
        {
            if (!mapState.IsWalkable(x, y))
            {
                return I18n.GetString("HHS.Core.Map.CollisionSystem.BlockedByTerrain", actor.Name, x, y);
            }

            IGameActor? collision = GetCollisionAt(x, y);
            if (collision != null && collision != actor)
            {
                return I18n.GetString("HHS.Core.Map.CollisionSystem.BlockedByOthers", actor.Name, collision.Name, x, y);
            }

            return I18n.GetString("HHS.Core.Map.CollisionSystem.Movement", actor.Name, x, y);
        }
    }
}
