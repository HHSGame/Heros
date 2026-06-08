using HHSGame.Core.Rendering;
using Terminal.Gui.Drawing;
using Attribute = Terminal.Gui.Drawing.Attribute;
using Color = Terminal.Gui.Drawing.Color;

namespace HHSGame
{
    /// <summary>
    /// Terminal.Gui 映射扩展，将平台无关的 GameAttribute 转换为 Terminal.Gui 的 Attribute。
    /// </summary>
    public static class TerminalGuiMapping
    {
        public static Attribute ToTerminalAttribute(this GameAttribute attribute)
        {
            return new Attribute(attribute.Foreground.ToTerminalColor(), attribute.Background.ToTerminalColor());
        }

        public static GameAttribute ToGameAttribute(this Attribute attribute)
        {
            return new GameAttribute(attribute.Foreground.ToGameColor(), attribute.Background.ToGameColor());
        }

        public static Color ToTerminalColor(this GameColor color)
        {
            if (color == GameColor.Black) return Color.Black;
            if (color == GameColor.Red) return Color.Red;
            if (color == GameColor.Green) return Color.Green;
            if (color == GameColor.Yellow) return Color.Yellow;
            if (color == GameColor.Blue) return Color.Blue;
            if (color == GameColor.Magenta) return Color.Magenta;
            if (color == GameColor.Cyan) return Color.Cyan;
            if (color == GameColor.White) return Color.White;
            if (color == GameColor.DarkGray) return Color.DarkGray;
            if (color == GameColor.BrightRed) return Color.BrightRed;
            if (color == GameColor.BrightGreen) return Color.BrightGreen;
            if (color == GameColor.BrightYellow) return Color.BrightYellow;
            if (color == GameColor.BrightBlue) return Color.BrightBlue;
            if (color == GameColor.BrightMagenta) return Color.BrightMagenta;
            if (color == GameColor.BrightCyan) return Color.BrightCyan;
            if (color == GameColor.Gray) return Color.Gray;
            return Color.Black;
        }

        public static GameColor ToGameColor(this Color color)
        {
            if (color == Color.Black) return GameColor.Black;
            if (color == Color.Red) return GameColor.Red;
            if (color == Color.Green) return GameColor.Green;
            if (color == Color.Yellow) return GameColor.Yellow;
            if (color == Color.Blue) return GameColor.Blue;
            if (color == Color.Magenta) return GameColor.Magenta;
            if (color == Color.Cyan) return GameColor.Cyan;
            if (color == Color.White) return GameColor.White;
            if (color == Color.DarkGray) return GameColor.DarkGray;
            if (color == Color.BrightRed) return GameColor.BrightRed;
            if (color == Color.BrightGreen) return GameColor.BrightGreen;
            if (color == Color.BrightYellow) return GameColor.BrightYellow;
            if (color == Color.BrightBlue) return GameColor.BrightBlue;
            if (color == Color.BrightMagenta) return GameColor.BrightMagenta;
            if (color == Color.BrightCyan) return GameColor.BrightCyan;
            if (color == Color.Gray) return GameColor.Gray;
            return GameColor.Black;
        }
    }
}
