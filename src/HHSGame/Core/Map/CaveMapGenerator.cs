using HHSGame.UI;

namespace HHSGame.Core.Map
{
    public class CaveMapGenerator(int MapWidth, int MapHeight, Random random) : BaseMapGenerator(MapWidth, MapHeight, random)
    {
        private const int InitialFillPercent = 45;
        private const int SmoothingIterations = 5;

        public override Cell[,] Generate()
        {
            Cell[,] map = new Cell[MapHeight, MapWidth];

            // Random fill the map
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    if (x == 0 || x == MapWidth - 1 || y == 0 || y == MapHeight - 1)
                    {
                        map[y, x] = new Cell { Character = '#', Attribute = GetTerrainColor('#') };
                    }
                    else
                    {
                        map[y, x] = Random.Next(0, 100) < InitialFillPercent
                            ? new Cell { Character = '#', Attribute = GetTerrainColor('#') }
                            : new Cell { Character = '.', Attribute = GetTerrainColor('.') };
                    }
                }
            }

            // Smooth the map
            for (int i = 0; i < SmoothingIterations; i++)
            {
                map = SmoothMap(map);
            }

            // Add water features
            map = AddRandomFeature(map, new Cell { Character = '~', Attribute = ColorPresets.Terrain.Water }, 10, 5); // Water
            map = AddRandomFeature(map, new Cell { Character = '^', Attribute = ColorPresets.Terrain.Lava }, 5, 3);  // Lava
            map = AddRandomFeature(map, new Cell { Character = '*', Attribute = ColorPresets.Terrain.Forest }, 15, 7); // Vegetation

            return map;
        }
    }
}
