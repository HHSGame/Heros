using Attribute = Terminal.Gui.Drawing.Attribute;

namespace HHSGame.Core.Rendering
{
    public struct Cell
    {
        public char Character { get; set; }
        public Attribute Attribute { get; set; }

        public readonly bool IsWalkable => Character is '.' or '▒';
    }
}
