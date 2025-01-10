using System;
using RpgGame.UI;

namespace RpgGame.Core
{
    public class TownMapGenerator : BaseMapGenerator
    {
        private const int MinBuildingSize = 3;
        private const int MaxBuildingSize = 6;
        private const int StreetWidth = 2;
        private const int MinBuildings = 5;
        private const int MaxBuildings = 10;
        
        public TownMapGenerator(int mapWidth, int mapHeight, Random random) 
            : base(mapWidth, mapHeight, random)
        {
        }

        public override Cell[,] Generate()
        {
            Cell[,] map = GenerateTown();

            map = AddRandomFeature(map, new Cell{Character = '~', Attribute = ColorPresets.Terrain.Water}, 2, 1);  // Wells
            map = AddRandomFeature(map, new Cell{Character = '*', Attribute = ColorPresets.Terrain.Forest}, 5, 2); // Trees


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
                    map[y, x] = new Cell {Character = '.', Attribute = ColorPresets.Terrain.Grass};
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
                
                map[y, 0] = new Cell {Character = leftChar, Attribute = ColorPresets.Terrain.Stone};
                map[y, _mapWidth - 1] = new Cell {Character = rightChar, Attribute = ColorPresets.Terrain.Stone};
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
                
                map[0, x] = new Cell {Character = topChar, Attribute = ColorPresets.Terrain.Stone};
                map[_mapHeight - 1, x] = new Cell {Character = bottomChar, Attribute = ColorPresets.Terrain.Stone};
            }

            // Create street grid
            int streetSpacing = 8;
            for (int y = streetSpacing; y < _mapHeight - streetSpacing; y += streetSpacing)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    map[y, x] = new Cell {Character = '▒', Attribute = ColorPresets.Terrain.Grass};
                }
            }
            for (int x = streetSpacing; x < _mapWidth - streetSpacing; x += streetSpacing)
            {
                for (int y = 0; y < _mapHeight; y++)
                {
                    map[y, x] = new Cell {Character = '▒', Attribute = ColorPresets.Terrain.Grass};
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
                        
                        map[y, blockX] = new Cell {Character = leftChar, Attribute = ColorPresets.Terrain.Stone};
                        map[y, blockX + houseWidth - 1] = new Cell {Character = rightChar, Attribute = ColorPresets.Terrain.Stone};
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
                        
                        map[blockY, x] = new Cell {Character = topChar, Attribute = ColorPresets.Terrain.Stone};
                        map[blockY + houseHeight - 1, x] = new Cell {Character = bottomChar, Attribute = ColorPresets.Terrain.Stone};
                    }
                    
                    // House interior
                    for (int y = blockY + 1; y < blockY + houseHeight - 1; y++)
                    {
                        for (int x = blockX + 1; x < blockX + houseWidth - 1; x++)
                        {
                            map[y, x] = new Cell {Character = '.', Attribute = ColorPresets.Terrain.Grass};
                        }
                    }
                    
                    // Door
                    int doorSide = _random.Next(4);
                    int doorX = blockX + _random.Next(1, houseWidth - 1);
                    int doorY = blockY + _random.Next(1, houseHeight - 1);
                    
                    switch (doorSide)
                    {
                        case 0: // Top
                            map[blockY, doorX] = new Cell {Character = '.', Attribute = ColorPresets.Terrain.Grass};
                            break;
                        case 1: // Bottom
                            map[blockY + houseHeight - 1, doorX] = new Cell {Character = '.', Attribute = ColorPresets.Terrain.Grass};
                            break;
                        case 2: // Left
                            map[doorY, blockX] = new Cell {Character = '.', Attribute = ColorPresets.Terrain.Grass};
                            break;
                        case 3: // Right
                            map[doorY, blockX + houseWidth - 1] = new Cell {Character = '.', Attribute = ColorPresets.Terrain.Grass};
                            break;
                    }
                }
            }

            return map;
        }
    }
}
