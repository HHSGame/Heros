
namespace HHSGame.Core.Map
{
    using System.Collections.ObjectModel;
    using Enemies;
    using Items;

    public class SurroundingsManager(ItemManager itemManager, EnemyManager enemyManager, MapState mapState)
    {

        public ObservableCollection<string> VisibleEntities { get; set; } = [];


        public void UpdateVisibleEntities()
        {
            VisibleEntities.Clear();

            foreach (var (tileX, tileY) in mapState.CurrentVisibleTiles)
            {
                var enemies = enemyManager.Enemies.Where(e => e.X == tileX && e.Y == tileY).ToList();
                var items = itemManager.Loot.Where(item => item.X == tileX && item.Y == tileY).ToList();

                foreach (var enemy in enemies)
                {
                    VisibleEntities.Add(enemy.Name);
                }

                foreach (var item in items)
                {
                    VisibleEntities.Add(item.Name);
                }
            }
        }
    }
}