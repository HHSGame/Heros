using HHSGame.Core.Tutorial;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Terminal.Gui.Input;
using Terminal.Gui.Drivers;

namespace HHSGame.UI.Views
{
    /// <summary>
    /// A modal dialog that displays tutorial hints to the player.
    /// Shows the step title, message, and options to continue or skip.
    /// </summary>
    public class TutorialHintDialog : Window
    {
        private readonly Label titleLabel;
        private readonly Label messageLabel;
        private readonly Label stepLabel;
        private readonly Label helpLabel;

        public event EventHandler? ContinuePressed;
        public event EventHandler? SkipPressed;

        public TutorialHintDialog()
        {
            Title = "Tutorial";
            X = Pos.Center();
            Y = Pos.Center();
            Width = 60;
            Height = 12;
            Visible = false;

            titleLabel = new Label
            {
                X = 1,
                Y = 0,
                Width = Dim.Fill() - 2,
                Text = string.Empty
            };
            Add(titleLabel);

            messageLabel = new Label
            {
                X = 1,
                Y = 2,
                Width = Dim.Fill() - 2,
                Height = 5,
                Text = string.Empty
            };
            Add(messageLabel);

            stepLabel = new Label
            {
                X = 1,
                Y = 8,
                Width = Dim.Fill() - 2,
                Text = string.Empty
            };
            Add(stepLabel);

            helpLabel = new Label
            {
                X = 1,
                Y = 10,
                Width = Dim.Fill() - 2,
                Text = "Enter: Continue | S: Skip Tutorial | Esc: Dismiss"
            };
            Add(helpLabel);
        }

        public void ShowHint(TutorialStep step, int completed, int total)
        {
            titleLabel.Text = step.Title;
            messageLabel.Text = WrapText(step.Message, 56);
            stepLabel.Text = $"Step {completed + 1}/{total}";
            Visible = true;
            SetFocus();
        }

        public bool HandleKeyEvent(Key key)
        {
            switch (key.KeyCode)
            {
                case KeyCode.Enter:
                case KeyCode.Esc:
                    Visible = false;
                    ContinuePressed?.Invoke(this, EventArgs.Empty);
                    return true;
                case KeyCode.S:
                    Visible = false;
                    SkipPressed?.Invoke(this, EventArgs.Empty);
                    return true;
                default:
                    return false;
            }
        }

        public void Hide()
        {
            Visible = false;
        }

        private static string WrapText(string text, int maxWidth)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxWidth)
            {
                return text;
            }

            List<string> lines = [];
            string remaining = text;
            while (remaining.Length > maxWidth)
            {
                int breakPoint = remaining.LastIndexOf(' ', maxWidth);
                if (breakPoint <= 0)
                {
                    breakPoint = maxWidth;
                }
                lines.Add(remaining[..breakPoint]);
                remaining = remaining[(breakPoint + 1)..];
            }
            if (remaining.Length > 0)
            {
                lines.Add(remaining);
            }

            return string.Join("\n", lines);
        }
    }
}