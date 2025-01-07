
using RpgGame.UI;

namespace RpgGame.Core {
    

    public abstract class GameActor : IDrawable {
        public int X { get; protected set; }
        public int Y { get; protected set; }

        public abstract char Glyph { get; }

        public void Draw(IDrawingContext ctx) {
            ctx.DrawAt((X, Y), Glyph);
        }
    }
}