using HHSGame.Core.Rendering;
using System.Collections.ObjectModel;
using HHSGame.Core;
using HHSGame.Core.SkillSystem;
using Terminal.Gui.Drivers;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace HHSGame.UI.Views
{
    public sealed class SkillActionWindow : Window
    {
        private sealed class SkillEntry(SkillActionDefinition definition)
        {
            public SkillActionDefinition Definition { get; } = definition;

            public override string ToString()
            {
                return Definition.Name;
            }
        }

        private readonly Game game;
        private readonly ListView listView;
        private readonly TextView detailView;
        private readonly MapFrame mapWindow;
        private readonly ObservableCollection<SkillEntry> entries = [];

        public event EventHandler<SkillActionDefinition>? ActionSelected;

        public SkillActionWindow(Game game, MapFrame mapWindow)
        {
            this.game = game;
            this.mapWindow = mapWindow;
            Title = GUISettings.SkillWindowTitle;
            X = Pos.Right(mapWindow);
            Y = 0;
            Height = Dim.Fill()! - (GUISettings.MessageWindowHeight + GUISettings.StatusBarHeight);
            Width = Dim.Percent(34);
            Visible = false;

            listView = new ListView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Percent(45),
                AllowsMarking = false,
                AllowsMultipleSelection = false
            };

            detailView = new TextView
            {
                X = 0,
                Y = Pos.Bottom(listView),
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ReadOnly = true
            };

            Add(listView);
            Add(detailView);
            SetScheme(GUISettingsExtensions.CommonWindowColorScheme);

            listView.SelectedItemChanged += (_, __) => UpdateDetails();
            listView.SetSource(entries);
            RefreshEntries();
        }

        public void Toggle()
        {
            if (!Visible)
            {
                Visible = true;
                mapWindow.Width = Dim.Percent(66);
                mapWindow.SetNeedsDraw();
                RefreshEntries();
                SetNeedsDraw();
            }
            else
            {
                Visible = false;
                mapWindow.Width = Dim.Fill()! - 20;
                mapWindow.SetNeedsDraw();
                SetNeedsDraw();
            }
        }

        public void RefreshEntries()
        {
            entries.Clear();

            foreach (SkillActionDefinition action in SkillActionCatalog.All)
            {
                entries.Add(new SkillEntry(action));
            }

            listView.SelectedItem = 0;
            UpdateDetails();
        }

        public bool HandleKeyEvent(Key key)
        {
            switch (key.KeyCode)
            {
                case KeyCode.CursorUp:
                    listView.MoveUp();
                    return true;
                case KeyCode.CursorDown:
                    listView.MoveDown();
                    return true;
                case KeyCode.Esc:
                    Toggle();
                    return true;
                case KeyCode.Enter:
                    return HandleSelection();
                default:
                    return false;
            }
        }

        private bool HandleSelection()
        {
            if (listView.SelectedItem < 0 || listView.SelectedItem >= entries.Count)
            {
                return false;
            }

            SkillActionDefinition action = entries[listView.SelectedItem].Definition;
            ActionSelected?.Invoke(this, action);
            return true;
        }

        private void UpdateDetails()
        {
            if (listView.SelectedItem < 0 || listView.SelectedItem >= entries.Count)
            {
                detailView.Text = string.Empty;
                return;
            }

            Player? player = game.Player;
            SkillActionDefinition action = entries[listView.SelectedItem].Definition;
            int skillValue = 0;
            if (player != null)
            {
                skillValue = game.Context.PartyState.GetEffectiveSkillValue(player, action.Skill);
            }

            string detailText = BuildDetails(action, skillValue);
            detailView.Text = detailText;
        }

        private static string BuildDetails(SkillActionDefinition action, int skillValue)
        {
            List<string> lines = [];
            lines.Add(action.Description);
            lines.Add(string.Empty);
            lines.Add($"Skill: {action.Skill} ({skillValue})");
            lines.Add($"AP Cost: {action.ApCost}");
            lines.Add($"Target: {DescribeTarget(action.TargetType)}");
            return string.Join(Environment.NewLine, lines);
        }

        private static string DescribeTarget(SkillActionTargetType targetType)
        {
            return targetType switch
            {
                SkillActionTargetType.None => "Self",
                SkillActionTargetType.Direction => "Direction",
                SkillActionTargetType.AdjacentEnemy => "Adjacent enemy",
                SkillActionTargetType.AdjacentNpc => "Adjacent NPC",
                SkillActionTargetType.AdjacentAllyOrSelf => "Self or adjacent ally",
                SkillActionTargetType.AdjacentDoor => "Adjacent door",
                SkillActionTargetType.RangedEnemy => "Enemy in range",
                _ => "Self"
            };
        }
    }
}
