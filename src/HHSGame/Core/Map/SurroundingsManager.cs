
namespace HHSGame.Core.Map
{
    using Enemies;
    using Items;

    public class SurroundingsManager(ItemManager itemManager, EnemyManager enemyManager, MapState mapState)
    {


        public List<(string Name, (int x, int y) Position)> GetVisibleItemsAndEnemies()
        {
            var visibleEntities = new List<(string Name, (int x, int y) Position)>();

            foreach (var (tileX, tileY) in mapState.CurrentVisibleTiles)
            {
                var enemies = enemyManager.Enemies.Where(e => e.X == tileX && e.Y == tileY).ToList();
                var items = itemManager.Loot.Where(item => item.X == tileX && item.Y == tileY).ToList();

                foreach (var enemy in enemies)
                {
                    visibleEntities.Add((enemy.Name, (enemy.X, enemy.Y)));
                }

                foreach (var item in items)
                {
                    visibleEntities.Add((item.Name, (item.X, item.Y)));
                }
            }

            return visibleEntities;
        }

    }

}