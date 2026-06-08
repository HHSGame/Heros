using HHSGame.Core.Rendering;
using System.Collections.ObjectModel;
using HHSGame.Core.Quests;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Terminal.Gui.Input;
using Terminal.Gui.Drivers;

namespace HHSGame.UI.Views
{
    public sealed class QuestLogWindow : Window
    {
        private sealed class QuestLogEntry(string title, string details)
        {
            public string Title { get; } = title;
            public string Details { get; } = details;

            public override string ToString()
            {
                return Title;
            }
        }

        private readonly QuestManager questManager;
        private readonly ListView listView;
        private readonly TextView detailView;
        private readonly MapFrame mapWindow;
        private readonly ObservableCollection<QuestLogEntry> entries = [];

        public QuestLogWindow(QuestManager questManager, MapFrame mapWindow)
        {
            this.questManager = questManager;
            this.mapWindow = mapWindow;
            Title = GUISettings.QuestLogWindowTitle;
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
            questManager.QuestLogChanged += (_, __) => RefreshEntries();

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
                default:
                    return false;
            }
        }

        private void RefreshEntries()
        {
            entries.Clear();

            foreach (QuestState quest in questManager.Quests.OrderBy(q => q.Status == QuestStatus.Active ? 0 : q.Status == QuestStatus.Inactive ? 1 : 2))
            {
                string title = $"Quest: {quest.Definition.Name} [{quest.Status}]";
                string details = BuildDetails(quest.Definition.Description, quest.Definition.Hint, quest.BuildObjectiveLines());
                entries.Add(new QuestLogEntry(title, details));
            }

            foreach (AchievementState achievement in questManager.Achievements)
            {
                string status = achievement.IsUnlocked ? "Unlocked" : "Locked";
                string title = $"Achievement: {achievement.Definition.Name} [{status}]";
                string details = BuildDetails(achievement.Definition.Description, string.Empty, achievement.BuildObjectiveLines());
                entries.Add(new QuestLogEntry(title, details));
            }

            if (entries.Count == 0)
            {
                entries.Add(new QuestLogEntry("No quests available", string.Empty));
            }

            listView.SelectedItem = 0;
            UpdateDetails();
            SetNeedsDraw();
        }

        private void UpdateDetails()
        {
            if (listView.SelectedItem < 0 || listView.SelectedItem >= entries.Count)
            {
                detailView.Text = string.Empty;
                return;
            }

            detailView.Text = entries[listView.SelectedItem].Details;
        }

        private static string BuildDetails(string description, string hint, IReadOnlyList<string> objectives)
        {
            List<string> lines = [];
            if (!string.IsNullOrWhiteSpace(description))
            {
                lines.Add(description.Trim());
            }

            if (!string.IsNullOrWhiteSpace(hint))
            {
                if (lines.Count > 0)
                {
                    lines.Add(string.Empty);
                }
                lines.Add($"Hint: {hint.Trim()}");
            }

            if (objectives.Count > 0)
            {
                if (lines.Count > 0)
                {
                    lines.Add(string.Empty);
                }
                lines.Add("Objectives:");
                foreach (string objective in objectives)
                {
                    lines.Add($"- {objective}");
                }
            }

            return string.Join(Environment.NewLine, lines);
        }
    }
}
