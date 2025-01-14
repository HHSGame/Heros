using HHSGame.UI;

namespace HHSGame.Core
{


    public interface IGameActor : IDrawable
    {
        public int X { get; }
        public int Y { get; }

        public abstract string Name { get; }

        public abstract char Glyph { get; }

        public abstract Terminal.Gui.Attribute Attribute { get; }

        void IDrawable.Draw(IDrawingContext ctx)
        {
            ctx.DrawAt((X, Y), new Cell { Character = Glyph, Attribute = Attribute });
        }
    }
}
