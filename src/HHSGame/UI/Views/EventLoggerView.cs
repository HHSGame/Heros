
using Terminal.Gui;
using HHSGame.Core;

namespace HHSGame.UI.Views
{
    public class EventLoggerView : View
    {
        private readonly List<string> _eventMessages;
        private const int MaxEventMessages = 10;

        public EventLoggerView()
        {
            Width = Dim.Fill();
            Height = Dim.Fill();
            _eventMessages = new List<string>();
            Height = MaxEventMessages + 2;
            EventSystem.OnGameMessageEvent += HandleGameEvent;
        }

        private void HandleGameEvent(object? sender, GameMessageEventArgs e)
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

        public override void Redraw(Rect bounds)
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