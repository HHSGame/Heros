
using System.Collections.Immutable;
using HHSGame.UI;
using HHSGame.Core.Items;

namespace HHSGame.Core.Enemies
{
    public class EnemyManager(ItemManager itemManager) : IDrawable
    {
        private readonly List<Enemy> enemies = [];

        public ImmutableList<Enemy> Enemies => [.. enemies];

        private void DropLoot(Enemy enemy)
        {
            List<Item> loot = enemy.GenerateLoot();
            foreach (Item item in loot)
            {
                item.X = enemy.X;
                item.Y = enemy.Y;
                itemManager.AddLoot(item);
            }
            Events.RaiseSurroundingsChange((enemy.X, enemy.Y), 1, SurroundingsChangeType.DropLoot);
        }


        public void ExecuteTurn(Player player, bool useAp)
        {
            foreach (Enemy enemy in enemies.ToArray())
            {
                if (enemy.IsDead)
                {
                    // Handle enemy death
                    enemies.Remove(enemy);
                    DropLoot(enemy);
                    continue;
                }

                enemy.ResetTurn(useAp);
                enemy.PlanTurn(player, useAp);
                enemy.ExecutePlannedActions(useAp);
                enemy.EndTurn();
            }
        }

        public void Draw(IDrawingContext ctx)
        {
            Viewport viewport = ctx.Viewport;

            foreach (Enemy enemy in enemies)
            {
                if (viewport.Contains((enemy.X, enemy.Y)))
                {
                    (enemy as IGameActor).Draw(ctx);
                }
            }

        }

        public Enemy? GetEnemyAt(int x, int y)
        {
            return enemies.FirstOrDefault(e => e.X == x && e.Y == y);
        }

        public void SetEnemies(List<Enemy> enemies)
        {
            this.enemies.Clear();
            this.enemies.AddRange(enemies);
        }

    }
}
