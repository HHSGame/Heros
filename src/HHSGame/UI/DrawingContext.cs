using HHSGame.Core.Map;
using Terminal.Gui.ViewBase;

namespace HHSGame.UI
{
    using Views;

    using Position = (int X, int Y);


    public class Viewport(int x, int y, int width, int height)
    {

        public int X { get; private set; } = x;
        public int Y { get; private set; } = y;
        public int Width { get; private set; } = width;
        public int Height { get; private set; } = height;

        public bool Contains(Position pos)
        {
            return pos.X >= X && pos.X < X + Width && pos.Y >= Y && pos.Y < Y + Height;
        }

        public Position ToLocal(Position pos)
        {
            return (pos.X - X, pos.Y - Y);
        }

        public void UpdateViewport(int x, int y, int mapWidth, int mapHeight)
        {
            int margin = 5;

            if (x - X < margin)
                X = Math.Max(0, x - margin);
            else if (x - X > Width - margin)
                X = Math.Min(mapWidth - Width, x - Width + margin);

            if (y - Y < margin)
                Y = Math.Max(0, y - margin);
            else if (y - Y > Height - margin)
                Y = Math.Min(mapHeight - Height, y - Height + margin);
        }

        public void Resize(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }

    public interface IDrawingContext
    {

        Viewport Viewport { get; }
        void DrawAt(Position pos, char tile);
        void DrawAt(Position pos, Cell cell);
        void Clear();
        void Render();
        void AttachTo(View parent);
    }

    public interface IDrawable
    {
        void Draw(IDrawingContext ctx) { }
    }


    public struct Cell
    {
        public char Character { get; set; }
        public Terminal.Gui.Drawing.Attribute Attribute { get; set; }

        public bool IsWalkable => Character == '.' || Character == '▒';
    }

    public class MapViewDrawingContext(MapView mapView, MapState mapState) : IDrawingContext
    {
        private readonly MapView mapView = mapView;
        private readonly Viewport viewport = new(0, 0, 0, 0);
        public Viewport Viewport => viewport;

        // Explicit interface implementation for original method
        public void DrawAt((int X, int Y) pos, char tile)
        {
            this.DrawAt(pos, new Cell { Character = tile, Attribute = ColorPresets.Terrain.Stone });
        }

        public void DrawAt((int X, int Y) pos, Cell cell)
        {
            if (viewport.Width != mapView.Frame.Width || viewport.Height != mapView.Frame.Height)
            {
                viewport.Resize(mapView.Frame.Width, mapView.Frame.Height);
            }
            if (!viewport.Contains(pos))
                return;

            // Check visibility
            if (mapState != null)
            {
                if (mapState.IsVisible(pos.X, pos.Y))
                {
                    // Visible - use normal colors
                    var (posX, posY) = viewport.ToLocal(pos);
                    mapView.SetCell(posX, posY, cell.Character, cell.Attribute);
                }
                else if (mapState.WasVisited(pos.X, pos.Y))
                {
                    // Visited but not visible - grey out
                    var (posX, posY) = viewport.ToLocal(pos);
                    mapView.SetCell(posX, posY, cell.Character, ColorPresets.GreyedOut);
                }
                else
                {
                    // Visited but not visible - grey out
                    var (posX, posY) = viewport.ToLocal(pos);
                    mapView.SetCell(posX, posY, ' ', ColorPresets.GreyedOut);
                }
                // Else - don't draw at all
            }
            else
            {
                // Fallback if no game world reference
                var (posX, posY) = viewport.ToLocal(pos);
                mapView.SetCell(posX, posY, cell.Character, cell.Attribute);
            }
        }

        public void Render()
        {
            mapView.SetNeedsDraw();
        }

        public void AttachTo(View parent)
        {
            parent.Add(mapView);
        }

        public void Clear()
        {
        }
    }
}

