namespace HHSGame.Core
{
    using Items;
    public class GameMessageEventArgs : EventArgs
    {
        public string Message { get; }

        public GameMessageEventArgs(string message)
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

    public class TurnEventArgs : EventArgs
    {
        public TurnState State { get; }

        public TurnEventArgs(TurnState state)
        {
            State = state;
        }
    }

    public enum InventoryEventType
    {
        PickUp,
        Drop,
        Use,
        Remove,
    }

    public class InventoryChangeEventArgs : EventArgs
    {
        InventoryEventType EventType { get; }
        public Item Item { get; }

        public InventoryChangeEventArgs(InventoryEventType type, Item item)
        {
            EventType = type;
            Item = item;
        }
    }

    public enum SurroundingsChangeType
    {
        Movement,
        DropLoot,
        PickUpLoot,
        DropItem,
    }

    public class SurroundingsChangeEventArgs : EventArgs
    {
        public (int X, int Y) Position { get; }
        public int FOVRadius { get; }

        public SurroundingsChangeEventArgs((int X, int Y) position, int fovRadius, SurroundingsChangeType type)
        {
            Position = position;
            FOVRadius = fovRadius;
        }
    }

    public static class EventSystem
    {

        public static event EventHandler<GameMessageEventArgs>? OnGameMessageEvent;
        public static event EventHandler<TurnEventArgs>? OnTurnChanged;
        public static event EventHandler<InventoryChangeEventArgs>? OnInventoryChange;

        public static event EventHandler<SurroundingsChangeEventArgs>? OnSurroundingsChange;

        public static void RaiseGameMessage(string message)
        {
            OnGameMessageEvent?.Invoke(null, new GameMessageEventArgs(message));
        }

        public static void RaiseTurnChanged(TurnState state)
        {
            OnTurnChanged?.Invoke(null, new TurnEventArgs(state));
        }

        public static void RaiseInventoryChange(InventoryEventType type, Item item)
        {
            OnInventoryChange?.Invoke(null, new InventoryChangeEventArgs(type, item));
        }

        public static void RaiseSurroundingsChange((int X, int Y) position, int fovRadius, SurroundingsChangeType type)
        {
            OnSurroundingsChange?.Invoke(null, new SurroundingsChangeEventArgs(position, fovRadius, type));
        }
    }
}
