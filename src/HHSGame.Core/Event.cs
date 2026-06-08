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

    /// <summary>
    /// 静态事件类，向后兼容。
    /// 新代码应使用 IEventBus 接口。
    /// </summary>
    public static class Events
    {
        private static IEventBus? eventBus;

        public static void Initialize(IEventBus bus)
        {
            eventBus = bus;
        }

        public static event EventHandler<GameMessageEventArgs>? OnGameMessageEvent
        {
            add { if (eventBus != null) eventBus.OnGameMessage += value; }
            remove { if (eventBus != null) eventBus.OnGameMessage -= value; }
        }

        public static event EventHandler<TurnEventArgs>? OnTurnChanged
        {
            add { if (eventBus != null) eventBus.OnTurnChanged += value; }
            remove { if (eventBus != null) eventBus.OnTurnChanged -= value; }
        }

        public static event EventHandler<InventoryChangeEventArgs>? OnInventoryChange
        {
            add { if (eventBus != null) eventBus.OnInventoryChange += value; }
            remove { if (eventBus != null) eventBus.OnInventoryChange -= value; }
        }

        public static event EventHandler<SurroundingsChangeEventArgs>? OnSurroundingsChange
        {
            add { if (eventBus != null) eventBus.OnSurroundingsChange += value; }
            remove { if (eventBus != null) eventBus.OnSurroundingsChange -= value; }
        }

        public static event EventHandler? OnActionSequenceChanged
        {
            add { if (eventBus != null) eventBus.OnActionSequenceChanged += value; }
            remove { if (eventBus != null) eventBus.OnActionSequenceChanged -= value; }
        }

        public static void RaiseGameMessage(string message)
        {
            eventBus?.RaiseGameMessage(message);
        }

        public static void RaiseTurnChanged(TurnState state)
        {
            eventBus?.RaiseTurnChanged(state);
        }

        public static void RaiseInventoryChange(InventoryEventType type, Item item)
        {
            eventBus?.RaiseInventoryChange(type, item);
        }

        public static void RaiseSurroundingsChange((int X, int Y) position, int fovRadius, SurroundingsChangeType type)
        {
            eventBus?.RaiseSurroundingsChange(position, fovRadius, type);
        }

        public static void RaiseActionSequenceChanged()
        {
            eventBus?.RaiseActionSequenceChanged();
        }
    }
}
