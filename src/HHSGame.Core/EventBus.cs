using HHSGame.Core.Items;

namespace HHSGame.Core
{
    /// <summary>
    /// 可注入的事件总线实现。
    /// </summary>
    public sealed class EventBus : IEventBus
    {
        public event EventHandler<GameMessageEventArgs>? OnGameMessage;
        public event EventHandler<TurnEventArgs>? OnTurnChanged;
        public event EventHandler<InventoryChangeEventArgs>? OnInventoryChange;
        public event EventHandler<SurroundingsChangeEventArgs>? OnSurroundingsChange;
        public event EventHandler? OnActionSequenceChanged;

        public void RaiseGameMessage(string message)
        {
            OnGameMessage?.Invoke(this, new GameMessageEventArgs(message));
        }

        public void RaiseTurnChanged(TurnState state)
        {
            OnTurnChanged?.Invoke(this, new TurnEventArgs(state));
        }

        public void RaiseInventoryChange(InventoryEventType type, Item item)
        {
            OnInventoryChange?.Invoke(this, new InventoryChangeEventArgs(type, item));
        }

        public void RaiseSurroundingsChange((int X, int Y) position, int fovRadius, SurroundingsChangeType type)
        {
            OnSurroundingsChange?.Invoke(this, new SurroundingsChangeEventArgs(position, fovRadius, type));
        }

        public void RaiseActionSequenceChanged()
        {
            OnActionSequenceChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
