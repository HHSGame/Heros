using HHSGame.UI;

namespace HHSGame.Core
{


    public interface IGameActor : IDrawable
    {
        int X { get; }
        int Y { get; }

        abstract string Name { get; }

        abstract char Glyph { get; }

        abstract Terminal.Gui.Drawing.Attribute Attribute { get; }

        void IDrawable.Draw(IDrawingContext ctx)
        {
            ctx.DrawAt((X, Y), new Cell { Character = Glyph, Attribute = Attribute });
        }
    }
}
