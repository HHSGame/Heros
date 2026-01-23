namespace HHSGame.Core.Map
{
    public sealed class HiddenFeature(Coordinate position, char revealedGlyph, string description)
    {
        public Coordinate Position { get; } = position;
        public char RevealedGlyph { get; } = revealedGlyph;
        public string Description { get; } = description;
        public bool Revealed { get; set; }
    }
}
