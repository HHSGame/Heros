using System;
using HHSGame.UI;

namespace HHSGame.Core.Map
{
    public class HillsMapGenerator(int MapWidth, int MapHeight, Random Random) : BaseMapGenerator(MapWidth, MapHeight, Random)
    {
        private const int HeightMapSmoothing = 5;
        private const float MountainThreshold = 0.7f;
        private const float HillThreshold = 0.5f;
        private const float ForestThreshold = 0.3f;

        public override Cell[,] Generate()
        {
            Cell[,] map = new Cell[MapHeight, MapWidth];
            float[,] heightMap = GenerateHeightMap();

            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
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

            map = AddRandomFeature(map, new Cell { Character = '~', Attribute = ColorPresets.Terrain.Water }, 5, 3);  // Streams
            map = AddRandomFeature(map, new Cell { Character = '*', Attribute = ColorPresets.Terrain.Forest }, 10, 5); // Scrub

            return map;
        }

        private float[,] GenerateHeightMap()
        {
            float[,] heightMap = new float[MapHeight, MapWidth];

            // Initialize with Random values
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    heightMap[y, x] = (float)Random.NextDouble();
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
