using System;
using System.Collections.Generic;
using RpgGame.UI;

namespace RpgGame.Core
{
    using Attribute = Terminal.Gui.Attribute;
    public enum MapStyle
    {
        Cave,
        Hills,
    }

    public class MapGenerator
    {
        private static readonly Dictionary<char, Attribute> _terrainColors = new()
        {
            { '#', Colors.Terrain.Stone },    // Walls
            { '.', Colors.Terrain.Grass },    // Floors
            { '~', Colors.Terrain.Water },    // Water
            { '^', Colors.Terrain.Lava },     // Lava/Hills
            { '*', Colors.Terrain.Forest },   // Vegetation
            { 'H', Colors.Terrain.Stone },    // Houses
            { 'S', Colors.Terrain.Sand }      // Shops
        };

        public static Attribute GetTerrainColor(char terrainChar)
        {
            return _terrainColors.TryGetValue(terrainChar, out var color) 
                ? color 
                : Colors.Terrain.Grass;
        }
    
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

        public Cell[,] GenerateDungeon()
        {
            EventSystem.RaiseEvent($"Generating {_style.ToString().ToLower()} map...");
            
            var map = new Cell[_mapHeight, _mapWidth];
            
            // Initialize map based on style
            switch (_style)
            {
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

        private Cell[,] GenerateCave()
        {
            var map = new Cell[_mapHeight, _mapWidth];
            
            // Initialize random map
            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    map[y, x] = _random.Next(100) < 45 ? new Cell {Character = '#', Attribute = Colors.Terrain.Stone} : 
                        new Cell {Character = '.', Attribute = Colors.Terrain.Grass};
                }
            }

            // Apply cellular automata rules
            for (int i = 0; i < 5; i++)
            {
                map = SmoothMap(map);
            }

            return map;
        }

        private Cell[,] GenerateHills()
        {
            var map = new Cell[_mapHeight, _mapWidth];
            
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
                        map[y, x] = new Cell {Character = '^', Attribute = Colors.Terrain.Lava};
                    else if (heightMap[y, x] > 0.5f)
                        map[y, x] = new Cell {Character = '*', Attribute = Colors.Terrain.Forest};
                    else
                        map[y, x] = new Cell {Character = '.', Attribute = Colors.Terrain.Grass};
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

        private Cell[,] SmoothMap(Cell[,] map, char wallChar = '#')
        {
            Cell[,] newMap = new Cell[_mapHeight, _mapWidth];
            
            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    int neighborCount = GetSurroundingCount(map, x, y, wallChar);

                    if (neighborCount > 4)
                        newMap[y, x] = new Cell {Character = wallChar, Attribute = GetTerrainColor(wallChar)};
                    else if (neighborCount < 4)
                        newMap[y, x] = new Cell {Character = '.', Attribute = Colors.Terrain.Grass};
                    else
                        newMap[y, x] = map[y, x];
                }
            }
            
            return newMap;
        }

        private int GetSurroundingCount(Cell[,] map, int x, int y, char target)
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
                            count += map[neighborY, neighborX].Character == target ? 1 : 0;
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

        private Cell[,] AddTerrainFeatures(Cell[,] map)
        {
            switch (_style)
            {                                        
                case MapStyle.Hills:
                    map = AddRandomFeature(map, new Cell{Character = '~', Attribute = Colors.Terrain.Water}, 5, 3);  // Streams
                    map = AddRandomFeature(map, new Cell{Character = '*', Attribute = Colors.Terrain.Forest}, 10, 5); // Scrub
                    break;
                    
                default: // Cave
                    map = AddRandomFeature(map, new Cell{Character = '~', Attribute = Colors.Terrain.Water}, 10, 5); // Water
                    map = AddRandomFeature(map, new Cell{Character = '^', Attribute = Colors.Terrain.Lava}, 5, 3);  // Lava
                    map = AddRandomFeature(map, new Cell{Character = '*', Attribute = Colors.Terrain.Forest}, 15, 7); // Vegetation
                    break;
            }
            return map;
        }

        private Cell[,] AddRandomFeature(Cell[,] map, Cell feature, int count, int maxSize)
        {
            for (int i = 0; i < count; i++)
            {
                int startX = _random.Next(1, _mapWidth - 1);
                int startY = _random.Next(1, _mapHeight - 1);
                
                if (map[startY, startX].Character == '.')
                {
                    map = FloodFillFeature(map, startX, startY, feature, maxSize);
                }
            }
            return map;
        }

        private Cell[,] FloodFillFeature(Cell[,] map, int x, int y, Cell feature, int maxSize)
        {
            Queue<(int x, int y)> queue = new();
            queue.Enqueue((x, y));
            int filled = 0;

            while (queue.Count > 0 && filled < maxSize)
            {
                var (currentX, currentY) = queue.Dequeue();
                
                if (map[currentY, currentX].Character == '.')
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
