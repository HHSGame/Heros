using System;
using System.Collections.Generic;

namespace RpgGame.Core
{
    public enum MapStyle
    {
        Cave,
        Town,
        Hills,
    }

    public class MapGenerator
    {
        private readonly Random _random;
        private readonly int _mapWidth;
        private readonly int _mapHeight;
        private readonly MapStyle _style;
        
        public MapGenerator(int mapWidth, int mapHeight, Random random, MapStyle style = MapStyle.Cave)
        {
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
            _random = random;
            _style = style;
        }

        public char[,] GenerateDungeon()
        {
            EventSystem.RaiseEvent($"Generating {_style.ToString().ToLower()} map...");
            
            var map = new char[_mapHeight, _mapWidth];
            
            // Initialize map based on style
            switch (_style)
            {
                case MapStyle.Town:
                    map = GenerateTown();
                    break;
                case MapStyle.Hills:
                    map = GenerateHills();
                    break;
                default:
                    map = GenerateCave();
                    break;
            }

            // Add style-specific terrain features
            map = AddTerrainFeatures(map);
            
            EventSystem.RaiseEvent($"{_style} map generated successfully");
            return map;
        }

        private char[,] GenerateCave()
        {
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

            return map;
        }

        private char[,] GenerateTown()
        {
            var map = new char[_mapHeight, _mapWidth];
            
            // Create a grid-like structure
            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    if (x % 5 == 0 || y % 5 == 0)
                        map[y, x] = '#';
                    else
                        map[y, x] = '.';
                }
            }

            // Add some randomness to walls
            for (int i = 0; i < 2; i++)
            {
                map = SmoothMap(map, '#');
            }

            return map;
        }

        private char[,] GenerateHills()
        {
            var map = new char[_mapHeight, _mapWidth];
            
            // Create height map
            float[,] heightMap = new float[_mapHeight, _mapWidth];
            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    heightMap[y, x] = (float)_random.NextDouble();
                }
            }

            // Apply smoothing
            for (int i = 0; i < 3; i++)
            {
                heightMap = SmoothHeightMap(heightMap);
            }

            // Convert to terrain
            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    if (heightMap[y, x] > 0.7f)
                        map[y, x] = '^';
                    else if (heightMap[y, x] > 0.5f)
                        map[y, x] = '*';
                    else
                        map[y, x] = '.';
                }
            }

            return map;
        }

        private float[,] SmoothHeightMap(float[,] heightMap)
        {
            float[,] newMap = new float[_mapHeight, _mapWidth];
            
            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    float sum = 0;
                    int count = 0;
                    
                    for (int ny = y - 1; ny <= y + 1; ny++)
                    {
                        for (int nx = x - 1; nx <= x + 1; nx++)
                        {
                            if (nx >= 0 && nx < _mapWidth && ny >= 0 && ny < _mapHeight)
                            {
                                sum += heightMap[ny, nx];
                                count++;
                            }
                        }
                    }
                    
                    newMap[y, x] = sum / count;
                }
            }
            
            return newMap;
        }

        private char[,] SmoothMap(char[,] map, char wallChar = '#')
        {
            char[,] newMap = new char[_mapHeight, _mapWidth];
            
            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    int neighborCount = GetSurroundingCount(map, x, y, wallChar);

                    if (neighborCount > 4)
                        newMap[y, x] = wallChar;
                    else if (neighborCount < 4)
                        newMap[y, x] = '.';
                    else
                        newMap[y, x] = map[y, x];
                }
            }
            
            return newMap;
        }

        private int GetSurroundingCount(char[,] map, int x, int y, char target)
        {
            int count = 0;
            for (int neighborY = y - 1; neighborY <= y + 1; neighborY++)
            {
                for (int neighborX = x - 1; neighborX <= x + 1; neighborX++)
                {
                    if (neighborX >= 0 && neighborX < _mapWidth && 
                        neighborY >= 0 && neighborY < _mapHeight)
                    {
                        if (neighborX != x || neighborY != y)
                        {
                            count += map[neighborY, neighborX] == target ? 1 : 0;
                        }
                    }
                    else
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        private char[,] AddTerrainFeatures(char[,] map)
        {
            switch (_style)
            {                    
                case MapStyle.Town:
                    map = AddRandomFeature(map, 'H', 10, 1); // Houses
                    map = AddRandomFeature(map, 'S', 3, 1);  // Shops
                    break;
                    
                case MapStyle.Hills:
                    map = AddRandomFeature(map, '~', 5, 3);  // Streams
                    map = AddRandomFeature(map, '*', 10, 5); // Scrub
                    break;
                    
                default: // Cave
                    map = AddRandomFeature(map, '~', 10, 5); // Water
                    map = AddRandomFeature(map, '^', 5, 3);  // Lava
                    map = AddRandomFeature(map, '*', 15, 7); // Vegetation
                    break;
            }
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
