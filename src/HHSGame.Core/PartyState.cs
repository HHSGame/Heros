namespace HHSGame.Core
{
    public sealed class PartyState
    {
        private IReadOnlyList<Player> players = Array.Empty<Player>();

        public IReadOnlyList<Player> Players => players;
        public Player? ActivePlayer { get; private set; }
        public int ActivePlayerIndex { get; private set; } = -1;
        public int SkillCheckBonus { get; private set; }
        public int AttributeCheckBonus { get; private set; }
        public Player? InspireSource { get; private set; }
        public int InspirePenalty { get; private set; }

        public bool InspireActive => SkillCheckBonus > 0 || AttributeCheckBonus > 0;

        public void SetPlayers(IReadOnlyList<Player> players, Player activePlayer)
        {
            this.players = players;
            SetActivePlayer(activePlayer);
        }

        public void SetActivePlayer(Player activePlayer)
        {
            ActivePlayer = activePlayer;
            ActivePlayerIndex = FindPlayerIndex(activePlayer);
        }

        private int FindPlayerIndex(Player player)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (ReferenceEquals(players[i], player))
                {
                    return i;
                }
            }

            return -1;
        }

        public void ApplyInspire(Player source, int bonus, int penalty)
        {
            SkillCheckBonus = bonus;
            AttributeCheckBonus = bonus;
            InspirePenalty = penalty;
            InspireSource = source;
        }

        public int GetEffectiveSkillValue(Player player, Stats.SkillType skill)
        {
            int value = player.Stats.Skills.GetValue(skill, player.Stats.Attributes);
            if (SkillCheckBonus != 0)
            {
                value += SkillCheckBonus;
                if (ReferenceEquals(player, InspireSource))
                {
                    value -= InspirePenalty;
                }
            }

            return value;
        }

        public int GetEffectiveAttributeValue(Player player, Stats.AttributeType attribute)
        {
            int value = player.Stats.Attributes.Get(attribute);
            if (AttributeCheckBonus != 0)
            {
                value += AttributeCheckBonus;
                if (ReferenceEquals(player, InspireSource))
                {
                    value -= InspirePenalty;
                }
            }

            return value;
        }
    }
}
