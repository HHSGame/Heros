using HHSGame.UI;

namespace HHSGame.Core.Map
{
    public class MapLoader
    {
        private static readonly char[] SplitChars = ['\r', '\n'];
        public static MapData LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Map file not found: {filePath}");
            }

            string[] lines = File.ReadAllLines(filePath);
            return ParseMapData(lines);
        }

        public static MapData LoadFromText(string mapText)
        {
            string[] lines = mapText.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
            return ParseMapData(lines);
        }

        public static MapData CreateEmptyMap(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                throw new ArgumentException("Map dimensions must be positive.");
            }

            Cell[,] map = new Cell[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isBorder = x == 0 || y == 0 || x == width - 1 || y == height - 1;
                    char glyph = isBorder ? '#' : '.';
                    map[y, x] = new Cell { Character = glyph, Attribute = TilePresets.GetTerrainColor(glyph) };
                }
            }

            return new MapData(map, []);
        }

        private static MapData ParseMapData(string[] lines)
        {
            if (lines.Length == 0)
            {
                throw new ArgumentException("Map file is empty");
            }

            int height = lines.Length;
            int width = lines.Max(line => line.Length);

            Cell[,] map = new Cell[height, width];
            List<(Coordinate, char symbol)> specialPositions = [];

            for (int y = 0; y < height; y++)
            {
                string line = lines[y];
                for (int x = 0; x < Math.Min(line.Length, width); x++)
                {
                    char c = line[x];

                    // Handle special characters
                    switch (c)
                    {
                        case '>': // Stairs down
                            specialPositions.Add((new(x, y), '>'));
                            map[y, x] = new Cell { Character = '.', Attribute = TilePresets.GetTerrainColor('.') };
                            break;
                        case '<': // Stairs up
                            specialPositions.Add((new(x, y), '<'));
                            map[y, x] = new Cell { Character = '.', Attribute = TilePresets.GetTerrainColor('.') };
                            break;
                        case '@': // Player start position
                            specialPositions.Add((new(x, y), '@'));
                            map[y, x] = new Cell { Character = '.', Attribute = TilePresets.GetTerrainColor('.') };
                            break;
                        case ' ': // Empty space - treat as wall
                            map[y, x] = new Cell { Character = '#', Attribute = TilePresets.GetTerrainColor('#') };
                            break;
                        case '#': // Wall
                            map[y, x] = new Cell { Character = '#', Attribute = TilePresets.GetTerrainColor('#') };
                            break;
                        case '.': // Floor/walkable
                            map[y, x] = new Cell { Character = '.', Attribute = TilePresets.GetTerrainColor('.') };
                            break;
                        case '~': // Water
                            map[y, x] = new Cell { Character = '~', Attribute = TilePresets.GetTerrainColor('~') };
                            break;
                        case '^': // Mountain
                            map[y, x] = new Cell { Character = '^', Attribute = TilePresets.GetTerrainColor('^') };
                            break;
                        case '*': // Tree
                            map[y, x] = new Cell { Character = '*', Attribute = TilePresets.GetTerrainColor('*') };
                            break;
                        case '+': // Door
                            map[y, x] = new Cell { Character = '+', Attribute = TilePresets.GetTerrainColor('+') };
                            break;
                        default: // Unknown character - treat as wall
                            map[y, x] = new Cell { Character = c, Attribute = TilePresets.GetTerrainColor(c) };
                            break;
                    }
                }

                // Pad shorter lines with walls
                for (int x = line.Length; x < width; x++)
                {
                    map[y, x] = new Cell { Character = '#', Attribute = TilePresets.GetTerrainColor('#') };
                }
            }

            return new MapData(map, specialPositions);
        }

        public static List<string> GetAvailableMaps(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return [];
            }

            return Directory.GetFiles(directoryPath, "*.txt")
                           .Select(Path.GetFileNameWithoutExtension)
                           .Where(name => name != null)
                           .ToList()!;
        }
    }

    public class MapData(Cell[,] map, List<(Coordinate position, char symbol)> specialPositions)
    {
        public Cell[,] Map { get; } = map;
        public List<(Coordinate position, char symbol)> SpecialPositions { get; } = specialPositions;
        public int Width => Map.GetLength(1);
        public int Height => Map.GetLength(0);
    }
}
