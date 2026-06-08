using Terminal.Gui.ViewBase;

namespace HHSGame.Core.Rendering
{
    public class Viewport(int x, int y, int width, int height)
    {
        public int X { get; private set; } = x;
        public int Y { get; private set; } = y;
        public int Width { get; private set; } = width;
        public int Height { get; private set; } = height;

        public bool Contains((int X, int Y) pos)
        {
            return pos.X >= X && pos.X < X + Width && pos.Y >= Y && pos.Y < Y + Height;
        }

        public (int X, int Y) ToLocal((int X, int Y) pos)
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
        void DrawAt((int X, int Y) pos, char tile);
        void DrawAt((int X, int Y) pos, Cell cell);
        void Clear();
        void Render();
        void AttachTo(View parent);
    }

    public interface IDrawable
    {
        void Draw(IDrawingContext ctx) { }
    }
}
