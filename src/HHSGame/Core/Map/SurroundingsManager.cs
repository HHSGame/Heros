using System.Collections.ObjectModel;
using HHSGame.Core.Enemies;
using HHSGame.Core.Items;
using HHSGame.Core.Npcs;

namespace HHSGame.Core.Map
{
    public class SurroundingsManager(ItemManager itemManager, EnemyManager enemyManager, MapState mapState, NpcManager npcManager)
    {

        public ObservableCollection<string> VisibleEntities { get; set; } = [];


        public void UpdateVisibleEntities()
        {
            VisibleEntities.Clear();

            foreach ((int tileX, int tileY) in mapState.CurrentVisibleTiles)
            {
                List<Enemy> enemies = [.. enemyManager.Enemies.Where(e => e.X == tileX && e.Y == tileY)];
                List<Item> items = [.. itemManager.Loot.Where(item => item.X == tileX && item.Y == tileY)];
                List<Npc> npcs = [.. npcManager.Npcs.Where(npc => npc.X == tileX && npc.Y == tileY)];

                foreach (Enemy? enemy in enemies)
                {
                    VisibleEntities.Add(enemy.ToString());
                }

                foreach (Item? item in items)
                {
                    VisibleEntities.Add(item.ToString());
                }

                foreach (Npc? npc in npcs)
                {
                    VisibleEntities.Add($"{npc.Name} (NPC)");
                }
            }
        }
    }
}
