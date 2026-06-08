using HHSGame.Core.Map;
using HHSGame.Core.Interactions;

namespace HHSGame.Core
{
    /// <summary>
    /// 世界相关服务的分组上下文。
    /// </summary>
    public sealed class WorldContext
    {
        public MapState MapState { get; }
        public CollisionSystem CollisionSystem { get; }
        public SurroundingsManager SurroundingsManager { get; }
        public InteractableManager InteractableManager { get; }

        public WorldContext(MapState mapState, CollisionSystem collisionSystem, SurroundingsManager surroundingsManager, InteractableManager interactableManager)
        {
            MapState = mapState;
            CollisionSystem = collisionSystem;
            SurroundingsManager = surroundingsManager;
            InteractableManager = interactableManager;
        }
    }
}
