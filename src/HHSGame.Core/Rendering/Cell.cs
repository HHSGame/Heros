namespace HHSGame.Core.Rendering
{
    public struct Cell
    {
        public char Character { get; set; }
        public GameAttribute Attribute { get; set; }

        public readonly bool IsWalkable => Character is '.' or '▒';
    }
}
