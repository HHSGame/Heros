using System.Collections.Immutable;
using HHSGame.Core.Rendering;

namespace HHSGame.Core.Map
{
    public class MapState : IDrawable
    {
        private Cell[,] map;
        public int Width { get; private set; } = Constant.WIDTH;
        public int Height { get; private set; } = Constant.HEIGHT;

        private readonly List<(Coordinate Position, char Symbol)> specialPositions = [];
        public IReadOnlyList<(Coordinate Position, char Symbol)> SpecialPositions => specialPositions;
        private readonly List<HiddenFeature> hiddenFeatures = [];
        public IReadOnlyList<HiddenFeature> HiddenFeatures => hiddenFeatures;

        private readonly HashSet<Coordinate> visitedTileSet = [];
        private readonly HashSet<Coordinate> currentVisibleTiles = [];

        public ImmutableHashSet<Coordinate> CurrentVisibleTiles => [.. currentVisibleTiles];

        // Default Initialize
        public MapState()
        {
            Width = Constant.WIDTH;
            Height = Constant.HEIGHT;
            map = new Cell[Height, Width];
        }

        public void Init(MapData mapData)
        {
            Width = mapData.Width;
            Height = mapData.Height;
            map = mapData.Map;
            specialPositions.Clear();
            hiddenFeatures.Clear();
            foreach ((Coordinate position, char symbol) in mapData.SpecialPositions)
            {
                specialPositions.Add((position, symbol));
            }
            Init(mapData.Map);
        }

        private void Init(Cell[,] map)
        {
            this.map = map;
            visitedTileSet.Clear();
            currentVisibleTiles.Clear();
        }

        public Cell GetCell(int x, int y)
        {
            return map[y, x];
        }

        public Cell GetCell(Coordinate coordinate)
        {
            return map[coordinate.Y, coordinate.X];
        }

        public void SetCell(int x, int y, Cell cell)
        {
            map[y, x] = cell;
        }

        public void SetCell(Coordinate coordinate, Cell cell)
        {
            map[coordinate.Y, coordinate.X] = cell;
        }

        public bool IsWalkable(int x, int y)
        {
            return IsInBounds(x, y) && map[y, x].IsWalkable;
        }

        public bool IsWalkable(Coordinate coordinate)
        {
            return IsInBounds(coordinate) && map[coordinate.Y, coordinate.X].IsWalkable;
        }

        public bool IsInBounds(Coordinate coordinate)
        {
            return IsInBounds(coordinate.X, coordinate.Y);
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

        public bool IsTransparent(int x, int y)
        {
            // todo: fix
            return IsInBounds(x, y) && map[y, x].IsWalkable;
        }

        public bool IsTransparent(Coordinate coordinate)
        {
            return IsTransparent(coordinate.X, coordinate.Y);
        }

        public bool IsProjectileBlocking(int x, int y)
        {
            if (!IsInBounds(x, y))
            {
                return true;
            }

            Cell cell = map[y, x];
            if (cell.IsWalkable)
            {
                return false;
            }

            return cell.Character != '~';
        }

        public bool IsProjectileBlocking(Coordinate coordinate)
        {
            return IsProjectileBlocking(coordinate.X, coordinate.Y);
        }

        public void AddHiddenFeature(HiddenFeature feature)
        {
            hiddenFeatures.Add(feature);
        }

        public List<HiddenFeature> RevealHiddenFeatures(Coordinate origin, int radius)
        {
            List<HiddenFeature> revealed = [];
            foreach (HiddenFeature feature in hiddenFeatures)
            {
                if (feature.Revealed)
                {
                    continue;
                }

                int dx = Math.Abs(feature.Position.X - origin.X);
                int dy = Math.Abs(feature.Position.Y - origin.Y);
                if (dx <= radius && dy <= radius)
                {
                    feature.Revealed = true;
                    SetCell(feature.Position.X, feature.Position.Y, new Cell
                    {
                        Character = feature.RevealedGlyph,
                        Attribute = TilePresets.GetTerrainColor(feature.RevealedGlyph)
                    });
                    revealed.Add(feature);
                }
            }

            return revealed;
        }

        public void MarkVisibleTiles(HashSet<(int x, int y)> visibleTiles)
        {
            currentVisibleTiles.Clear();

            foreach ((int x, int y) in visibleTiles)
            {
                if (IsInBounds(x, y))
                {
                    visitedTileSet.Add(new(x, y));
                    currentVisibleTiles.Add(new(x, y));
                }
            }
        }


        public void MarkVisibleTiles(HashSet<Coordinate> visibleTiles)
        {
            currentVisibleTiles.Clear();

            foreach ((int x, int y) in visibleTiles)
            {
                if (IsInBounds(x, y))
                {
                    visitedTileSet.Add(new(x, y));
                    currentVisibleTiles.Add(new(x, y));
                }
            }
        }

        public bool IsVisible(Coordinate coordinate)
        {
            return currentVisibleTiles.Contains(coordinate);
        }

        public bool IsVisible(int x, int y)
        {
            return currentVisibleTiles.Contains(new(x, y));
        }

        public bool WasVisited(int x, int y)
        {
            return IsInBounds(x, y) && visitedTileSet.Contains(new(x, y));
        }

        public bool WasVisited(Coordinate coordinate)
        {
            return WasVisited(coordinate.X, coordinate.Y);
        }

        public void MarkExplored(int x, int y)
        {
            if (IsInBounds(x, y))
            {
                visitedTileSet.Add(new(x, y));
            }
        }

        public void MarkExplored(Coordinate coordinate)
        {
            MarkExplored(coordinate.X, coordinate.Y);
        }


        public void Draw(IDrawingContext ctx)
        {
            Viewport viewport = ctx.Viewport;

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
