using HHSGame.Core.Map;
using HHSGame.Core.Rendering;
using Terminal.Gui.ViewBase;
using HHSGame.UI.Views;

namespace HHSGame.UI
{
    public class MapViewDrawingContext(MapView mapView, MapState mapState) : IDrawingContext
    {
        private readonly MapView mapView = mapView;

        public Viewport Viewport { get; } = new(0, 0, 0, 0);

        // Explicit interface implementation for original method
        public void DrawAt((int X, int Y) pos, char tile)
        {
            this.DrawAt(pos, new Cell { Character = tile, Attribute = ColorPresets.Terrain.Stone });
        }

        public void DrawAt((int X, int Y) pos, Cell cell)
        {
            if (Viewport.Width != mapView.Frame.Width || Viewport.Height != mapView.Frame.Height)
            {
                Viewport.Resize(mapView.Frame.Width, mapView.Frame.Height);
            }
            if (!Viewport.Contains(pos))
            {
                return;
            }

            // Check visibility
            if (mapState != null)
            {
                if (mapState.IsVisible(pos.X, pos.Y))
                {
                    // Visible - use normal colors
                    (int posX, int posY) = Viewport.ToLocal(pos);
                    mapView.SetCell(posX, posY, cell.Character, cell.Attribute);
                }
                else if (mapState.WasVisited(pos.X, pos.Y))
                {
                    // Visited but not visible - grey out
                    (int posX, int posY) = Viewport.ToLocal(pos);
                    mapView.SetCell(posX, posY, cell.Character, ColorPresets.GreyedOut);
                }
                else
                {
                    // Visited but not visible - grey out
                    (int posX, int posY) = Viewport.ToLocal(pos);
                    mapView.SetCell(posX, posY, ' ', ColorPresets.GreyedOut);
                }
                // Else - don't draw at all
            }
            else
            {
                // Fallback if no game world reference
                (int posX, int posY) = Viewport.ToLocal(pos);
                mapView.SetCell(posX, posY, cell.Character, cell.Attribute);
            }
        }

        public void Render()
        {
            mapView.SetNeedsDraw();
        }

        public void AttachTo(View parent)
        {
            parent.Add(mapView);
        }

        public void Clear()
        {
            mapView.ClearBuffer();
        }
    }
}
