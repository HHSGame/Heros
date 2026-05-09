using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Terminal.Gui.Input;
using Terminal.Gui.Drivers;

namespace HHSGame.UI.Views
{
    /// <summary>
    /// Help screen showing keyboard shortcuts and game mechanics.
    /// </summary>
    public class HelpView : Window
    {
        private readonly ListView contentList;

        private static readonly string[] HelpContent =
        [
            "=== HHSGame Help ===",
            "",
            "--- Movement ---",
            "Arrow keys / HJKL  Move (up/down/left/right)",
            "Y U B N            Move diagonally (NW/NE/SW/SE)",
            "",
            "--- Combat ---",
            "C                  Toggle combat mode",
            "Enter              Commit planned actions",
            "A                  Attack target (combat)",
            "M                  Move path (combat)",
            "",
            "--- Items ---",
            "G                  Pick up items",
            "I                  Open inventory",
            "",
            "--- Skills ---",
            "S                  Open skill menu",
            "",
            "--- Interaction ---",
            "T                  Talk to NPC",
            "Q                  Quest log",
            "",
            "--- System ---",
            "Ctrl+S             Save game",
            "Ctrl+L             Load game",
            "Tab / Space        Switch active player",
            "Ctrl+Q             Quit",
            "",
            "--- Gameplay ---",
            "Enemies appear as you explore. In combat mode,",
            "plan your actions then press Enter to execute.",
            "Skills consume AP in combat and can be used",
            "freely in exploration (some cost SP).",
            "Complete quests by talking to NPCs and exploring.",
            "",
            "Press Esc to return."
        ];

        public event EventHandler? Closed;

        public HelpView()
        {
            Title = "Help";
            X = 0;
            Y = 0;
            Width = Dim.Fill();
            Height = Dim.Fill();
            Visible = false;

            contentList = new ListView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                AllowsMarking = false,
                AllowsMultipleSelection = false
            };
            contentList.SetSource<string>(new System.Collections.ObjectModel.ObservableCollection<string>(HelpContent));
            Add(contentList);
        }

        public bool HandleKeyEvent(Key key)
        {
            switch (key.KeyCode)
            {
                case KeyCode.CursorUp:
                    contentList.MoveUp();
                    return true;
                case KeyCode.CursorDown:
                    contentList.MoveDown();
                    return true;
                case KeyCode.Esc:
                case KeyCode.Q:
                    Visible = false;
                    Closed?.Invoke(this, EventArgs.Empty);
                    return true;
                default:
                    return false;
            }
        }

        public void Show()
        {
            Visible = true;
            contentList.SelectedItem = 0;
        }
    }
}