
using System.Collections.Immutable;
using RpgGame.UI;

namespace RpgGame.Core {
    public class EnemyManager(ItemManager _itemManager) : IDrawable {
        private readonly List<Enemy> _enemies = [];

        public ImmutableList<Enemy> Enemies => [.. _enemies];

        private void DropLoot(Enemy enemy)
        {
            var loot = enemy.GenerateLoot();
            foreach (var item in loot)
            {
                item.X = enemy.X;
                item.Y = enemy.Y;
                _itemManager.AddLoot(item);
            }
            EventSystem.RaiseSurroundingsChange((enemy.X, enemy.Y), 1, SurroundingsChangeType.DropLoot);
        }


        public void UpdateEnemies(Player player, bool isEnemyTurn)
        {
            foreach (var enemy in _enemies.ToArray())
            {
                if (enemy.Health <= 0)
                {
                    // Handle enemy death
                    _enemies.Remove(enemy);
                    DropLoot(enemy);
                    continue;
                }

                enemy.Update(player, isEnemyTurn);

            }
        }

        public void Draw(IDrawingContext ctx) {
            var viewport = ctx.Viewport;

            foreach (var enemy in _enemies)
            {
                if (viewport.Contains((enemy.X, enemy.Y)))
                {
                    enemy.Draw(ctx);
                }
            }

        }

        public Enemy? GetEnemyAt(int x, int y)
        {
            return _enemies.FirstOrDefault(e => e.X == x && e.Y == y);
        }

        public void SetEnemies(List<Enemy> enemies)
        {
            _enemies.Clear();
            _enemies.AddRange(enemies);
        }

    }
}