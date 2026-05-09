using System.Collections.ObjectModel;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Terminal.Gui.Input;
using Terminal.Gui.Drivers;

namespace HHSGame.UI.Views
{
    /// <summary>
    /// Main menu shown at game start. Offers New Game, Continue, Load, Help, and Quit.
    /// </summary>
    public class MainMenuView : Window
    {
        private readonly ListView menuList;
        private readonly List<string> menuItems = ["New Game", "Continue", "Load Game", "Help", "Quit"];

        public event EventHandler? NewGameSelected;
        public event EventHandler? ContinueSelected;
        public event EventHandler? LoadSelected;
        public event EventHandler? HelpSelected;
        public event EventHandler? QuitSelected;

        public MainMenuView()
        {
            Title = "HHSGame";
            X = 0;
            Y = 0;
            Width = Dim.Fill();
            Height = Dim.Fill();
            Visible = false;

            Label titleLabel = new()
            {
                Text = "HHSGame - Roguelike Adventure",
                X = Pos.Center(),
                Y = 2,
                Width = Dim.Fill()
            };
            Add(titleLabel);

            Label subtitleLabel = new()
            {
                Text = "A terminal-based roguelike in the style of NetHack",
                X = Pos.Center(),
                Y = 4,
                Width = Dim.Fill()
            };
            Add(subtitleLabel);

            menuList = new ListView
            {
                X = Pos.Center(),
                Y = 8,
                Width = 30,
                Height = menuItems.Count + 2,
                AllowsMarking = false,
                AllowsMultipleSelection = false
            };
            menuList.SetSource<string>(new ObservableCollection<string>(menuItems));
            menuList.SelectedItem = 0;
            Add(menuList);

            Label helpHint = new()
            {
                Text = "Arrow keys to select, Enter to confirm",
                X = Pos.Center(),
                Y = Pos.Bottom(menuList) + 2,
                Width = Dim.Fill()
            };
            Add(helpHint);
        }

        public bool HandleKeyEvent(Key key)
        {
            switch (key.KeyCode)
            {
                case KeyCode.CursorUp:
                    menuList.MoveUp();
                    return true;
                case KeyCode.CursorDown:
                    menuList.MoveDown();
                    return true;
                case KeyCode.Enter:
                    HandleSelection();
                    return true;
                default:
                    return false;
            }
        }

        private void HandleSelection()
        {
            int index = menuList.SelectedItem;
            if (index < 0 || index >= menuItems.Count)
            {
                return;
            }

            switch (index)
            {
                case 0: // New Game
                    Visible = false;
                    NewGameSelected?.Invoke(this, EventArgs.Empty);
                    break;
                case 1: // Continue
                    ContinueSelected?.Invoke(this, EventArgs.Empty);
                    break;
                case 2: // Load Game
                    LoadSelected?.Invoke(this, EventArgs.Empty);
                    break;
                case 3: // Help
                    HelpSelected?.Invoke(this, EventArgs.Empty);
                    break;
                case 4: // Quit
                    QuitSelected?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }

        public void Show()
        {
            Visible = true;
            menuList.SelectedItem = 0;
        }
    }
}