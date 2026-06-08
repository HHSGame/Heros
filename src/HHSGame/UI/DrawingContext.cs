using HHSGame.Core.Map;
using HHSGame.Core.Rendering;
using HHSGame.UI.Views;
using Terminal.Gui.ViewBase;

namespace HHSGame.UI
{
    public class MapViewDrawingContext(MapView mapView, MapState mapState) : IDrawingContext
    {
        private readonly MapView mapView = mapView;

        public Viewport Viewport { get; } = new(0, 0, 0, 0);

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

            if (mapState != null)
            {
                if (mapState.IsVisible(pos.X, pos.Y))
                {
                    (int posX, int posY) = Viewport.ToLocal(pos);
                    mapView.SetCell(posX, posY, cell.Character, cell.Attribute.ToTerminalAttribute());
                }
                else if (mapState.WasVisited(pos.X, pos.Y))
                {
                    (int posX, int posY) = Viewport.ToLocal(pos);
                    mapView.SetCell(posX, posY, cell.Character, ColorPresets.GreyedOut.ToTerminalAttribute());
                }
                else
                {
                    (int posX, int posY) = Viewport.ToLocal(pos);
                    mapView.SetCell(posX, posY, ' ', ColorPresets.GreyedOut.ToTerminalAttribute());
                }
            }
            else
            {
                (int posX, int posY) = Viewport.ToLocal(pos);
                mapView.SetCell(posX, posY, cell.Character, cell.Attribute.ToTerminalAttribute());
            }
        }

        public void Render()
        {
            mapView.SetNeedsDraw();
        }

        public void Clear()
        {
            mapView.ClearBuffer();
        }

        public void AttachTo(object parent)
        {
            if (parent is View view)
            {
                view.Add(mapView);
            }
        }
    }
}
