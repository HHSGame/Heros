using RpgGame.Core;
using Terminal.Gui;


namespace RpgGame.UI {

    using Position = (int X, int Y);


    public class Viewport {

        public static int DefaultWidth => 150;
        public static int DefaultHeight => 35;
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; }
        public int Height { get; }

        public Viewport(int x, int y, int width, int height) {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool Contains(Position pos) {
            return pos.X >= X && pos.X < X + Width && pos.Y >= Y && pos.Y < Y + Height;
        }

        public Position ToLocal(Position pos) {
            return (pos.X - X, pos.Y - Y);
        }

        public void UpdateViewport(int x, int y, int mapWidth, int mapHeight) {
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
    }

    public interface IDrawingContext {

        Viewport Viewport { get;}

        void DrawAt(Position pos, char tile);
        void DrawAt(Position pos, Cell cell);
        void Clear();
        void Render();
    }

    public interface IDrawable {
        void Draw(IDrawingContext ctx);
    }


    public struct Cell
    {
        public char Character;
        public Terminal.Gui.Attribute Attribute;
    }

    public class MapViewDrawingContext : IDrawingContext
    {
        public int Width { get; }
        public int Height { get; }
        private readonly MapView _mapView;
        private readonly Viewport _viewport;

        public View View => _mapView;

        public Viewport Viewport => _viewport;

        public MapViewDrawingContext(int width, int height)
        {
            Width = width;
            Height = height;
            _mapView = new MapView(width, height);
            _viewport = new Viewport(0, 0, width, height);
        }

        // Explicit interface implementation for original method
        public void DrawAt((int X, int Y) pos, char tile)
        {
            this.DrawAt(pos, new Cell {Character = tile, Attribute = Colors.Terrain.Stone});
        }

        public void DrawAt((int X, int Y) pos, Cell cell)
        {
            if (!_viewport.Contains(pos))
                return;
            var (posX, posY) = _viewport.ToLocal(pos);
            _mapView.SetCell(posX, posY, cell.Character, cell.Attribute);
        }

        public void Render()
        {
            _mapView.Redraw(new Rect(0, 0, Width, Height));
            _mapView.SetNeedsDisplay();
        }

        public void Clear()
        {
        }
    }
}
