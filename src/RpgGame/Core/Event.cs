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

    public enum TurnState
    {
        PlayerTurn,
        EnemyTurn
    }

    public class TurnEvent : EventArgs
    {
        public TurnState State { get; }

        public TurnEvent(TurnState state)
        {
            State = state;
        }
    }

    public static class EventSystem
    {
        public static event EventHandler<GameEvent>? OnGameEvent;
        public static event EventHandler<TurnEvent>? OnTurnChanged;

        public static void RaiseEvent(string message)
        {
            OnGameEvent?.Invoke(null, new GameEvent(message));
        }

        public static void RaiseTurnChanged(TurnState state)
        {
            OnTurnChanged?.Invoke(null, new TurnEvent(state));
        }
    }
}
