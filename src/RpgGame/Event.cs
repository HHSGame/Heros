namespace RpgGame
{
    public class GameEvent : EventArgs
    {
        public string Message { get; }

        public GameEvent(string message)
        {
            Message = message;
        }

        public override string ToString()
        {
            return Message;
        }
    }

    public static class EventSystem
    {
        public static event EventHandler<GameEvent>? OnGameEvent;

        public static void RaiseEvent(string message)
        {
            OnGameEvent?.Invoke(null, new GameEvent(message));
        }
    }
}
