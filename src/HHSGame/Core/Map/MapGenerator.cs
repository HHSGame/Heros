using HHSGame.UI;
using System.Globalization;
using HHSGame.Core.Items;

namespace HHSGame.Core.Map
{
    public enum MapStyle
    {
        Cave,
        Hills,
        Town,
    }

    public class MapGenerator(GameParameters parameters, Random random, ItemManager itemManager, ItemFactory itemFactory)
    {
        private readonly int mapWidth = parameters.MapWidth;
        private readonly int mapHeight = parameters.MapHeight;
        private readonly MapStyle style = parameters.MapStyle;

        public Cell[,] GenerateDungeon()
        {
            Events.RaiseGameMessage($"Generating {style.ToString().ToLower(CultureInfo.InvariantCulture)} map...");

            BaseMapGenerator generator = style switch
            {
                MapStyle.Hills => new HillsMapGenerator(mapWidth, mapHeight, random),
                MapStyle.Town => new TownMapGenerator(mapWidth, mapHeight, random),
                _ => new CaveMapGenerator(mapWidth, mapHeight, random)
            };

            Cell[,] map = generator.Generate();

            // Place random items in the dungeon
            PlaceRandomItems(map);

            Events.RaiseGameMessage($"{style} map generated successfully");
            return map;
        }

        private void PlaceRandomItems(Cell[,] map)
        {
            int itemCount = random.Next(700, 1000); // Place some items
            for (int i = 0; i < itemCount; i++)
            {
                Item item = itemFactory.CreateRandomItem();
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
