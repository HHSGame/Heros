using System;
using System.Collections.Generic;
using System.Linq;

namespace RpgGame.Core
{
    public class MapGenerator
    {
        private struct Rectangle : IEquatable<Rectangle>
        {
            public int X { get; }
            public int Y { get; }
            public int Width { get; }
            public int Height { get; }

            public Rectangle(int x, int y, int width, int height)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }

            public Point Center => new(X + Width / 2, Y + Height / 2);

            public bool Intersects(Rectangle other)
            {
                return X <= other.X + other.Width &&
                       X + Width >= other.X &&
                       Y <= other.Y + other.Height &&
                       Y + Height >= other.Y;
            }

            public bool Equals(Rectangle other)
            {
                return X == other.X && 
                       Y == other.Y &&
                       Width == other.Width &&
                       Height == other.Height;
            }

            public override bool Equals(object? obj)
            {
                return obj is Rectangle other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(X, Y, Width, Height);
            }

            public static bool operator ==(Rectangle left, Rectangle right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(Rectangle left, Rectangle right)
            {
                return !(left == right);
            }
        }

        private struct Point
        {
            public int X { get; }
            public int Y { get; }

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }
        }
        private const int MapWidth = 150;
        private const int MapHeight = 30;
        private const int MaxRooms = 15;
        private const int MinRoomSize = 8;
        private const int MaxRoomSize = 15;
        private const int CaveFillPercent = 40;
        
        private readonly Random _random;
        private char[,] _map;

        public MapGenerator(Random random)
        {
            _random = random;
            _map = new char[MapHeight, MapWidth];
        }

        public char[,] GenerateMap()
        {
            InitializeMap();
            GenerateRooms();
            GenerateCaves();
            ConnectAreas();
            AddSpecialRooms();
            AddTerrainFeatures();
            return _map;
        }

