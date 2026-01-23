using HHSGame.Core;
using Terminal.Gui.ViewBase;

namespace HHSGame.UI.Views
{
    public class StatusBarView : View
    {
        private readonly GameContext context;
        private readonly UiStatusState uiStatus;

        public StatusBarView(GameContext context, MapFrame mapWindow, UiStatusState uiStatus)
        {
            this.context = context;
            this.uiStatus = uiStatus;
            X = 0;
            Y = Pos.Bottom(mapWindow);
            Width = Dim.Fill();
            Height = GUISettings.StatusBarHeight;
            CanFocus = false;

            Events.OnGameMessageEvent += (_, __) => SetNeedsDraw();
            Events.OnTurnChanged += (_, __) => SetNeedsDraw();
            Events.OnInventoryChange += (_, __) => SetNeedsDraw();
            uiStatus.Changed += (_, __) => SetNeedsDraw();
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

            string playerLabel = player.Name;
            if (context.Players.Count > 1)
            {
                int index = context.ActivePlayerIndex >= 0 ? context.ActivePlayerIndex + 1 : 1;
                playerLabel = $"{playerLabel} ({index}/{context.Players.Count})";
            }

            string status =
                $"P {playerLabel} " +
                $"HP {player.Health}/{player.MaxHealth} " +
                $"SP {player.Sanity}/{player.MaxSanity} " +
                $"AP {player.CurrentAp}/{player.MaxAp} " +
                $"XP {player.Experience} " +
                $"Lv {player.Level} " +
                $"Pos {player.X},{player.Y} " +
                $"Time {turnCount}";

            bool showCombatHints = context.StateMachine.CurrentState == GameStateType.Combat
                || context.EnemyManager.Enemies.Any(enemy => context.MapState.IsVisible(enemy.X, enemy.Y));
            if (showCombatHints)
            {
                status += " | M Move A Attack G Pick C Exit(no enemies) Enter End S Skills";
            }

            string baseMode = context.StateMachine.CurrentState == GameStateType.Combat ? "Combat" : "Travel";
            string mode = uiStatus.ModeOverride ?? baseMode;
            string detail = uiStatus.DetailOverride ?? string.Empty;
            status += $" | {mode}";
            if (!string.IsNullOrWhiteSpace(detail))
            {
                status += $": {detail}";
            }

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
