using Terminal.Gui.ViewBase;
using System.Drawing;

using Attribute = Terminal.Gui.Drawing.Attribute;

namespace HHSGame.UI.Views
{
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

        public void SetCell(int x, int y, char character, Attribute attribute)
        {
            ResizeBackBuffer();
            if (x >= 0 && x < Frame.Width && y >= 0 && y < Frame.Height)
            {
                buffer[y, x] = new Cell
                {
                    Character = (char)character,
                    Attribute = attribute
                };
               SetNeedsDraw(new Rectangle(x, y, 1, 1));
            }
        }

        protected override bool OnDrawingContent()
        {
            ResizeBackBuffer();
            for (int y = 0; y < Frame.Height; y++)
            {
                for (int x = 0; x < Frame.Width; x++)
                {
                    Cell cell = buffer[y, x];
                    Move(x, y);
                    SetAttribute(cell.Attribute);
                    AddRune(cell.Character);
                }
            }
            return true;
        }

    }
}