namespace HHSGame.Core
{
    /// <summary>
    /// 游戏机制常量，避免魔法数字散布在代码中。
    /// </summary>
    public static class GameConstants
    {
        // 战斗系统
        public static class Combat
        {
            /// <summary>d20 骰子最小值</summary>
            public const int DiceMin = 1;
            /// <summary>d20 骰子最大值</summary>
            public const int DiceMax = 20;
            /// <summary>暴击成功阈值（掷出1）</summary>
            public const int CriticalSuccessThreshold = 1;
            /// <summary>暴击失败阈值（掷出20）</summary>
            public const int CriticalFailureThreshold = 20;
            /// <summary>暴击伤害倍率</summary>
            public const double CriticalDamageMultiplier = 1.5;
            /// <summary>最低伤害值</summary>
            public const int MinDamage = 1;
        }

        // 角色成长
        public static class Progression
        {
            /// <summary>经验值公式基数</summary>
            public const int ExperienceBase = 100;
            /// <summary>经验值公式指数</summary>
            public const double ExperienceExponent = 1.5;
            /// <summary>每级基础技能点</summary>
            public const int BaseSkillPointsPerLevel = 10;
            /// <summary>智力对技能点的除数</summary>
            public const int IntelligenceDivisor = 2;
            /// <summary>软技能上限乘数</summary>
            public const int SoftCapLevelMultiplier = 5;
            /// <summary>软技能上限基数</summary>
            public const int SoftCapBase = 20;
            /// <summary>硬技能上限</summary>
            public const int HardSkillCap = 100;
        }

        // 属性系统
        public static class Attributes
        {
            /// <summary>负重公式乘数</summary>
            public const int CarryCapacityMultiplier = 10;
        }

        // 敌人 AI
        public static class EnemyAI
        {
            /// <summary>攻击范围（格子数的平方）</summary>
            public const int AttackRangeSquared = 1;
            /// <summary>检测范围（格子数的平方，8格）</summary>
            public const int DetectionRangeSquared = 64;
        }

        // 声望系统
        public static class Reputation
        {
            /// <summary>声望最小值</summary>
            public const int MinReputation = -100;
            /// <summary>声望最大值</summary>
            public const int MaxReputation = 100;
            /// <summary>伪装暴露概率基数</summary>
            public const int DisguiseBlowChanceBase = 100;
        }

        // 技能系统
        public static class Skills
        {
            /// <summary>开锁难度（简单）</summary>
            public const int LockpickDifficultyEasy = 18;
            /// <summary>开锁难度（困难）</summary>
            public const int LockpickDifficultyHard = 25;
        }

        // 引擎设置
        public static class Engine
        {
            /// <summary>脚本步骤延迟（毫秒）</summary>
            public const int ScriptStepDelayMs = 100;
        }

        // 地图尺寸（旧常量，保留兼容性）
        public static class Map
        {
            public static readonly int Width = 800;
            public static readonly int Height = 600;
        }
    }
}
