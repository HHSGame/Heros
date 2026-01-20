using HHSGame.Core;
using Terminal.Gui.ViewBase;

namespace HHSGame.UI.Views
{
    public class StatusBarView : View
    {
        private readonly GameContext context;

        public StatusBarView(GameContext context, MapFrame mapWindow)
        {
            this.context = context;
            X = 0;
            Y = Pos.Bottom(mapWindow);
            Width = Dim.Fill();
            Height = GUISettings.StatusBarHeight;
            CanFocus = false;

            Events.OnGameMessageEvent += (_, __) => SetNeedsDraw();
            Events.OnTurnChanged += (_, __) => SetNeedsDraw();
            Events.OnInventoryChange += (_, __) => SetNeedsDraw();
        }

        protected override bool OnDrawingContent()
        {
            string content = BuildStatusLine();
            Move(0, 0);
            AddStr(content);
            return true;
        }

        private string BuildStatusLine()
        {
            Player? player = context.PlayerOrNull;
            int turnCount = context.TurnManager.TurnCount;

            if (player == null)
            {
                return PadToWidth("HP --/-- SP --/-- AP --/-- XP -- Lv -- Pos --,-- Time --");
            }

            string status =
                $"HP {player.Health}/{player.MaxHealth} " +
                $"SP {player.Sanity}/{player.MaxSanity} " +
                $"AP {player.CurrentAp}/{player.MaxAp} " +
                $"XP {player.Experience} " +
                $"Lv {player.Level} " +
                $"Pos {player.X},{player.Y} " +
                $"Time {turnCount}";

            return PadToWidth(status);
        }

        private string PadToWidth(string text)
        {
            if (Frame.Width <= 0)
            {
                return text;
            }

            if (text.Length > Frame.Width)
            {
                return text[..Frame.Width];
            }

            return text.PadRight(Frame.Width);
        }
    }
}
