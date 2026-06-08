using HHSGame.Core.Rendering;
using System.Collections.ObjectModel;
using HHSGame.Core;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace HHSGame.UI.Views
{
    public class ActionSequenceFrame : FrameView
    {
        private readonly Game game;
        private readonly ListView listView;
        private readonly ObservableCollection<string> actionItems = [];

        public ActionSequenceFrame(MessageFrame messageFrame, StatusBarView statusBarView, Game game)
        {
            Title = GUISettings.ActionSequenceWindowTitle;
            X = Pos.Right(messageFrame);
            Y = Pos.Bottom(statusBarView);
            Width = GUISettings.ActionSequenceWidth;
            Height = GUISettings.MessageWindowHeight;
            this.game = game;

            listView = new ListView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                AllowsMarking = false,
                AllowsMultipleSelection = false
            };

            listView.SetSource(actionItems);
            Add(listView);
            SetScheme(GUISettingsExtensions.CommonWindowColorScheme);

            UpdateList();
            Events.OnActionSequenceChanged += (_, __) => UpdateList();
            Events.OnTurnChanged += (_, __) => UpdateList();
        }

        private void UpdateList()
        {
            actionItems.Clear();
            foreach (string entry in game.GetPlannedActionDescriptions())
            {
                actionItems.Add(entry);
            }
            SetNeedsDraw();
        }
    }
}
