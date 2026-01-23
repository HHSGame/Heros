using HHSGame.Core;
using HHSGame.Core.Quests;
using HHSGame.Core.Stats;

namespace HHSGame.Core.Dialogue
{
    public sealed class DialogueSession
    {
        public DialogueSession(Npc npc, DialogueDefinition definition, DialogueNode node, IReadOnlyList<DialogueOption> options, bool isRepeatVisit)
        {
            Npc = npc;
            Definition = definition;
            Node = node;
            Options = options;
            IsRepeatVisit = isRepeatVisit;
        }

        public Npc Npc { get; }
        public DialogueDefinition Definition { get; }
        public DialogueNode Node { get; private set; }
        public IReadOnlyList<DialogueOption> Options { get; private set; }
        public bool IsRepeatVisit { get; private set; }

        public void UpdateNode(DialogueNode node, IReadOnlyList<DialogueOption> options, bool isRepeatVisit)
        {
            Node = node;
            Options = options;
            IsRepeatVisit = isRepeatVisit;
        }
    }

    public sealed class DialogueManager
    {
        private readonly PartyState partyState;
        private readonly QuestManager questManager;
        private readonly Dictionary<string, DialogueDefinition> definitions = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, HashSet<string>> visitedNodes = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, HashSet<string>> visitedOptions = new(StringComparer.OrdinalIgnoreCase);

        public DialogueManager(PartyState partyState, QuestManager questManager)
        {
            this.partyState = partyState;
            this.questManager = questManager;
        }

        public DialogueSession? CurrentSession { get; private set; }

        public event EventHandler? SessionChanged;

        public void LoadDefinitions(IEnumerable<DialogueDefinition> dialogueDefinitions)
        {
            definitions.Clear();
            foreach (DialogueDefinition definition in dialogueDefinitions)
            {
                definitions[definition.Id] = definition;
            }
        }