        private void InitializeMap()
        {
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    _map[y, x] = '#';
                }
            }
        }

        private void GenerateRooms()
        {
            var rooms = new List<Rectangle>();
            
            for (int i = 0; i < MaxRooms; i++)
            {
                int width = _random.Next(MinRoomSize, MaxRoomSize);
                int height = _random.Next(MinRoomSize, MaxRoomSize);
                int x = _random.Next(1, MapWidth - width - 1);
                int y = _random.Next(1, MapHeight - height - 1);

                var newRoom = new Rectangle(x, y, width, height);
                
                if (!rooms.Any(room => newRoom.Intersects(room)))
                {
                    CreateRoom(newRoom);
                    rooms.Add(newRoom);

                    if (rooms.Count > 1)
                    {
                        ConnectRooms(rooms[^2], newRoom);
                    }
                }
            }
        }

        private void CreateRoom(Rectangle room)
        {
            for (int y = room.Y + 1; y < room.Y + room.Height; y++)
            {
                for (int x = room.X + 1; x < room.X + room.Width; x++)
                {
                    _map[y, x] = '.';
                }
            }
        }

        private void ConnectRooms(Rectangle room1, Rectangle room2)
        {
            var center1 = room1.Center;
            var center2 = room2.Center;

            if (_random.Next(2) == 0)
            {
                CreateHTunnel(center1.X, center2.X, center1.Y);
                CreateVTunnel(center1.Y, center2.Y, center2.X);
            }
            else
            {
                CreateVTunnel(center1.Y, center2.Y, center1.X);
                CreateHTunnel(center1.X, center2.X, center2.Y);
            }
        }

        private void CreateHTunnel(int x1, int x2, int y)
        {
            for (int x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x++)
            {
                _map[y, x] = '.';
            }
        }

        private void CreateVTunnel(int y1, int y2, int x)
        {
            for (int y = Math.Min(y1, y2); y <= Math.Max(y1, y2); y++)
            {
                _map[y, x] = '.';
            }
        }

        private void GenerateCaves()
        {
            // Initial random fill
            for (int y = 1; y < MapHeight - 1; y++)
            {
                for (int x = 1; x < MapWidth - 1; x++)
                {
                    if (_map[y, x] == '#' && _random.Next(100) < CaveFillPercent)
                    {
                        _map[y, x] = '.';
                    }
                }
            }

            // Smooth caves using cellular automata
            for (int i = 0; i < 5; i++)
            {
                SmoothCaves();
            }
        }

        private void SmoothCaves()
        {
            var newMap = new char[MapHeight, MapWidth];
            
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    int neighborWallCount = GetSurroundingWallCount(x, y);

                    if (neighborWallCount > 4)
                        newMap[y, x] = '#';
                    else if (neighborWallCount < 4)
                        newMap[y, x] = '.';
                    else
                        newMap[y, x] = _map[y, x];
                }
            }
            
            _map = newMap;
        }

        private int GetSurroundingWallCount(int x, int y)
        {
            int wallCount = 0;
            for (int neighborY = y - 1; neighborY <= y + 1; neighborY++)
            {
                for (int neighborX = x - 1; neighborX <= x + 1; neighborX++)
                {
                    if (neighborX >= 0 && neighborX < MapWidth && 
                        neighborY >= 0 && neighborY < MapHeight)
                    {
                        if (neighborX != x || neighborY != y)
                        {
                            wallCount += _map[neighborY, neighborX] == '#' ? 1 : 0;
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

        private void ConnectAreas()
        {
            // Connect caves to rooms with doors
            for (int y = 1; y < MapHeight - 1; y++)
            {
                for (int x = 1; x < MapWidth - 1; x++)
                {
                    if (_map[y, x] == '#' && 
                        ((_map[y-1, x] == '.' && _map[y+1, x] == '.') ||
                         (_map[y, x-1] == '.' && _map[y, x+1] == '.')))
                    {
                        _map[y, x] = '+';
                    }
                }
            }
        }

        private void AddSpecialRooms()
        {
            // Find all rooms
            var rooms = FindRooms();
            
            if (rooms.Count > 1)
            {
                // Add treasure room
                var treasureRoom = rooms[_random.Next(rooms.Count)];
                CreateSpecialRoom(treasureRoom, 'T');

                // Add boss room
                var bossRoom = rooms[_random.Next(rooms.Count)];
                while (bossRoom == treasureRoom)
                {
                    bossRoom = rooms[_random.Next(rooms.Count)];
                }
                CreateSpecialRoom(bossRoom, 'B');
            }
        }

        private List<Rectangle> FindRooms()
        {
            var rooms = new List<Rectangle>();
            var visited = new bool[MapHeight, MapWidth];

            for (int y = 1; y < MapHeight - 1; y++)
            {
                for (int x = 1; x < MapWidth - 1; x++)
                {
                    if (_map[y, x] == '.' && !visited[y, x])
                    {
                        var room = FloodFillRoom(x, y, visited);
                        rooms.Add(room);
                    }
                }
            }

            return rooms;
        }

        private Rectangle FloodFillRoom(int startX, int startY, bool[,] visited)
        {
            var queue = new Queue<(int x, int y)>();
            queue.Enqueue((startX, startY));
            visited[startY, startX] = true;

            int minX = startX, maxX = startX;
            int minY = startY, maxY = startY;

            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();
                
                minX = Math.Min(minX, x);
                maxX = Math.Max(maxX, x);
                minY = Math.Min(minY, y);
                maxY = Math.Max(maxY, y);

                foreach (var (nx, ny) in GetNeighbors(x, y))
                {
                    if (_map[ny, nx] == '.' && !visited[ny, nx])
                    {
                        visited[ny, nx] = true;
                        queue.Enqueue((nx, ny));
                    }
                }
            }

            return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        private IEnumerable<(int x, int y)> GetNeighbors(int x, int y)
        {
            if (x > 0) yield return (x - 1, y);
            if (x < MapWidth - 1) yield return (x + 1, y);
            if (y > 0) yield return (x, y - 1);
            if (y < MapHeight - 1) yield return (x, y + 1);
        }

        private void CreateSpecialRoom(Rectangle room, char type)
        {
            for (int y = room.Y; y < room.Y + room.Height; y++)
            {
                for (int x = room.X; x < room.X + room.Width; x++)
                {
                    if (x == room.X || y == room.Y || 
                        x == room.X + room.Width - 1 || 
                        y == room.Y + room.Height - 1)
                    {
                        if (_map[y, x] == '.')
                        {
                            _map[y, x] = '+';
                        }
                        else
                        {
                            _map[y, x] = '#';
                        }
                    }
                    else
                    {
                        _map[y, x] = type;
                    }
                }
            }
        }

        private void AddTerrainFeatures()
        {
            // Add water
            AddRandomFeature('~', 10, 5);
            // Add lava
            AddRandomFeature('^', 5, 3);
            // Add vegetation
            AddRandomFeature('*', 15, 7);
        }

        private void AddRandomFeature(char feature, int count, int maxSize)
        {
            for (int i = 0; i < count; i++)
            {
                int startX = _random.Next(1, MapWidth - 1);
                int startY = _random.Next(1, MapHeight - 1);
                
                if (_map[startY, startX] == '.')
                {
                    FloodFillFeature(startX, startY, feature, maxSize);
                }
            }
        }

        private void FloodFillFeature(int x, int y, char feature, int maxSize)
        {
            var queue = new Queue<(int x, int y)>();
            queue.Enqueue((x, y));
            int filled = 0;

            while (queue.Count > 0 && filled < maxSize)
            {
                var (currentX, currentY) = queue.Dequeue();
                
                if (_map[currentY, currentX] == '.')
                {
                    _map[currentY, currentX] = feature;
                    filled++;
                    
                    foreach (var (nx, ny) in GetNeighbors(currentX, currentY))
                    {
                        queue.Enqueue((nx, ny));
                    }
                }
            }
        }
    }
}
