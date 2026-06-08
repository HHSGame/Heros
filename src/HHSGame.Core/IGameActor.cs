using HHSGame.Core.Rendering;

namespace HHSGame.Core
{
    public interface IGameActor : IDrawable
    {
        int X { get; }
        int Y { get; }

        string Name { get; }

        char Glyph { get; }

        GameAttribute Attribute { get; }

        void IDrawable.Draw(IDrawingContext ctx)
        {
            ctx.DrawAt((X, Y), new Cell { Character = Glyph, Attribute = Attribute });
        }
    }
}
