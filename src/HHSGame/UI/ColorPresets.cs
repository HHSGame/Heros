using Terminal.Gui.Drawing;

namespace HHSGame.UI
{
    using Attribute = Terminal.Gui.Drawing.Attribute;
    public static class ColorPresets
    {
        public static readonly Attribute GreyedOut = new(Color.DarkGray, Color.Black);

        // Terrain colors
        public static class Terrain
        {
            public static readonly Attribute Grass = new(Color.Green, Color.Black);
            public static readonly Attribute Water = new(Color.BrightBlue, Color.Black);
            public static readonly Attribute Stone = new(Color.Gray, Color.Black);
            public static readonly Attribute Sand = new(Color.BrightYellow, Color.Black);
            public static readonly Attribute Lava = new(Color.BrightRed, Color.Black);
            public static readonly Attribute Ice = new(Color.BrightCyan, Color.Black);
            public static readonly Attribute Forest = new(Color.Blue, Color.Black);
            public static readonly Attribute Swamp = new(Color.DarkGray, Color.Black);
        }

        // Enemy colors
        public static class Enemies
        {
            public static readonly Attribute Gangster = new(Color.Gray, Color.BrightGreen);
            public static readonly Attribute Bandit = new(Color.Blue, Color.BrightGreen);
            public static readonly Attribute BanditLeader = new(Color.DarkGray, Color.BrightGreen);
            public static readonly Attribute Thug = new(Color.Cyan, Color.DarkGray);
            public static readonly Attribute Soldier = new(Color.Gray, Color.DarkGray);
            public static readonly Attribute Officer = new(Color.Red, Color.White);
            // public static Attribute Dragon = new(Color.Red, Color.White);
            // public static Attribute Skeleton = new(Color.White, Color.Black);
            // public static Attribute Slime = new(Color.BrightCyan, Color.Black);
            // public static Attribute Demon = new(Color.BrightRed, Color.Black);
            // public static Attribute Ghost = new(Color.BrightMagenta, Color.Black);
        }

        // Player color
        public static readonly Attribute Player = new(Color.BrightYellow, Color.Black);
    }
}
