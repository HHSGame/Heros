
using System.Collections.Immutable;
using HHSGame.Core.Combat;
using HHSGame.UI;
using HHSGame.Core.Items;

namespace HHSGame.Core.Enemies
{
    public class EnemyManager(ItemManager itemManager, TurnManager turnManager) : IDrawable
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
            EventSystem.RaiseSurroundingsChange((enemy.X, enemy.Y), 1, SurroundingsChangeType.DropLoot);
        }


        public void UpdateEnemies(Player player)
        {
            foreach (Enemy enemy in enemies.ToArray())
            {
                if (enemy.Health <= 0)
                {
                    // Handle enemy death
                    enemies.Remove(enemy);
                    DropLoot(enemy);
                    continue;
                }

                enemy.Update(player, turnManager.IsEnemyTurn());
            }
            turnManager.EndEnemyTurn();
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