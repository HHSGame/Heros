using System;
using RpgGame.UI;

namespace RpgGame.Core
{
    public class HillsMapGenerator : BaseMapGenerator
    {
        private const int HeightMapSmoothing = 5;
        private const float MountainThreshold = 0.7f;
        private const float HillThreshold = 0.5f;
        private const float ForestThreshold = 0.3f;
        
        public HillsMapGenerator(int mapWidth, int mapHeight, Random random) 
            : base(mapWidth, mapHeight, random)
        {
        }

        public override Cell[,] Generate()
        {
            Cell[,] map = new Cell[_mapHeight, _mapWidth];
            float[,] heightMap = GenerateHeightMap();
            
            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    float height = heightMap[y, x];
                    
                    if (height > MountainThreshold)
                    {
                        map[y, x] = new Cell { Character = '^', Attribute = GetTerrainColor('^') };
                    }
                    else if (height > HillThreshold)
                    {
                        map[y, x] = new Cell { Character = '*', Attribute = GetTerrainColor('*') };
                    }
                    else if (height > ForestThreshold)
                    {
                        map[y, x] = new Cell { Character = '*', Attribute = GetTerrainColor('*') };
                    }
                    else
                    {
                        map[y, x] = new Cell { Character = '.', Attribute = GetTerrainColor('.') };
                    }
                }
            }

            map = AddRandomFeature(map, new Cell{Character = '~', Attribute = ColorPresets.Terrain.Water}, 5, 3);  // Streams
            map = AddRandomFeature(map, new Cell{Character = '*', Attribute = ColorPresets.Terrain.Forest}, 10, 5); // Scrub

            return map;
        }

        private float[,] GenerateHeightMap()
        {
            float[,] heightMap = new float[_mapHeight, _mapWidth];
            
            // Initialize with random values
            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    heightMap[y, x] = (float)_random.NextDouble();
                }
            }

            // Smooth the height map
            for (int i = 0; i < HeightMapSmoothing; i++)
            {
                heightMap = SmoothHeightMap(heightMap);
            }

            return heightMap;
        }
    }
}
