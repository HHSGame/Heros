namespace HHSGame.Core.Rendering
{
    /// <summary>
    /// 平台无关的字符属性（前景色 + 背景色），替代 Terminal.Gui.Drawing.Attribute。
    /// </summary>
    public readonly struct GameAttribute : IEquatable<GameAttribute>
    {
        public GameColor Foreground { get; }
        public GameColor Background { get; }

        public GameAttribute(GameColor foreground, GameColor background)
        {
            Foreground = foreground;
            Background = background;
        }

        public bool Equals(GameAttribute other)
        {
            return Foreground == other.Foreground && Background == other.Background;
        }

        public override bool Equals(object? obj)
        {
            return obj is GameAttribute other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Foreground, Background);
        }

        public static bool operator ==(GameAttribute left, GameAttribute right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GameAttribute left, GameAttribute right)
        {
            return !left.Equals(right);
        }
    }
}
