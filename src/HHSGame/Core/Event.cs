using HHSGame.Core.Items;

namespace HHSGame.Core
{
    public class GameMessageEventArgs(string message) : EventArgs
    {
        public string Message { get; } = message;

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

    public class TurnEventArgs(TurnState state) : EventArgs
    {
        public TurnState State { get; } = state;
    }

    public enum InventoryEventType
    {
        PickUp,
        Drop,
        Use,
        Remove,
    }

    public class InventoryChangeEventArgs(InventoryEventType type, Item item) : EventArgs
    {
        public InventoryEventType EventType { get; } = type;
        public Item Item { get; } = item;
    }

    public enum SurroundingsChangeType
    {
        Movement,
        DropLoot,
        PickUpLoot,
        DropItem,
    }

    public class SurroundingsChangeEventArgs((int X, int Y) position, int fovRadius, SurroundingsChangeType type) : EventArgs
    {
        public (int X, int Y) Position { get; } = position;
        public int FOVRadius { get; } = fovRadius;
        public SurroundingsChangeType Type { get; } = type;
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
