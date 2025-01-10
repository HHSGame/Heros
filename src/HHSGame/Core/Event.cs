namespace HHSGame.Core
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

    public enum InventoryEventType {
        PickUp,
        Drop,
        Use,
        Remove,
    }

    public class InventoryChangeEvent : EventArgs
    {
        InventoryEventType EventType { get; }
        public Item Item { get; }

        public InventoryChangeEvent(InventoryEventType type, Item item)
        {
            EventType = type;
            Item = item;
        }
    }

    public enum SurroundingsChangeType {
        Movement,
        DropLoot,
        PickUpLoot,
        DropItem,
    }

    public class SurroundingsChangeEvent : EventArgs
    {
        public (int X, int Y) Position;
        public int FOVRadius; 

        public SurroundingsChangeEvent((int X, int Y) position, int fovRadius, SurroundingsChangeType type)
        {
            Position = position;
            FOVRadius = fovRadius;
        }
    }

    public static class EventSystem
    {

        public static event EventHandler<GameEvent>? OnGameEvent;
        public static event EventHandler<TurnEvent>? OnTurnChanged;
        public static event EventHandler<InventoryChangeEvent>? OnInventoryChange;

        public static event EventHandler<SurroundingsChangeEvent>? OnSurroundingsChange;

        public static void RaiseEvent(string message)
        {
            OnGameEvent?.Invoke(null, new GameEvent(message));
        }

        public static void RaiseTurnChanged(TurnState state)
        {
            OnTurnChanged?.Invoke(null, new TurnEvent(state));
        }

        public static void RaiseInventoryChange(InventoryEventType type, Item item)
        {
            OnInventoryChange?.Invoke(null, new InventoryChangeEvent(type, item));
        }

        public static void RaiseSurroundingsChange((int X, int Y) position, int fovRadius, SurroundingsChangeType type)
        {
            OnSurroundingsChange?.Invoke(null, new SurroundingsChangeEvent(position, fovRadius, type));
        }
    }
}
