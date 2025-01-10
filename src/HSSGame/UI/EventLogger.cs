
using Terminal.Gui;
using HHSGame.Core;

namespace HHSGame.UI
{
    public class EventLogger : View
    {
        private readonly List<string> _eventMessages;
        private const int MaxEventMessages = 10;

        public EventLogger()
        {
            Width = Dim.Fill();
            Height = Dim.Fill();
            _eventMessages = new List<string>();
            Height = MaxEventMessages + 2;
            EventSystem.OnGameEvent += HandleGameEvent;
        }

        private void HandleGameEvent(object? sender, GameEvent e)
        {
            LogEvent(e.Message);
        }

        public void LogEvent(string message)
        {
            _eventMessages.Insert(0, message);
            if (_eventMessages.Count > MaxEventMessages)
            {
                _eventMessages.RemoveAt(MaxEventMessages);
            }
            SetNeedsDisplay();
        }

        public override void Redraw(Rect region)
        {
            for (int i = 0; i < _eventMessages.Count; i++)
            {
                Move(0, i);
                Driver.SetAttribute(ColorScheme.Focus);
                Driver.AddStr(_eventMessages[i]);
            }
        }
    }
}