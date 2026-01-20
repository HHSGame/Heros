using HHSGame.UI;
using HHSGame.Core.Classes;
using HHSGame.Core.Map;

namespace HHSGame.Core
{
    public class GameWorld(GameContext context) : IDrawable
    {
        public GameContext Context => context;

        public Player NewPlayer(AbstractClass playerClass)
        {
            MapData mapData = LoadMapData();
            context.MapState.Init(mapData);
            // create a player at the center of the map but avoid any obstacles
            Coordinate? startPosition = context.Parameters.PlayerStartPosition ?? GetSpecialPosition(mapData, '@');
            int x = startPosition?.X ?? context.MapState.Width / 2;
            int y = startPosition?.Y ?? context.MapState.Height / 2;

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

        private MapData LoadMapData()
        {
            if (context.Parameters.UseCustomMap && !string.IsNullOrWhiteSpace(context.Parameters.CustomMapPath))
            {
                return MapLoader.LoadFromFile(context.Parameters.CustomMapPath);
            }

            return MapLoader.CreateEmptyMap(context.Parameters.MapWidth, context.Parameters.MapHeight);
        }

        private static Coordinate? GetSpecialPosition(MapData mapData, char symbol)
        {
            foreach ((Coordinate position, char marker) in mapData.SpecialPositions)
            {
                if (marker == symbol)
                {
                    return position;
                }
            }

            return null;
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
