namespace HHSGame.Core.Rendering
{
    public static class ColorPresets
    {
        public static readonly GameAttribute GreyedOut = new(GameColor.DarkGray, GameColor.Black);

        // Terrain colors
        public static class Terrain
        {
            public static readonly GameAttribute Grass = new(GameColor.Green, GameColor.Black);
            public static readonly GameAttribute Water = new(GameColor.BrightBlue, GameColor.Black);
            public static readonly GameAttribute Stone = new(GameColor.Gray, GameColor.Black);
            public static readonly GameAttribute Sand = new(GameColor.BrightYellow, GameColor.Black);
            public static readonly GameAttribute Lava = new(GameColor.BrightRed, GameColor.Black);
            public static readonly GameAttribute Ice = new(GameColor.BrightCyan, GameColor.Black);
            public static readonly GameAttribute Forest = new(GameColor.Blue, GameColor.Black);
            public static readonly GameAttribute Swamp = new(GameColor.DarkGray, GameColor.Black);
        }

        // Enemy colors (WWII themed)
        public static class Enemies
        {
            public static readonly GameAttribute Occupier = new(GameColor.Gray, GameColor.BrightRed);
            public static readonly GameAttribute Officer = new(GameColor.BrightYellow, GameColor.DarkGray);
            public static readonly GameAttribute Gestapo = new(GameColor.Black, GameColor.BrightRed);
            public static readonly GameAttribute Collaborator = new(GameColor.DarkGray, GameColor.BrightYellow);
            public static readonly GameAttribute Informer = new(GameColor.Cyan, GameColor.DarkGray);
            public static readonly GameAttribute Bandit = new(GameColor.BrightRed, GameColor.Black);
            public static readonly GameAttribute BanditBoss = new(GameColor.BrightYellow, GameColor.BrightRed);
            public static readonly GameAttribute Deserter = new(GameColor.Gray, GameColor.DarkGray);
            public static readonly GameAttribute Sniper = new(GameColor.DarkGray, GameColor.Black);
            public static readonly GameAttribute Patrol = new(GameColor.BrightGreen, GameColor.Black);
            public static readonly GameAttribute Medic = new(GameColor.White, GameColor.BrightRed);
            public static readonly GameAttribute Dog = new(GameColor.BrightYellow, GameColor.Black);
            public static readonly GameAttribute Tank = new(GameColor.BrightCyan, GameColor.DarkGray);
            public static readonly GameAttribute Turncoat = new(GameColor.BrightMagenta, GameColor.Black);
        }

        // Player color
        public static readonly GameAttribute Player = new(GameColor.BrightYellow, GameColor.Red);
        public static readonly GameAttribute PlayerActive = new(GameColor.Black, GameColor.BrightYellow);

        public static class Npcs
        {
            public static readonly GameAttribute Default = new(GameColor.White, GameColor.DarkGray);
            public static readonly GameAttribute Trader = new(GameColor.BrightGreen, GameColor.Black);
            public static readonly GameAttribute Scholar = new(GameColor.BrightCyan, GameColor.Black);
            public static readonly GameAttribute Guard = new(GameColor.BrightRed, GameColor.Black);
        }

        public static readonly GameAttribute PathPreview = new(GameColor.BrightCyan, GameColor.Black);
        public static readonly GameAttribute TargetPreview = new(GameColor.BrightMagenta, GameColor.Black);
        public static readonly GameAttribute TargetPreviewHighlight = new(GameColor.Black, GameColor.BrightMagenta);
        public static readonly GameAttribute PlannedDestination = new(GameColor.BrightGreen, GameColor.Black);
        public static readonly GameAttribute RangePreview = new(GameColor.BrightBlue, GameColor.Black);
    }
}
