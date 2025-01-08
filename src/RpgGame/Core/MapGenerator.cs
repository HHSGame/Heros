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
        Town,
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
            { 'S', Colors.Terrain.Sand },     // Shops
            { '║', Colors.Terrain.Stone },    // Town walls
            { '═', Colors.Terrain.Stone },    // Town walls
            { '╔', Colors.Terrain.Stone },    // Town walls
            { '╗', Colors.Terrain.Stone },    // Town walls
            { '╚', Colors.Terrain.Stone },    // Town walls
            { '╝', Colors.Terrain.Stone },    // Town walls
            { '▒', Colors.Terrain.Grass },    // Town streets
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
                case MapStyle.Town:
                    map = GenerateTown();
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

        private Cell[,] GenerateTown()
        {
            var map = new Cell[_mapHeight, _mapWidth];
            
            // Initialize with grass
            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    map[y, x] = new Cell {Character = '.', Attribute = Colors.Terrain.Grass};
                }
            }

            // Create town walls with proper connections
            for (int y = 0; y < _mapHeight; y++)
            {
                // Left and right walls
                char leftChar = '║';
                char rightChar = '║';
                
                // Top and bottom corners
                if (y == 0)
                {
                    leftChar = '╔';
                    rightChar = '╗';
                }
                else if (y == _mapHeight - 1)
                {
                    leftChar = '╚';
                    rightChar = '╝';
                }
                
                map[y, 0] = new Cell {Character = leftChar, Attribute = Colors.Terrain.Stone};
                map[y, _mapWidth - 1] = new Cell {Character = rightChar, Attribute = Colors.Terrain.Stone};
            }
            
            for (int x = 0; x < _mapWidth; x++)
            {
                // Top and bottom walls
                char topChar = '═';
                char bottomChar = '═';
                
                // Left and right corners
                if (x == 0)
                {
                    topChar = '╔';
                    bottomChar = '╚';
                }
                else if (x == _mapWidth - 1)
                {
                    topChar = '╗';
                    bottomChar = '╝';
                }
                
                map[0, x] = new Cell {Character = topChar, Attribute = Colors.Terrain.Stone};
                map[_mapHeight - 1, x] = new Cell {Character = bottomChar, Attribute = Colors.Terrain.Stone};
            }

            // Create street grid
            int streetSpacing = 8;
            for (int y = streetSpacing; y < _mapHeight - streetSpacing; y += streetSpacing)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    map[y, x] = new Cell {Character = '▒', Attribute = Colors.Terrain.Grass};
                }
            }
            for (int x = streetSpacing; x < _mapWidth - streetSpacing; x += streetSpacing)
            {
                for (int y = 0; y < _mapHeight; y++)
                {
                    map[y, x] = new Cell {Character = '▒', Attribute = Colors.Terrain.Grass};
                }
            }

            // Generate houses
            int houseCount = (_mapWidth / streetSpacing) * (_mapHeight / streetSpacing) / 2;
            for (int i = 0; i < houseCount; i++)
            {
                int blockX = _random.Next(1, (_mapWidth - 2) / streetSpacing) * streetSpacing;
                int blockY = _random.Next(1, (_mapHeight - 2) / streetSpacing) * streetSpacing;
                
                // House size
                int houseWidth = _random.Next(6, 9);
                int houseHeight = Math.Max(_random.Next(houseWidth - 2, houseWidth + 2), 5);
                
                // Place house if space is available
                if (blockX + houseWidth < _mapWidth - 1 && 
                    blockY + houseHeight < _mapHeight - 1)
                {
                    // House walls with proper connections
                    for (int y = blockY; y < blockY + houseHeight; y++)
                    {
                        // Vertical walls
                        char leftChar = '│';
                        char rightChar = '│';
                        
                        // Top and bottom corners
                        if (y == blockY)
                        {
                            leftChar = '┌';
                            rightChar = '┐';
                        }
                        else if (y == blockY + houseHeight - 1)
                        {
                            leftChar = '└';
                            rightChar = '┘';
                        }
                        
                        map[y, blockX] = new Cell {Character = leftChar, Attribute = Colors.Terrain.Stone};
                        map[y, blockX + houseWidth - 1] = new Cell {Character = rightChar, Attribute = Colors.Terrain.Stone};
                    }
                    
                    for (int x = blockX; x < blockX + houseWidth; x++)
                    {
                        // Horizontal walls
                        char topChar = '─';
                        char bottomChar = '─';
                        
                        // Left and right corners
                        if (x == blockX)
                        {
                            topChar = '┌';
                            bottomChar = '└';
                        }
                        else if (x == blockX + houseWidth - 1)
                        {
                            topChar = '┐';
                            bottomChar = '┘';
                        }
                        
                        map[blockY, x] = new Cell {Character = topChar, Attribute = Colors.Terrain.Stone};
                        map[blockY + houseHeight - 1, x] = new Cell {Character = bottomChar, Attribute = Colors.Terrain.Stone};
                    }
                    
                    // House interior
                    for (int y = blockY + 1; y < blockY + houseHeight - 1; y++)
                    {
                        for (int x = blockX + 1; x < blockX + houseWidth - 1; x++)
                        {
                            map[y, x] = new Cell {Character = '.', Attribute = Colors.Terrain.Grass};
                        }
                    }
                    
                    // Door
                    int doorSide = _random.Next(4);
                    int doorX = blockX + _random.Next(1, houseWidth - 1);
                    int doorY = blockY + _random.Next(1, houseHeight - 1);
                    
                    switch (doorSide)
                    {
                        case 0: // Top
                            map[blockY, doorX] = new Cell {Character = '.', Attribute = Colors.Terrain.Grass};
                            break;
                        case 1: // Bottom
                            map[blockY + houseHeight - 1, doorX] = new Cell {Character = '.', Attribute = Colors.Terrain.Grass};
                            break;
                        case 2: // Left
                            map[doorY, blockX] = new Cell {Character = '.', Attribute = Colors.Terrain.Grass};
                            break;
                        case 3: // Right
                            map[doorY, blockX + houseWidth - 1] = new Cell {Character = '.', Attribute = Colors.Terrain.Grass};
                            break;
                    }
                }
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
                    
                case MapStyle.Town:
                    map = AddRandomFeature(map, new Cell{Character = '~', Attribute = Colors.Terrain.Water}, 2, 1);  // Wells
                    map = AddRandomFeature(map, new Cell{Character = '*', Attribute = Colors.Terrain.Forest}, 5, 2); // Trees
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
