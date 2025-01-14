using HHSGame.UI;

namespace HHSGame.Core.Map
{
    using System.Globalization;
    using Items;
    using Attribute = Terminal.Gui.Attribute;
    public enum MapStyle
    {
        Cave,
        Hills,
        Town,
    }

    public class MapGenerator(GameParameters parameters, Random random, ItemManager itemManager, ItemFactory itemFactory)
    {
        private static readonly Dictionary<char, Attribute> _terrainColors = new()
        {
            { '#', ColorPresets.Terrain.Stone },    // Walls
            { '.', ColorPresets.Terrain.Grass },    // Floors
            { '~', ColorPresets.Terrain.Water },    // Water
            { '^', ColorPresets.Terrain.Lava },     // Lava/Hills
            { '*', ColorPresets.Terrain.Forest },   // Vegetation
            { 'H', ColorPresets.Terrain.Stone },    // Houses
            { 'S', ColorPresets.Terrain.Sand },     // Shops
            { '║', ColorPresets.Terrain.Stone },    // Town walls
            { '═', ColorPresets.Terrain.Stone },    // Town walls
            { '╔', ColorPresets.Terrain.Stone },    // Town walls
            { '╗', ColorPresets.Terrain.Stone },    // Town walls
            { '╚', ColorPresets.Terrain.Stone },    // Town walls
            { '╝', ColorPresets.Terrain.Stone },    // Town walls
            { '│', ColorPresets.Terrain.Stone },    // House/Room walls
            { '─', ColorPresets.Terrain.Stone },    // House/Room walls
            { '┌', ColorPresets.Terrain.Stone },    // House/Room walls
            { '┐', ColorPresets.Terrain.Stone },    // House/Room walls
            { '└', ColorPresets.Terrain.Stone },    // House/Room walls
            { '┘', ColorPresets.Terrain.Stone },    // House/Room walls
            { '▒', ColorPresets.Terrain.Grass },    // Town streets
        };

        public static Attribute GetTerrainColor(char terrainChar)
        {
            return _terrainColors.TryGetValue(terrainChar, out var color)
                ? color
                : ColorPresets.Terrain.Grass;
        }

        private readonly int mapWidth = parameters.MapWidth;
        private readonly int mapHeight = parameters.MapHeight;
        private readonly MapStyle style = parameters.MapStyle;

        public Cell[,] GenerateDungeon()
        {
            EventSystem.RaiseGameMessage($"Generating {style.ToString().ToLower(CultureInfo.InvariantCulture)} map...");

            BaseMapGenerator generator = style switch
            {
                MapStyle.Hills => new HillsMapGenerator(mapWidth, mapHeight, random),
                MapStyle.Town => new TownMapGenerator(mapWidth, mapHeight, random),
                _ => new CaveMapGenerator(mapWidth, mapHeight, random)
            };

            var map = generator.Generate();

            // Place random items in the dungeon
            PlaceRandomItems(map);

            EventSystem.RaiseGameMessage($"{style} map generated successfully");
            return map;
        }

        private void PlaceRandomItems(Cell[,] map)
        {
            int itemCount = random.Next(700, 1000); // Place some items
            for (int i = 0; i < itemCount; i++)
            {
                var item = itemFactory.CreateRandomItem();
                int x = random.Next(1, mapWidth - 1);
                int y = random.Next(1, mapHeight - 1);
                while (!map[y, x].IsWalkable)
                {
                    x = random.Next(1, mapWidth - 1);
                    y = random.Next(1, mapHeight - 1);
                }
                item.X = x;
                item.Y = y;
                itemManager.AddItem(item);
            }
        }
    }
}
