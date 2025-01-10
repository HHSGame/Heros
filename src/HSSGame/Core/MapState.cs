using System.Collections.Immutable;
using HHSGame.UI;

namespace HHSGame.Core {
    public class MapState : IDrawable {
        private Cell[,] _map;
        public int Width { get; private set; }
        public int Height { get; private set; }

        private readonly bool[,] _visitedTiles;
        private readonly HashSet<(int x, int y)> _currentVisibleTiles = [];

        private readonly Pathfinder _pathfinder;

        public ImmutableHashSet<(int x, int y)> CurrentVisibleTiles => [.. _currentVisibleTiles];
        public Pathfinder Pathfinder => _pathfinder;

        public MapState(int width, int height) {
            Width = width;
            Height = height;
            _map = new Cell[height, width];
            _visitedTiles = new bool[Height, Width];
            _pathfinder = new Pathfinder(this);
        }

        public void Init(Cell[,] map) {
            _map = map;
            _visitedTiles.Initialize();
            _currentVisibleTiles.Clear();
        }

        public Cell GetCell(int x, int y) {
            return _map[y, x];
        }

        public void SetCell(int x, int y, Cell cell) {
            _map[y, x] = cell;
        }

        public bool IsWalkable(int x, int y)
        {
            return IsInBounds(x, y) && _map[y, x].IsWalkable;
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

        public bool IsTransparent(int x, int y)
        {
            return IsInBounds(x, y) && _map[y, x].IsWalkable;
        }

        public void MarkVisibleTiles(HashSet<(int x, int y)> visibleTiles)
        {
            _currentVisibleTiles.Clear();

            foreach (var (x, y) in visibleTiles)
            {
                if (IsInBounds(x, y))
                {
                    _visitedTiles[y, x] = true;
                    _currentVisibleTiles.Add((x, y));
                }
            }
        }

        public bool IsVisible(int x, int y)
        {
            return _currentVisibleTiles.Contains((x, y));
        }

        public bool WasVisited(int x, int y)
        {
            return IsInBounds(x, y) && _visitedTiles[y, x];
        }

        public List<(int x, int y)> GetPath((int x, int y) start, (int x, int y) end)
        {
            return _pathfinder.FindPath(start, end);
        }

        public void Draw(IDrawingContext ctx)
        {
            var viewport = ctx.Viewport;

            for (int y = viewport.Y; y < viewport.Y + viewport.Height; y++)
            {
                for (int x = viewport.X; x < viewport.X + viewport.Width; x++)
                {
                    if (x >= 0 && x < Width && y >= 0 && y < Height)
                    {
                        ctx.DrawAt((x, y), _map[y, x]);
                    }
                }
            }
        }
    }
}