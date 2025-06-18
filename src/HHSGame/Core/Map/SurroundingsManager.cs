using System.Collections.ObjectModel;
using HHSGame.Core.Enemies;
using HHSGame.Core.Items;

namespace HHSGame.Core.Map
{
    public class SurroundingsManager(ItemManager itemManager, EnemyManager enemyManager, MapState mapState)
    {

        public ObservableCollection<string> VisibleEntities { get; set; } = [];


        public void UpdateVisibleEntities()
        {
            VisibleEntities.Clear();

            foreach ((int tileX, int tileY) in mapState.CurrentVisibleTiles)
            {
                List<Enemy> enemies = [.. enemyManager.Enemies.Where(e => e.X == tileX && e.Y == tileY)];
                List<Item> items = [.. itemManager.Loot.Where(item => item.X == tileX && item.Y == tileY)];

                foreach (Enemy? enemy in enemies)
                {
                    VisibleEntities.Add(enemy.ToString());
                }

                foreach (Item? item in items)
                {
                    VisibleEntities.Add(item.ToString());
                }
            }
        }
    }
}