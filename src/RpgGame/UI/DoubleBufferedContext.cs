using System;
using System.Text;
using Terminal.Gui;

namespace RpgGame.UI
{
    public class DoubleBufferedContext : IDrawingContext
    {
        public int Width { get; }
        public int Height { get; }
        private bool _needsFullRedraw = true;
        
        // Store both characters and their attributes
        private struct Cell
        {
            public char Character;
            public Terminal.Gui.Attribute Attribute;
        }

        private Cell[,] _frontBuffer;
        private Cell[,] _backBuffer;

        public DoubleBufferedContext(int width, int height)
        {
            Width = width;
            Height = height;
            
            // Initialize buffers
            _frontBuffer = new Cell[height, width];
            _backBuffer = new Cell[height, width];
            
            Clear();
        }

        // Explicit interface implementation for original method
        void IDrawingContext.DrawAt((int X, int Y) pos, char tile)
        {
            DrawAt(pos, tile, null);
        }

        public void DrawAt((int X, int Y) pos, char tile, Terminal.Gui.Attribute? attr = null)
        {
            if (pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height)
            {
                _backBuffer[pos.Y, pos.X] = new Cell {
                    Character = tile,
                    Attribute = Colors.Base.Normal
                };
            }
        }

        public void Render()
        {
            var driver = Application.Driver;
            
            if (_needsFullRedraw)
            {
                // Full redraw
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        var cell = _backBuffer[y, x];
                        driver.Move(x + 1, y + 1);
                        driver.SetAttribute(cell.Attribute);
                        driver.AddRune(cell.Character);
                    }
                }
                _needsFullRedraw = false;
            }
            else
            {
                EventSystem.RaiseEvent($"Partial redraw");
                // Partial redraw - only update changed cells
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        if (!_frontBuffer[y, x].Equals(_backBuffer[y, x]))
                        {
                            var cell = _backBuffer[y, x];
                            driver.Move(x + 1, y + 1);
                            driver.SetAttribute(cell.Attribute);
                            driver.AddRune(cell.Character);
                        }
                    }
                }
            }

            // Swap buffers
            (_frontBuffer, _backBuffer) = (_backBuffer, _frontBuffer);
        }

        public void Clear()
        {
            _needsFullRedraw = true;
        }
    }
}
