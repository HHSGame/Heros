using System.Collections.Immutable;
using HHSGame.UI;

namespace HHSGame.Core.Map
{
    public class MapState : IDrawable
    {
        private Cell[,] map;
        public int Width { get; private set; }
        public int Height { get; private set; }

        private readonly bool[,] visitedTiles;
        private readonly HashSet<(int x, int y)> currentVisibleTiles = [];

        public ImmutableHashSet<(int x, int y)> CurrentVisibleTiles => [.. currentVisibleTiles];

        public MapState(GameParameters parameters, MapGenerator generator)
        {
            Width = parameters.MapWidth;
            Height = parameters.MapWidth;
            map = new Cell[Height, Width];
            visitedTiles = new bool[Height, Width];
            Init(generator.GenerateDungeon());
        }

        public void Init(Cell[,] map)
        {
            this.map = map;
            visitedTiles.Initialize();
            currentVisibleTiles.Clear();
        }

        public Cell GetCell(int x, int y)
        {
            return map[y, x];
        }

        public void SetCell(int x, int y, Cell cell)
        {
            map[y, x] = cell;
        }

        public bool IsWalkable(int x, int y)
        {
            return IsInBounds(x, y) && map[y, x].IsWalkable;
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

        public bool IsTransparent(int x, int y)
        {
            return IsInBounds(x, y) && map[y, x].IsWalkable;
        }

        public void MarkVisibleTiles(HashSet<(int x, int y)> visibleTiles)
        {
            currentVisibleTiles.Clear();

            foreach (var (x, y) in visibleTiles)
            {
                if (IsInBounds(x, y))
                {
                    visitedTiles[y, x] = true;
                    currentVisibleTiles.Add((x, y));
                }
            }
        }

        public bool IsVisible(int x, int y)
        {
            return currentVisibleTiles.Contains((x, y));
        }

        public bool WasVisited(int x, int y)
        {
            return IsInBounds(x, y) && visitedTiles[y, x];
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
                        ctx.DrawAt((x, y), map[y, x]);
                    }
                }
            }
        }
    }
}