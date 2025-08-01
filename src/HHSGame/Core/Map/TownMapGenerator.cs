using HHSGame.UI;

namespace HHSGame.Core.Map
{
    public class TownMapGenerator(int MapWidth, int MapHeight, Random Random) : BaseMapGenerator(MapWidth, MapHeight, Random)
    {

        public override MapData Generate()
        {
            Cell[,] map = GenerateTown();

            map = AddRandomFeature(map, new Cell { Character = '~', Attribute = ColorPresets.Terrain.Water }, 2, 1);  // Wells
            map = AddRandomFeature(map, new Cell { Character = '*', Attribute = ColorPresets.Terrain.Forest }, 5, 2); // Trees


            return new MapData(map, []);
        }

        private Cell[,] GenerateTown()
        {
            Cell[,] map = new Cell[MapHeight, MapWidth];

            // Initialize with grass
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    map[y, x] = new Cell { Character = '.', Attribute = ColorPresets.Terrain.Grass };
                }
            }

            // Create town walls with proper connections
            for (int y = 0; y < MapHeight; y++)
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
                else if (y == MapHeight - 1)
                {
                    leftChar = '╚';
                    rightChar = '╝';
                }

                map[y, 0] = new Cell { Character = leftChar, Attribute = ColorPresets.Terrain.Stone };
                map[y, MapWidth - 1] = new Cell { Character = rightChar, Attribute = ColorPresets.Terrain.Stone };
            }

            for (int x = 0; x < MapWidth; x++)
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
                else if (x == MapWidth - 1)
                {
                    topChar = '╗';
                    bottomChar = '╝';
                }

                map[0, x] = new Cell { Character = topChar, Attribute = ColorPresets.Terrain.Stone };
                map[MapHeight - 1, x] = new Cell { Character = bottomChar, Attribute = ColorPresets.Terrain.Stone };
            }

            // Create street grid
            int streetSpacing = 8;
            for (int y = streetSpacing; y < MapHeight - streetSpacing; y += streetSpacing)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    map[y, x] = new Cell { Character = '▒', Attribute = ColorPresets.Terrain.Grass };
                }
            }
            for (int x = streetSpacing; x < MapWidth - streetSpacing; x += streetSpacing)
            {
                for (int y = 0; y < MapHeight; y++)
                {
                    map[y, x] = new Cell { Character = '▒', Attribute = ColorPresets.Terrain.Grass };
                }
            }

            // Generate houses
            int houseCount = MapWidth / streetSpacing * (MapHeight / streetSpacing) / 2;
            for (int i = 0; i < houseCount; i++)
            {
                int blockX = Random.Next(1, (MapWidth - 2) / streetSpacing) * streetSpacing;
                int blockY = Random.Next(1, (MapHeight - 2) / streetSpacing) * streetSpacing;

                // House size
                int houseWidth = Random.Next(6, 9);
                int houseHeight = Math.Max(Random.Next(houseWidth - 2, houseWidth + 2), 5);

                // Place house if space is available
                if (blockX + houseWidth < MapWidth - 1 &&
                    blockY + houseHeight < MapHeight - 1)
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

                        map[y, blockX] = new Cell { Character = leftChar, Attribute = ColorPresets.Terrain.Stone };
                        map[y, blockX + houseWidth - 1] = new Cell { Character = rightChar, Attribute = ColorPresets.Terrain.Stone };
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

                        map[blockY, x] = new Cell { Character = topChar, Attribute = ColorPresets.Terrain.Stone };
                        map[blockY + houseHeight - 1, x] = new Cell { Character = bottomChar, Attribute = ColorPresets.Terrain.Stone };
                    }

                    // House interior
                    for (int y = blockY + 1; y < blockY + houseHeight - 1; y++)
                    {
                        for (int x = blockX + 1; x < blockX + houseWidth - 1; x++)
                        {
                            map[y, x] = new Cell { Character = '.', Attribute = ColorPresets.Terrain.Grass };
                        }
                    }

                    // Door
                    int doorSide = Random.Next(4);
                    int doorX = blockX + Random.Next(1, houseWidth - 1);
                    int doorY = blockY + Random.Next(1, houseHeight - 1);

                    switch (doorSide)
                    {
                        case 0: // Top
                            map[blockY, doorX] = new Cell { Character = '.', Attribute = ColorPresets.Terrain.Grass };
                            break;
                        case 1: // Bottom
                            map[blockY + houseHeight - 1, doorX] = new Cell { Character = '.', Attribute = ColorPresets.Terrain.Grass };
                            break;
                        case 2: // Left
                            map[doorY, blockX] = new Cell { Character = '.', Attribute = ColorPresets.Terrain.Grass };
                            break;
                        case 3: // Right
                            map[doorY, blockX + houseWidth - 1] = new Cell { Character = '.', Attribute = ColorPresets.Terrain.Grass };
                            break;
                    }
                }
            }

            return map;
        }
    }
}
