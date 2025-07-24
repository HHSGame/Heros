

using HHSGame.Core;
using Terminal.Gui.ViewBase;

namespace HHSGame.UI.Views
{
    public class EventLoggerView : View
    {
        private readonly Queue<string> eventMessages = [];
        private const int MaxEventMessages = 10;

        public EventLoggerView()
        {
            Width = Dim.Fill();
            Height = Dim.Fill();
            Height = MaxEventMessages + 2;
            Events.OnGameMessageEvent += HandleGameEvent;
        }

        private void HandleGameEvent(object? sender, GameMessageEventArgs e)
        {
            LogEvent(e.Message);
        }

        public void LogEvent(string message)
        {
            eventMessages.Enqueue(message);
            if (eventMessages.Count > MaxEventMessages)
            {
                eventMessages.Dequeue();
            }
            SetNeedsDraw();
        }

        protected override bool OnDrawingContent()
        {
            int i = 0;
            foreach (string message in eventMessages.Reverse())
            {
                Move(0, i);
                AddStr(message);
                i++;
            }
            return true;
        }
    }
}