        public bool TryStartDialogue(Npc npc)
        {
            if (!definitions.TryGetValue(npc.DialogueId, out DialogueDefinition? definition))
            {
                Events.RaiseGameMessage("No dialogue available.");
                return false;
            }

            if (!definition.Nodes.TryGetValue(definition.StartNodeId, out DialogueNode? node))
            {
                Events.RaiseGameMessage("Dialogue start node missing.");
                return false;
            }

            Player? player = partyState.ActivePlayer;
            if (player == null)
            {
                return false;
            }

            bool isRepeatVisit = MarkNodeVisited(npc.Id, node.Id);
            IReadOnlyList<DialogueOption> options = GetAvailableOptions(player, node);
            CurrentSession = new DialogueSession(npc, definition, node, options, isRepeatVisit);
            questManager.NotifyNpcTalked(npc.Id);
            SessionChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public void EndDialogue()
        {
            if (CurrentSession == null)
            {
                return;
            }

            CurrentSession = null;
            SessionChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool TrySelectOption(int index)
        {
            DialogueSession? session = CurrentSession;
            if (session == null)
            {
                return false;
            }

            if (index < 0 || index >= session.Options.Count)
            {
                return false;
            }

            DialogueOption option = session.Options[index];
            MarkOptionVisited(session.Npc.Id, session.Node.Id, option.Text);
            ApplyEffects(option);

            if (!string.IsNullOrWhiteSpace(option.NextNodeId)
                && session.Definition.Nodes.TryGetValue(option.NextNodeId.Trim(), out DialogueNode? nextNode))
            {
                Player? player = partyState.ActivePlayer;
                if (player == null)
                {
                    return false;
                }

                bool isRepeatVisit = MarkNodeVisited(session.Npc.Id, nextNode.Id);
                IReadOnlyList<DialogueOption> options = GetAvailableOptions(player, nextNode);
                session.UpdateNode(nextNode, options, isRepeatVisit);
                SessionChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }

            EndDialogue();
            return true;
        }

        private List<DialogueOption> GetAvailableOptions(Player player, DialogueNode node)
        {
            List<DialogueOption> result = [];
            foreach (DialogueOption option in node.Options)
            {
                if (MeetsRequirements(player, option.Requirements)
                    && MeetsQuestEffectConstraints(option))
                {
                    result.Add(option);
                }
            }

            return result;
        }

        private bool MeetsRequirements(Player player, IReadOnlyList<DialogueRequirement> requirements)
        {
            if (requirements.Count == 0)
            {
                return true;
            }

            foreach (DialogueRequirement requirement in requirements)
            {
                if (!MeetsRequirement(player, requirement))
                {
                    return false;
                }
            }

            return true;
        }

        private bool MeetsRequirement(Player player, DialogueRequirement requirement)
        {
            return requirement.Type switch
            {
                DialogueRequirementType.Skill => requirement.Skill.HasValue
                    && GetEffectiveSkillValue(player, requirement.Skill.Value) >= requirement.Minimum,
                DialogueRequirementType.Attribute => requirement.Attribute.HasValue
                    && GetEffectiveAttributeValue(player, requirement.Attribute.Value) >= requirement.Minimum,
                DialogueRequirementType.QuestStatus => requirement.QuestStatus.HasValue
                    && !string.IsNullOrWhiteSpace(requirement.QuestId)
                    && questManager.IsQuestInStatus(requirement.QuestId, requirement.QuestStatus.Value),
                DialogueRequirementType.QuestObjectiveComplete => !string.IsNullOrWhiteSpace(requirement.QuestId)
                    && questManager.AreQuestObjectivesComplete(requirement.QuestId),
                _ => true
            };
        }

        private int GetEffectiveSkillValue(Player player, SkillType skill)
        {
            int baseValue = partyState.GetEffectiveSkillValue(player, skill);
            return baseValue + CalculateAssistBonus(player, ally =>
                partyState.GetEffectiveSkillValue(ally, skill), baseValue);
        }

        private int GetEffectiveAttributeValue(Player player, AttributeType attribute)
        {
            int baseValue = partyState.GetEffectiveAttributeValue(player, attribute);
            return baseValue + CalculateAssistBonus(player, ally =>
                partyState.GetEffectiveAttributeValue(ally, attribute), baseValue);
        }

        private int CalculateAssistBonus(Player player, Func<Player, int> valueSelector, int baseValue)
        {
            int bestBonus = 0;
            foreach (Player ally in partyState.Players)
            {
                if (ReferenceEquals(ally, player))
                {
                    continue;
                }

                if (!IsAdjacent(player.Position, ally.Position))
                {
                    continue;
                }

                int allyValue = valueSelector(ally);
                if (allyValue <= baseValue)
                {
                    continue;
                }

                int bonus = (allyValue - baseValue) / 2;
                if (bonus > bestBonus)
                {
                    bestBonus = bonus;
                }
            }

            return bestBonus;
        }

        private static bool IsAdjacent(Coordinate a, Coordinate b)
        {
            int dx = Math.Abs(a.X - b.X);
            int dy = Math.Abs(a.Y - b.Y);
            return dx <= 1 && dy <= 1;
        }

        public string BuildOptionLabel(DialogueSession session, DialogueOption option)
        {
            List<string> prefixes = [];

            if (IsOptionVisited(session.Npc.Id, session.Node.Id, option.Text))
            {
                prefixes.Add("[Seen]");
            }

            if (HasQuestEffect(option))
            {
                prefixes.Add("[!]");
            }

            foreach (DialogueRequirement requirement in option.Requirements)
            {
                switch (requirement.Type)
                {
                    case DialogueRequirementType.Skill:
                        if (requirement.Skill.HasValue)
                        {
                            prefixes.Add($"[{requirement.Skill.Value} {requirement.Minimum}]");
                        }
                        break;
                    case DialogueRequirementType.Attribute:
                        if (requirement.Attribute.HasValue)
                        {
                            prefixes.Add($"[{requirement.Attribute.Value} {requirement.Minimum}]");
                        }
                        break;
                    default:
                        break;
                }
            }

            if (prefixes.Count == 0)
            {
                return option.Text;
            }

            return string.Concat(prefixes) + " " + option.Text;
        }

        private bool MeetsQuestEffectConstraints(DialogueOption option)
        {
            foreach (DialogueEffect effect in option.Effects)
            {
                switch (effect.Type)
                {
                    case DialogueEffectType.StartQuest:
                        if (!questManager.IsQuestInStatus(effect.Target, QuestStatus.Inactive))
                        {
                            return false;
                        }
                        break;
                    case DialogueEffectType.CompleteQuest:
                        if (questManager.CanCompleteQuest(effect.Target))
                        {
                            break;
                        }

                        if (HasStartQuestEffect(option, effect.Target)
                            && questManager.IsQuestInStatus(effect.Target, QuestStatus.Inactive))
                        {
                            break;
                        }

                        return false;
                    default:
                        break;
                }
            }

            return true;
        }

        private static bool HasQuestEffect(DialogueOption option)
        {
            return option.Effects.Any(effect => effect.Type == DialogueEffectType.StartQuest
                || effect.Type == DialogueEffectType.CompleteQuest);
        }

        private static bool HasStartQuestEffect(DialogueOption option, string questId)
        {
            return option.Effects.Any(effect => effect.Type == DialogueEffectType.StartQuest
                && string.Equals(effect.Target, questId, StringComparison.OrdinalIgnoreCase));
        }

        private bool MarkNodeVisited(string npcId, string nodeId)
        {
            if (!visitedNodes.TryGetValue(npcId, out HashSet<string>? nodes))
            {
                nodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                visitedNodes[npcId] = nodes;
            }

            if (nodes.Contains(nodeId))
            {
                return true;
            }

            nodes.Add(nodeId);
            return false;
        }

        private void MarkOptionVisited(string npcId, string nodeId, string optionText)
        {
            if (!visitedOptions.TryGetValue(npcId, out HashSet<string>? options))
            {
                options = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                visitedOptions[npcId] = options;
            }

            options.Add(BuildOptionKey(nodeId, optionText));
        }

        private bool IsOptionVisited(string npcId, string nodeId, string optionText)
        {
            if (!visitedOptions.TryGetValue(npcId, out HashSet<string>? options))
            {
                return false;
            }

            return options.Contains(BuildOptionKey(nodeId, optionText));
        }

        private static string BuildOptionKey(string nodeId, string optionText)
        {
            return $"{nodeId}:{optionText}";
        }

        private void ApplyEffects(DialogueOption option)
        {
            foreach (DialogueEffect effect in option.Effects)
            {
                switch (effect.Type)
                {
                    case DialogueEffectType.StartQuest:
                        questManager.StartQuest(effect.Target);
                        break;
                    case DialogueEffectType.CompleteQuest:
                        questManager.CompleteQuest(effect.Target);
                        break;
                    case DialogueEffectType.GiveItem:
                        questManager.GiveItemReward(effect.Target, effect.Amount <= 0 ? 1 : effect.Amount);
                        break;
                    case DialogueEffectType.AddCurrency:
                        questManager.GiveCurrencyReward(effect.Amount);
                        break;
                    case DialogueEffectType.UnlockAchievement:
                        questManager.UnlockAchievement(effect.Target);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
