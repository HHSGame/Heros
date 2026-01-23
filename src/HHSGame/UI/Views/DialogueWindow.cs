using System.Collections.ObjectModel;
using HHSGame.Core.Dialogue;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Terminal.Gui.Input;
using Terminal.Gui.Drivers;

namespace HHSGame.UI.Views
{
    public sealed class DialogueWindow : Window
    {
        private readonly DialogueManager dialogueManager;
        private readonly TextView textView;
        private readonly ListView optionsView;
        private readonly ObservableCollection<string> optionItems = [];

        public DialogueWindow(DialogueManager dialogueManager)
        {
            this.dialogueManager = dialogueManager;
            Title = GUISettings.DialogueWindowTitle;
            X = Pos.Center();
            Y = Pos.Center();
            Width = Dim.Percent(70);
            Height = Dim.Percent(50);
            Visible = false;

            textView = new TextView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Percent(60),
                ReadOnly = true
            };

            optionsView = new ListView
            {
                X = 0,
                Y = Pos.Bottom(textView),
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                AllowsMarking = false,
                AllowsMultipleSelection = false
            };

            Add(textView);
            Add(optionsView);
            SetScheme(GUISettings.CommonWindowColorScheme);

            optionsView.SetSource(optionItems);
            dialogueManager.SessionChanged += (_, __) => Refresh();
        }

        public void ShowDialogue()
        {
            Visible = true;
            Refresh();
        }

        public bool HandleKeyEvent(Key key)
        {
            switch (key.KeyCode)
            {
                case KeyCode.CursorUp:
                    optionsView.MoveUp();
                    return true;
                case KeyCode.CursorDown:
                    optionsView.MoveDown();
                    return true;
                case KeyCode.Enter:
                    return HandleSelection();
                case KeyCode.Esc:
                    dialogueManager.EndDialogue();
                    return true;
                default:
                    return false;
            }
        }

        private bool HandleSelection()
        {
            if (optionsView.Source is null)
            {
                return false;
            }

            int index = optionsView.SelectedItem;
            return dialogueManager.TrySelectOption(index);
        }

        private void Refresh()
        {
            DialogueSession? session = dialogueManager.CurrentSession;
            if (session == null)
            {
                Visible = false;
                textView.Text = string.Empty;
                optionItems.Clear();
                return;
            }

            string nodeText = session.Node.Text;
            if (session.IsRepeatVisit)
            {
                nodeText = "[Seen] " + nodeText;
            }
            textView.Text = nodeText;
            optionItems.Clear();
            for (int i = 0; i < session.Options.Count; i++)
            {
                DialogueOption option = session.Options[i];
                string label = dialogueManager.BuildOptionLabel(session, option);
                optionItems.Add($"{i + 1}. {label}");
            }
            optionsView.SelectedItem = optionItems.Count > 0 ? 0 : -1;
            SetNeedsDraw();
        }
    }
}
