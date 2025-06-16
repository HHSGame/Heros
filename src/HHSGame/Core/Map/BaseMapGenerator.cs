using HHSGame.UI;
using Attribute = Terminal.Gui.Drawing.Attribute;

namespace HHSGame.Core.Map
{
    public abstract class BaseMapGenerator(int mapWidth, int mapHeight, Random random)
    {
        protected static readonly Dictionary<char, Attribute> terrainColors = new()
        {
            { '#', ColorPresets.Terrain.Stone },    // Walls
            { '.', ColorPresets.Terrain.Grass },    // Floors
            { '~', ColorPresets.Terrain.Water },    // Water
            { '^', ColorPresets.Terrain.Lava },     // Lava/Hills
            { '*', ColorPresets.Terrain.Forest },   // Vegetation
            { 'H', ColorPresets.Terrain.Stone },    // Houses
            { 'S', ColorPresets.Terrain.Sand },     // Shops
            { '║', ColorPresets.Terrain.Stone },    // Town walls
            { '═', ColorPresets.Terrain.Stone },    // Town walls
            { '╔', ColorPresets.Terrain.Stone },    // Town walls`
            { '╗', ColorPresets.Terrain.Stone },    // Town walls
            { '╚', ColorPresets.Terrain.Stone },    // Town walls
            { '╝', ColorPresets.Terrain.Stone },    // Town walls
            { '▒', ColorPresets.Terrain.Grass },    // Town streets
        };

        private readonly Random random = random;
        private readonly int mapWidth = mapWidth;
        private readonly int mapHeight = mapHeight;

        protected Random Random => random;
        protected int MapWidth => mapWidth;
        protected int MapHeight => mapHeight;

        public abstract Cell[,] Generate();

        public static Attribute GetTerrainColor(char terrainChar)
        {
            return terrainColors.TryGetValue(terrainChar, out var color)
                ? color
                : ColorPresets.Terrain.Grass;
        }

        protected Cell[,] SmoothMap(Cell[,] map, char wallChar = '#')
        {
            Cell[,] newMap = new Cell[mapHeight, mapWidth];

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    int neighborCount = GetSurroundingCount(map, x, y, wallChar);

                    if (neighborCount > 4)
                        newMap[y, x] = new Cell { Character = wallChar, Attribute = GetTerrainColor(wallChar) };
                    else if (neighborCount < 4)
                        newMap[y, x] = new Cell { Character = '.', Attribute = ColorPresets.Terrain.Grass };
                    else
                        newMap[y, x] = map[y, x];
                }
            }

            return newMap;
        }

        protected int GetSurroundingCount(Cell[,] map, int x, int y, char target)
        {
            int count = 0;
            for (int neighborY = y - 1; neighborY <= y + 1; neighborY++)
            {
                for (int neighborX = x - 1; neighborX <= x + 1; neighborX++)
                {
                    if (neighborX >= 0 && neighborX < mapWidth &&
                        neighborY >= 0 && neighborY < mapHeight)
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

        protected Cell[,] AddRandomFeature(Cell[,] map, Cell feature, int count, int maxSize)
        {
            for (int i = 0; i < count; i++)
            {
                int startX = random.Next(1, mapWidth - 1);
                int startY = random.Next(1, mapHeight - 1);

                if (map[startY, startX].Character == '.')
                {
                    map = FloodFillFeature(map, startX, startY, feature, maxSize);
                }
            }
            return map;
        }

        protected Cell[,] FloodFillFeature(Cell[,] map, int x, int y, Cell feature, int maxSize)
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
                    if (currentX < mapWidth - 2) queue.Enqueue((currentX + 1, currentY));
                    if (currentY > 1) queue.Enqueue((currentX, currentY - 1));
                    if (currentY < mapHeight - 2) queue.Enqueue((currentX, currentY + 1));
                }
            }
            return map;
        }

        protected float[,] SmoothHeightMap(float[,] heightMap)
        {
            float[,] newMap = new float[mapHeight, mapWidth];

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float sum = 0;
                    int count = 0;

                    for (int ny = y - 1; ny <= y + 1; ny++)
                    {
                        for (int nx = x - 1; nx <= x + 1; nx++)
                        {
                            if (nx >= 0 && nx < mapWidth && ny >= 0 && ny < mapHeight)
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
    }
}
