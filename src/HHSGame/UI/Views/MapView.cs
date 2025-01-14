using Terminal.Gui;

namespace HHSGame.UI.Views
{

    using Attribute = Terminal.Gui.Attribute;
    public class MapView : View
    {
        private Cell[,] _backBuffer;

        public MapView()
        {
            Width = Dim.Fill();
            Height = Dim.Fill();
            _backBuffer = new Cell[Frame.Height, Frame.Width];
            Visible = true;
        }

        private void ResizeBackBuffer()
        {
            if (_backBuffer.GetLength(0) != Frame.Height || _backBuffer.GetLength(1) != Frame.Width)
            {
                _backBuffer = new Cell[Frame.Height, Frame.Width];
            }
        }

        public void SetCell(int x, int y, Rune character, Attribute attribute)
        {
            ResizeBackBuffer();
            if (x >= 0 && x < Frame.Width && y >= 0 && y < Frame.Height)
            {
                _backBuffer[y, x] = new Cell
                {
                    Character = (char)character,
                    Attribute = attribute
                };
            }
        }

        public override void Redraw(Rect bounds)
        {
            ResizeBackBuffer();
            base.Redraw(bounds);
            for (int y = 0; y < Frame.Height; y++)
            {
                for (int x = 0; x < Frame.Width; x++)
                {
                    var cell = _backBuffer[y, x];
                    Move(x, y);
                    Driver.SetAttribute(cell.Attribute);
                    Driver.AddRune(cell.Character);
                }
            }
        }
    }
}