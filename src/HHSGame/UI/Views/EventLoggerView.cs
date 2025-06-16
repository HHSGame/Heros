

using HHSGame.Core;
using Terminal.Gui.Drawing;
using Terminal.Gui.Configuration;
using Terminal.Gui.ViewBase;

namespace HHSGame.UI.Views
{
    public class EventLoggerView : View
    {
        private readonly List<string> eventMessages = [];
        private const int MaxEventMessages = 10;

        public EventLoggerView()
        {
            Width = Dim.Fill();
            Height = Dim.Fill();
            Height = MaxEventMessages + 2;
            EventSystem.OnGameMessageEvent += HandleGameEvent;
        }

        private void HandleGameEvent(object? sender, GameMessageEventArgs e)
        {
            LogEvent(e.Message);
        }

        public void LogEvent(string message)
        {
            eventMessages.Insert(0, message);
            if (eventMessages.Count > MaxEventMessages)
            {
                eventMessages.RemoveAt(MaxEventMessages);
            }
            SetNeedsDraw();
        }

        protected override bool OnDrawingContent()
        {
            for (int i = 0; i < eventMessages.Count; i++)
            {
                Move(0, i);
                SetAttribute(SchemeManager.GetScheme(Schemes.Base).GetAttributeForRole(VisualRole.Focus));
                AddStr(eventMessages[i]);
            }
            return false;
        }
    }
}