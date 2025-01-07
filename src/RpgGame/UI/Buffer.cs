
using Terminal.Gui;

namespace RpgGame.UI {
    public class BufferedDrawingContext : IDrawingContext
    {
        public int Width { get;}
        public int Height { get; }

        private readonly char[,] _buffer;

        private readonly View _view;

        public BufferedDrawingContext(int width, int height, View view) {
            Width = width;
            Height = height;
            _buffer = new char[height, width];
            _view = view;
        }
        public void DrawAt((int X, int Y) pos, char tile)
        {
            if (pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height)
            {
                _buffer[pos.Y, pos.X] = tile;
            }
        }

        public void Render() {
            var sb = new System.Text.StringBuilder();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    sb.Append(_buffer[y, x]);
                }
                sb.Append('\n');
            }

            _view.Text = sb.ToString();
        }

        public void Clear() {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    _buffer[y, x] = ' ';
                }
            }
        }
    }
}