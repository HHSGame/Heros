using HHSGame.Core.Map;
using Terminal.Gui.ViewBase;
using HHSGame.UI.Views;

using Position = (int X, int Y);

namespace HHSGame.UI
{
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
            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            int maxX = Math.Max(0, mapWidth - Width);
            int maxY = Math.Max(0, mapHeight - Height);
            int halfWidth = Width / 2;
            int halfHeight = Height / 2;

            X = Math.Clamp(x - halfWidth, 0, maxX);
            Y = Math.Clamp(y - halfHeight, 0, maxY);
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

        public readonly bool IsWalkable => Character is '.' or '▒';
    }

    public class MapViewDrawingContext(MapView mapView, MapState mapState) : IDrawingContext
    {
        private readonly MapView mapView = mapView;

        public Viewport Viewport { get; } = new(0, 0, 0, 0);

        // Explicit interface implementation for original method
        public void DrawAt((int X, int Y) pos, char tile)
        {
            this.DrawAt(pos, new Cell { Character = tile, Attribute = ColorPresets.Terrain.Stone });
        }

        public void DrawAt((int X, int Y) pos, Cell cell)
        {
            if (Viewport.Width != mapView.Frame.Width || Viewport.Height != mapView.Frame.Height)
            {
                Viewport.Resize(mapView.Frame.Width, mapView.Frame.Height);
            }
            if (!Viewport.Contains(pos))
            {
                return;
            }

            // Check visibility
            if (mapState != null)
            {
                if (mapState.IsVisible(pos.X, pos.Y))
                {
                    // Visible - use normal colors
                    (int posX, int posY) = Viewport.ToLocal(pos);
                    mapView.SetCell(posX, posY, cell.Character, cell.Attribute);
                }
                else if (mapState.WasVisited(pos.X, pos.Y))
                {
                    // Visited but not visible - grey out
                    (int posX, int posY) = Viewport.ToLocal(pos);
                    mapView.SetCell(posX, posY, cell.Character, ColorPresets.GreyedOut);
                }
                else
                {
                    // Visited but not visible - grey out
                    (int posX, int posY) = Viewport.ToLocal(pos);
                    mapView.SetCell(posX, posY, ' ', ColorPresets.GreyedOut);
                }
                // Else - don't draw at all
            }
            else
            {
                // Fallback if no game world reference
                (int posX, int posY) = Viewport.ToLocal(pos);
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
            mapView.ClearBuffer();
        }
    }
}
