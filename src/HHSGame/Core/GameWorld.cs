using HHSGame.UI;
using HHSGame.Core.Classes;

namespace HHSGame.Core
{
    public class GameWorld(GameContext context) : IDrawable
    {
        public static int MapWidth { get; } = 500;
        public static int MapHeight { get; } = 500;

        public GameContext Context => context;

        public Player NewPlayer(AbstractClass playerClass)
        {
            // create a player at the center of the map but avoid any obstacles
            int x = MapWidth / 2, y = MapHeight / 2;
            int round = 0;
            while (!Context.MapState.IsWalkable(x, y))
            {
                x = Context.Random.Next(x - round, x + round);
                y = Context.Random.Next(y - round, y + round);
                round++;
            }
            Player player = new(x, y, Context);
            playerClass.ApplyClassBonuses(player);
            playerClass.ApplyStartupEquipment(player);
            player.UpdateFOV();
            return player;
        }

        public void Update(Player player)
        {
            // Update all enemies
            Context.EnemyManager.UpdateEnemies(player);
        }

        public void Draw(IDrawingContext ctx)
        {
            // Draw map tiles within viewport
            Context.MapState.Draw(ctx);
            // Draw enemies within viewport
            Context.EnemyManager.Draw(ctx);
            // Draw loot within viewport
            Context.ItemManager.Draw(ctx);
        }
    }
}
