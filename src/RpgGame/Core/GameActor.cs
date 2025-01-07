using Terminal.Gui;
using RpgGame.UI;

namespace RpgGame.Core {
    

    public abstract class GameActor : IDrawable {
        public int X { get; protected set; }
        public int Y { get; protected set; }

        public virtual string Name => "Unknown";
        
        public abstract char Glyph { get; }

        public virtual Terminal.Gui.Attribute Attribute => Colors.Base.Normal;

        public void Draw(IDrawingContext ctx) {
            ctx.DrawAt((X, Y), new Cell {Character = Glyph, Attribute = Attribute});
        }
    }
}
