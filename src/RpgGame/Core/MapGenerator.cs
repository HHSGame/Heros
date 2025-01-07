using System;
using System.Collections.Generic;

namespace RpgGame.Core
{
    public class MapGenerator
    {
        private readonly Random _random;
        private readonly int _mapWidth;
        private readonly int _mapHeight;
        
        public MapGenerator(int mapWidth, int mapHeight, Random random)
        {
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
            _random = random;
        }

        public char[,] GenerateDungeon()
        {
            EventSystem.RaiseEvent("Generating dungeon using cellular automata...");
            
            var map = new char[_mapHeight, _mapWidth];
            
            // Initialize random map
            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    map[y, x] = _random.Next(100) < 45 ? '#' : '.';
                }
            }

            // Apply cellular automata rules
            for (int i = 0; i < 5; i++)
            {
                map = SmoothMap(map);
            }

            // Ensure border walls
            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    if (x == 0 || y == 0 || x == _mapWidth - 1 || y == _mapHeight - 1)
                    {
                        map[y, x] = '#';
                    }
                }
            }

            // Add special terrain
            map = AddTerrainFeatures(map);
            
            EventSystem.RaiseEvent("Dungeon generated with natural cave-like structure");
            return map;
        }

        private char[,] SmoothMap(char[,] map)
        {
            char[,] newMap = new char[_mapHeight, _mapWidth];
            
            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    int neighborWallTiles = GetSurroundingWallCount(map, x, y);

                    if (neighborWallTiles > 4)
                        newMap[y, x] = '#';
                    else if (neighborWallTiles < 4)
                        newMap[y, x] = '.';
                    else
                        newMap[y, x] = map[y, x];
                }
            }
            
            return newMap;
        }

        private int GetSurroundingWallCount(char[,] map, int x, int y)
        {
            int wallCount = 0;
            for (int neighborY = y - 1; neighborY <= y + 1; neighborY++)
            {
                for (int neighborX = x - 1; neighborX <= x + 1; neighborX++)
                {
                    if (neighborX >= 0 && neighborX < _mapWidth && 
                        neighborY >= 0 && neighborY < _mapHeight)
                    {
                        if (neighborX != x || neighborY != y)
                        {
                            wallCount += map[neighborY, neighborX] == '#' ? 1 : 0;
                        }
                    }
                    else
                    {
                        wallCount++;
                    }
                }
            }
            return wallCount;
        }

        private char[,] AddTerrainFeatures(char[,] map)
        {
            // Add water
            map = AddRandomFeature(map, '~', 10, 5);
            // Add lava
            map = AddRandomFeature(map, '^', 5, 3);
            // Add vegetation
            map = AddRandomFeature(map, '*', 15, 7);
            return map;
        }

        private char[,] AddRandomFeature(char[,] map, char feature, int count, int maxSize)
        {
            for (int i = 0; i < count; i++)
            {
                int startX = _random.Next(1, _mapWidth - 1);
                int startY = _random.Next(1, _mapHeight - 1);
                
                if (map[startY, startX] == '.')
                {
                    map = FloodFillFeature(map, startX, startY, feature, maxSize);
                }
            }
            return map;
        }

        private char[,] FloodFillFeature(char[,] map, int x, int y, char feature, int maxSize)
        {
            Queue<(int x, int y)> queue = new();
            queue.Enqueue((x, y));
            int filled = 0;

            while (queue.Count > 0 && filled < maxSize)
            {
                var (currentX, currentY) = queue.Dequeue();
                
                if (map[currentY, currentX] == '.')
                {
                    map[currentY, currentX] = feature;
                    filled++;
                    
                    // Add neighbors
                    if (currentX > 1) queue.Enqueue((currentX - 1, currentY));
                    if (currentX < _mapWidth - 2) queue.Enqueue((currentX + 1, currentY));
                    if (currentY > 1) queue.Enqueue((currentX, currentY - 1));
                    if (currentY < _mapHeight - 2) queue.Enqueue((currentX, currentY + 1));
                }
            }
            return map;
        }
    }
}
