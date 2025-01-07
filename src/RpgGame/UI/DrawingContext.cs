using Terminal.Gui;


namespace RpgGame.UI {

    using Position = (int X, int Y);

    public interface IDrawingContext {
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

        public View View => _mapView;

        public MapViewDrawingContext(int width, int height)
        {
            Width = width;
            Height = height;
            _mapView = new MapView(width, height);
        }

        // Explicit interface implementation for original method
        public void DrawAt((int X, int Y) pos, char tile)
        {
            _mapView.SetCell(pos.X, pos.Y, tile, Colors.Base.Normal);
        }

        public void DrawAt((int X, int Y) pos, Cell cell)
        {
            _mapView.SetCell(pos.X, pos.Y, cell.Character, cell.Attribute);
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