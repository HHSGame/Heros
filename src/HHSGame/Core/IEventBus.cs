using HHSGame.Core.Items;

namespace HHSGame.Core
{
    /// <summary>
    /// 可注入的事件总线接口，替代静态 Events 类。
    /// </summary>
    public interface IEventBus
    {
        event EventHandler<GameMessageEventArgs>? OnGameMessage;
        event EventHandler<TurnEventArgs>? OnTurnChanged;
        event EventHandler<InventoryChangeEventArgs>? OnInventoryChange;
        event EventHandler<SurroundingsChangeEventArgs>? OnSurroundingsChange;
        event EventHandler? OnActionSequenceChanged;

        void RaiseGameMessage(string message);
        void RaiseTurnChanged(TurnState state);
        void RaiseInventoryChange(InventoryEventType type, Item item);
        void RaiseSurroundingsChange((int X, int Y) position, int fovRadius, SurroundingsChangeType type);
        void RaiseActionSequenceChanged();
    }
}
