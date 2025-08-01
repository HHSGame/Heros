using HHSGame.UI;
using HHSGame.Core.Items;

namespace HHSGame.Core.Map
{
    public class CustomMapGenerator(string mapFilePath, Random random, ItemManager itemManager, ItemFactory itemFactory) : IMapGenerator
    {

        public int MapHeight { get; private set; }
        public int MapWidth { get; private set; }

        public MapData Generate()
        {
            Events.RaiseGameMessage($"Loading custom map: {Path.GetFileName(mapFilePath)}");

            try
            {
                MapData mapData = MapLoader.LoadFromFile(mapFilePath);


                MapHeight = mapData.Height;
                MapWidth = mapData.Width;

                // Place some random items in the custom map
                PlaceRandomItems(mapData.Map, mapData.Width, mapData.Height);

                Events.RaiseGameMessage($"Custom map loaded successfully: {mapData.Width}x{mapData.Height}");
                return mapData;
            }
            catch (Exception ex)
            {
                Events.RaiseGameMessage($"Failed to load custom map: {ex.Message}");

                // Fallback to a simple cave map
                return new CaveMapGenerator(80, 25, random).Generate();
            }
        }

        private void PlaceRandomItems(Cell[,] map, int width, int height)
        {
            int itemCount = random.Next(50, 100); // Fewer items for custom maps
            for (int i = 0; i < itemCount; i++)
            {
                Item item = itemFactory.CreateRandomItem();
                int x = random.Next(1, width - 1);
                int y = random.Next(1, height - 1);

                // Ensure we don't place items on walls
                while (y < height && x < width && !map[y, x].IsWalkable)
                {
                    x = random.Next(1, width - 1);
                    y = random.Next(1, height - 1);
                }

                if (y < height && x < width && map[y, x].IsWalkable)
                {
                    item.X = x;
                    item.Y = y;
                    itemManager.AddItem(item);
                }
            }
        }
    }
}