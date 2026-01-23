using Terminal.Gui.ViewBase;
using HHSGame.UI;
using System.Drawing;

using Attribute = Terminal.Gui.Drawing.Attribute;

namespace HHSGame.UI.Views
{
    public class MapView : View
    {
        private Cell[,] buffer;
        private static readonly Cell DefaultCell = new() { Character = ' ', Attribute = ColorPresets.GreyedOut };

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

        public void ClearBuffer()
        {
            ResizeBackBuffer();
            for (int y = 0; y < Frame.Height; y++)
            {
                for (int x = 0; x < Frame.Width; x++)
                {
                    buffer[y, x] = DefaultCell;
                }
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
                    if (cell.Character == '\0')
                    {
                        cell = DefaultCell;
                    }
                    Move(x, y);
                    SetAttribute(cell.Attribute);
                    AddRune(cell.Character);
                }
            }
            return true;
        }

    }
}
