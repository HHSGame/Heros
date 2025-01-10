using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace HHSGame.UI
{
    public static class ColorPresets
    {
        public static Attribute GreyedOut = new(Color.DarkGray, Color.Black);
        
        // Terrain colors
        public static class Terrain
        {
            public static Attribute Grass = new(Color.Green, Color.Black);
            public static Attribute Water = new(Color.BrightBlue, Color.Black);
            public static Attribute Stone = new(Color.Gray, Color.Black);
            public static Attribute Sand = new(Color.BrightYellow, Color.Black);
            public static Attribute Lava = new(Color.BrightRed, Color.Black);
            public static Attribute Ice = new(Color.BrightCyan, Color.Black);
            public static Attribute Forest = new(Color.Blue, Color.Black);
            public static Attribute Swamp = new(Color.DarkGray, Color.Black);
        }

        // Enemy colors
        public static class Enemies
        {
            public static Attribute Gangster = new(Color.Gray, Color.BrightGreen);
            public static Attribute Bandit = new(Color.Blue, Color.BrightGreen);
            public static Attribute BanditLeader = new(Color.DarkGray, Color.BrightGreen);
            // public static Attribute Dragon = new(Color.Red, Color.White);
            // public static Attribute Skeleton = new(Color.White, Color.Black);
            // public static Attribute Slime = new(Color.BrightCyan, Color.Black);
            // public static Attribute Demon = new(Color.BrightRed, Color.Black);
            // public static Attribute Ghost = new(Color.BrightMagenta, Color.Black);
        }

        // Player color
        public static Attribute Player = new(Color.BrightYellow, Color.Black);
    }
}
