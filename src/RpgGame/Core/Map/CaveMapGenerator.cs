using System;
using RpgGame.UI;

namespace RpgGame.Core
{
    public class CaveMapGenerator : BaseMapGenerator
    {
        private const int InitialFillPercent = 45;
        private const int SmoothingIterations = 5;
        
        public CaveMapGenerator(int mapWidth, int mapHeight, Random random) 
            : base(mapWidth, mapHeight, random)
        {
        }

        public override Cell[,] Generate()
        {
            Cell[,] map = new Cell[_mapHeight, _mapWidth];
            
            // Random fill the map
            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    if (x == 0 || x == _mapWidth - 1 || y == 0 || y == _mapHeight - 1)
                    {
                        map[y, x] = new Cell { Character = '#', Attribute = GetTerrainColor('#') };
                    }
                    else
                    {
                        map[y, x] = _random.Next(0, 100) < InitialFillPercent
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
            map = AddRandomFeature(map, new Cell{Character = '~', Attribute = Colors.Terrain.Water}, 10, 5); // Water
            map = AddRandomFeature(map, new Cell{Character = '^', Attribute = Colors.Terrain.Lava}, 5, 3);  // Lava
            map = AddRandomFeature(map, new Cell{Character = '*', Attribute = Colors.Terrain.Forest}, 15, 7); // Vegetation

            return map;
        }
    }
}
