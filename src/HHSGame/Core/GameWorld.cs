using HHSGame.UI;
using HHSGame.Core.Combat;
using HHSGame.Core.Classes;
using HHSGame.Core.Map;

namespace HHSGame.Core
{
    public class GameWorld(GameContext context) : IDrawable
    {
        private bool mapInitialized;
        private MapData? mapData;

        public GameContext Context => context;

        public Player NewPlayer(AbstractClass playerClass)
        {
            EnsureMapInitialized();
            Coordinate? startPosition = context.Parameters.PlayerStartPosition ?? GetSpecialPosition(mapData, '@');
            Coordinate resolved = ResolveStartPosition(startPosition);
            Player player = new(resolved.X, resolved.Y, Context);
            playerClass.ApplyClassBonuses(player);
            playerClass.ApplyStartupEquipment(player);
            if (Context.Parameters.PlayerAttributes != null)
            {
                player.ApplyBaseStats(
                    Context.Parameters.PlayerAttributes,
                    Context.Parameters.PlayerSkills ?? new Stats.Skills());
            }
            return player;
        }

        public void InitializeMap()
        {
            EnsureMapInitialized();
        }

        public Player CreatePlayer(Classes.ClassConfig classConfig, Coordinate position, Stats.Attributes? attributes, Stats.Skills? skills, string? name, char? glyph)
        {
            EnsureMapInitialized();
            Player player = new(position.X, position.Y, Context, name, glyph);
            AbstractClass chosenClass = classConfig.ToClass();
            chosenClass.ApplyClassBonuses(player);
            chosenClass.ApplyStartupEquipment(player);
            if (attributes != null)
            {
                player.ApplyBaseStats(attributes, skills ?? new Stats.Skills());
            }
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

        private void EnsureMapInitialized()
        {
            if (mapInitialized)
            {
                return;
            }

            mapData = LoadMapData();
            context.MapState.Init(mapData);
            mapInitialized = true;
        }

        private Coordinate ResolveStartPosition(Coordinate? startPosition)
        {
            Coordinate? position = startPosition ?? GetSpecialPosition(mapData, '@');
            int x = position?.X ?? context.MapState.Width / 2;
            int y = position?.Y ?? context.MapState.Height / 2;

            int round = 1;
            while (!Context.MapState.IsWalkable(x, y))
            {
                x = Context.Random.Next(x - round, x + round);
                y = Context.Random.Next(y - round, y + round);
                round++;
            }

            return new Coordinate(x, y);
        }

        private static Coordinate? GetSpecialPosition(MapData? mapData, char symbol)
        {
            if (mapData == null)
            {
                return null;
            }

            foreach ((Coordinate position, char marker) in mapData.SpecialPositions)
            {
                if (marker == symbol)
                {
                    return position;
                }
            }

            return null;
        }

        public void Update(Player player, bool useAp, Action<QueuedAction>? onEnemyAction = null)
        {
            Context.EnemyManager.ExecuteTurn(player, useAp, onEnemyAction);
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
