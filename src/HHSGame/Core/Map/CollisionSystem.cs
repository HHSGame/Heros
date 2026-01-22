using HHSGame.Core.Enemies;
using HHSGame.Utils;

namespace HHSGame.Core.Map
{
    public class CollisionSystem(EnemyManager enemyManager, MapState mapState)
    {
        private readonly List<Player> players = [];

        public void SetPlayer(Player player)
        {
            SetPlayers([player]);
        }

        public void SetPlayers(IEnumerable<Player> players)
        {
            this.players.Clear();
            this.players.AddRange(players);
        }

        public bool CanMoveTo(int x, int y, IGameActor actor)
        {
            // Check map boundaries
            if (x < 0 || y < 0 || x >= mapState.Width || y >= mapState.Height)
            {
                return false;
            }

            // Check if tile is walkable
            if (!mapState.IsWalkable(x, y))
            {
                return false;
            }

            Enemy? enemyAtTarget = enemyManager.GetEnemyAt(x, y);
            if (enemyAtTarget != null && enemyAtTarget != actor)
            {
                return false;
            }

            foreach (Player player in players)
            {
                if (actor != player && player.X == x && player.Y == y)
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

            foreach (Player player in players)
            {
                if (player.X == x && player.Y == y)
                {
                    return player;
                }
            }

            // Could add other collision types here (items, etc)
            return null;
        }

        public string GetCollisionMessage(int x, int y, IGameActor actor)
        {
            if (!mapState.IsWalkable(x, y))
            {
                return I18n.T("HHS.Core.Map.CollisionSystem.BlockedByTerrain", actor.Name, x, y);
            }

            IGameActor? collision = GetCollisionAt(x, y);
            if (collision != null && collision != actor)
            {
                return I18n.T("HHS.Core.Map.CollisionSystem.BlockedByOthers", actor.Name, collision.Name, x, y);
            }

            return I18n.T("HHS.Core.Map.CollisionSystem.Movement", actor.Name, x, y);
        }
    }
}
