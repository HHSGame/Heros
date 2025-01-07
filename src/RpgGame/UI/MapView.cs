using Terminal.Gui;

namespace RpgGame.UI
{

    using Attribute = Terminal.Gui.Attribute;
    public class MapView : View
    {
        private Cell[,] _backBuffer;

        public MapView(int width, int height)
        {
            Width = width;
            Height = height;
            _backBuffer = new Cell[height, width];
            Visible = true;
        }

        public void SetCell(int x, int y, Rune character, Attribute attribute)
        {
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