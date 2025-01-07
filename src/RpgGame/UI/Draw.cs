
namespace RpgGame.UI {

    using Position = (int X, int Y);

    public interface IDrawingContext {
        void DrawAt(Position pos, char tile);
        void Clear();
        void Render();
    }

    public interface IDrawable {
        void Draw(IDrawingContext ctx);
    }
}