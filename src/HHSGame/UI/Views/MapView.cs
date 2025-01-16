using Terminal.Gui;

namespace HHSGame.UI.Views
{

    using Attribute = Terminal.Gui.Attribute;
    public class MapView : View
    {
        private Cell[,] buffer;

        public MapView()
        {
            Width = Dim.Fill();
            Height = Dim.Fill();
            buffer = new Cell[Frame.Height, Frame.Width];
            Visible = true;
        }

        private void ResizeBackBuffer()
        {
            if (buffer.GetLength(0) != Frame.Height || buffer.GetLength(1) != Frame.Width)
            {
                buffer = new Cell[Frame.Height, Frame.Width];
            }
        }

        public void SetCell(int x, int y, Rune character, Attribute attribute)
        {
            ResizeBackBuffer();
            if (x >= 0 && x < Frame.Width && y >= 0 && y < Frame.Height)
            {
                buffer[y, x] = new Cell
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
                    var cell = buffer[y, x];
                    Move(x, y);
                    Driver.SetAttribute(cell.Attribute);
                    Driver.AddRune(cell.Character);
                }
            }
        }
    }
}