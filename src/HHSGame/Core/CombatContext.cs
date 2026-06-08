using HHSGame.Core.Combat;
using HHSGame.Core.Enemies;

namespace HHSGame.Core
{
    /// <summary>
    /// 战斗相关服务的分组上下文。
    /// </summary>
    public sealed class CombatContext
    {
        public EnemyManager EnemyManager { get; }
        public TurnManager TurnManager { get; }
        public EnemyFactory EnemyFactory { get; }

        public CombatContext(EnemyManager enemyManager, TurnManager turnManager, EnemyFactory enemyFactory)
        {
            EnemyManager = enemyManager;
            TurnManager = turnManager;
            EnemyFactory = enemyFactory;
        }
    }
}
