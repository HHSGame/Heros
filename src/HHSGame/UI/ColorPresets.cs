using Terminal.Gui.Drawing;
using Attribute = Terminal.Gui.Drawing.Attribute;

namespace HHSGame.UI
{
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

        // Enemy colors (WWII themed)
        public static class Enemies
        {
            public static readonly Attribute Occupier = new(Color.Gray, Color.BrightRed);
            public static readonly Attribute Officer = new(Color.BrightYellow, Color.DarkGray);
            public static readonly Attribute Gestapo = new(Color.Black, Color.BrightRed);
            public static readonly Attribute Collaborator = new(Color.DarkGray, Color.BrightYellow);
            public static readonly Attribute Informer = new(Color.Cyan, Color.DarkGray);
            public static readonly Attribute Bandit = new(Color.BrightRed, Color.Black);
            public static readonly Attribute BanditBoss = new(Color.BrightYellow, Color.BrightRed);
            public static readonly Attribute Deserter = new(Color.Gray, Color.DarkGray);
            public static readonly Attribute Sniper = new(Color.DarkGray, Color.Black);
            public static readonly Attribute Patrol = new(Color.BrightGreen, Color.Black);
            public static readonly Attribute Medic = new(Color.White, Color.BrightRed);
            public static readonly Attribute Dog = new(Color.BrightYellow, Color.Black);
            public static readonly Attribute Tank = new(Color.BrightCyan, Color.DarkGray);
            public static readonly Attribute Turncoat = new(Color.BrightMagenta, Color.Black);
        }

        // Player color
        public static readonly Attribute Player = new(Color.BrightYellow, Color.Red);
        public static readonly Attribute PlayerActive = new(Color.Black, Color.BrightYellow);

        public static class Npcs
        {
            public static readonly Attribute Default = new(Color.White, Color.DarkGray);
            public static readonly Attribute Trader = new(Color.BrightGreen, Color.Black);
            public static readonly Attribute Scholar = new(Color.BrightCyan, Color.Black);
            public static readonly Attribute Guard = new(Color.BrightRed, Color.Black);
        }

        public static readonly Attribute PathPreview = new(Color.BrightCyan, Color.Black);
        public static readonly Attribute TargetPreview = new(Color.BrightMagenta, Color.Black);
        public static readonly Attribute TargetPreviewHighlight = new(Color.Black, Color.BrightMagenta);
        public static readonly Attribute PlannedDestination = new(Color.BrightGreen, Color.Black);
        public static readonly Attribute RangePreview = new(Color.BrightBlue, Color.Black);
    }
}
