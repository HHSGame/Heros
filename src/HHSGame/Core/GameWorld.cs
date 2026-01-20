using HHSGame.UI;
using HHSGame.Core.Classes;

namespace HHSGame.Core
{
    public class GameWorld(GameContext context) : IDrawable
    {
        public GameContext Context => context;

        public Player NewPlayer(AbstractClass playerClass)
        {
            context.MapState.Init(context.MapGenerator.GenerateDungeon());
            // create a player at the center of the map but avoid any obstacles
            int x = context.MapState.Width / 2;
            int y = context.MapState.Height / 2;
            Coordinate? startPosition = context.Parameters.PlayerStartPosition;
            if (startPosition != null)
            {
                x = startPosition.X;
                y = startPosition.Y;
            }

            int round = 1;
            while (!Context.MapState.IsWalkable(x, y))
            {
                x = Context.Random.Next(x - round, x + round);
                y = Context.Random.Next(y - round, y + round);
                round++;
            }
            Player player = new(x, y, Context);
            playerClass.ApplyClassBonuses(player);
            playerClass.ApplyStartupEquipment(player);
            if (Context.Parameters.PlayerAttributes != null)
            {
                player.ApplyBaseStats(
                    Context.Parameters.PlayerAttributes,
                    Context.Parameters.PlayerSkills ?? new Stats.Skills());
            }
            player.UpdateFOV();
            return player;
        }

        public void Update(Player player, bool useAp)
        {
            Context.EnemyManager.ExecuteTurn(player, useAp);
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
