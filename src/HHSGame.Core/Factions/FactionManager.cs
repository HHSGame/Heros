namespace HHSGame.Core.Factions
{
    /// <summary>
    /// 阵营枚举
    /// </summary>
    public enum Faction
    {
        /// <summary>中立/未归属</summary>
        Neutral,
        /// <summary>占领军（德国占领军）</summary>
        Occupier,
        /// <summary>傀儡政权（维希法国）</summary>
        Puppet,
        /// <summary>反抗势力（法国抵抗运动）</summary>
        Resistance,
        /// <summary>盟军</summary>
        Allies,
        /// <summary>匪徒</summary>
        Bandits,
        /// <summary>平民</summary>
        Civilians,
        /// <summary>宗教/中立组织</summary>
        Church
    }

    /// <summary>
    /// 阵营间关系类型
    /// </summary>
    public enum FactionRelation
    {
        Allied,     // 盟友 — 不会互相攻击
        Neutral,    // 中立 — 不主动攻击，但也不会帮忙
        Hostile     // 敌对 — 会主动攻击
    }

    /// <summary>
    /// 管理阵营间关系和玩家阵营状态
    /// </summary>
    public sealed class FactionManager
    {
        // 玩家当前阵营
        public Faction PlayerFaction { get; set; } = Faction.Neutral;

        // 伪装状态
        public Faction? DisguisedAs { get; set; }
        public bool IsDisguised => DisguisedAs.HasValue;

        // 阵营声望 (-100 到 100)
        private readonly Dictionary<Faction, int> reputation = new()
        {
            { Faction.Occupier, 0 },
            { Faction.Puppet, 0 },
            { Faction.Resistance, 0 },
            { Faction.Allies, 0 },
            { Faction.Bandits, 0 },
            { Faction.Civilians, 0 },
            { Faction.Church, 0 }
        };

        // 阵营关系矩阵
        private readonly Dictionary<(Faction, Faction), FactionRelation> relations = new();

        public FactionManager()
        {
            InitializeRelations();
        }

        public IReadOnlyDictionary<Faction, int> Reputation => reputation;

        /// <summary>
        /// 获取两个阵营间的关系
        /// </summary>
        public FactionRelation GetRelation(Faction a, Faction b)
        {
            if (a == b) return FactionRelation.Allied;
            if (a == Faction.Neutral || b == Faction.Neutral) return FactionRelation.Neutral;

            return relations.TryGetValue((a, b), out FactionRelation rel) ? rel
                : relations.TryGetValue((b, a), out rel) ? rel
                : FactionRelation.Neutral;
        }

        /// <summary>
        /// 判断一个阵营是否会攻击另一个阵营
        /// </summary>
        public bool WillAttack(Faction attacker, Faction defender)
        {
            // 如果 defender 有伪装，用伪装的阵营判断
            return GetRelation(attacker, defender) == FactionRelation.Hostile;
        }

        /// <summary>
        /// 判断敌方阵营是否会攻击玩家（考虑伪装）
        /// </summary>
        public bool WillAttackPlayer(Faction enemyFaction)
        {
            Faction playerEffectiveFaction = GetEffectivePlayerFaction();
            return WillAttack(enemyFaction, playerEffectiveFaction);
        }

        /// <summary>
        /// 获取玩家有效阵营（考虑伪装）
        /// </summary>
        public Faction GetEffectivePlayerFaction()
        {
            return IsDisguised ? DisguisedAs!.Value : PlayerFaction;
        }

        /// <summary>
        /// 尝试伪装成某个阵营。需要足够的声望才能成功伪装。
        /// </summary>
        public bool TryDisguise(Faction targetFaction, int requiredReputation = 20)
        {
            if (targetFaction == Faction.Neutral)
            {
                DisguisedAs = null;
                return true;
            }

            if (GetReputation(targetFaction) >= requiredReputation)
            {
                DisguisedAs = targetFaction;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 脱下伪装
        /// </summary>
        public void RemoveDisguise()
        {
            DisguisedAs = null;
        }

        /// <summary>
        /// 修改声望
        /// </summary>
        public void ModifyReputation(Faction faction, int amount)
        {
            if (reputation.ContainsKey(faction))
            {
                reputation[faction] = Math.Clamp(reputation[faction] + amount, GameConstants.Reputation.MinReputation, GameConstants.Reputation.MaxReputation);
            }
        }

        /// <summary>
        /// 获取声望
        /// </summary>
        public int GetReputation(Faction faction)
        {
            return reputation.TryGetValue(faction, out int rep) ? rep : 0;
        }

        /// <summary>
        /// 设置两个阵营间的关系
        /// </summary>
        public void SetRelation(Faction a, Faction b, FactionRelation relation)
        {
            relations[(a, b)] = relation;
            relations[(b, a)] = relation;
        }

        /// <summary>
        /// 检查伪装是否会被识破。感知越高的敌人越容易识破。
        /// </summary>
        public bool CheckDisguiseBlow(int enemyPerception, Random random)
        {
            if (!IsDisguised) return false;

            // 声望越高越不容易被识破
            int rep = GetReputation(DisguisedAs!.Value);
            int blowChance = Math.Max(5, enemyPerception * 5 - rep);
            return random.Next(GameConstants.Reputation.DisguiseBlowChanceBase) < blowChance;
        }

        /// <summary>
        /// 从阵营 ID 字符串解析
        /// </summary>
        public static Faction ParseFaction(string? factionId)
        {
            return factionId?.ToLowerInvariant() switch
            {
                "occupier" => Faction.Occupier,
                "puppet" => Faction.Puppet,
                "resistance" => Faction.Resistance,
                "allies" => Faction.Allies,
                "bandits" => Faction.Bandits,
                "civilians" => Faction.Civilians,
                "church" => Faction.Church,
                _ => Faction.Neutral
            };
        }

        private void InitializeRelations()
        {
            // 默认所有阵营关系为 Neutral（中立互不攻击）
            // 只有匪徒默认敌对平民（劫掠），其他敌对关系需要通过玩家行为触发

            // 匪徒 — 对平民敌对（劫掠为生）
            relations[(Faction.Bandits, Faction.Civilians)] = FactionRelation.Hostile;
        }
    }
}