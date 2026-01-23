using System.IO;
using HHSGame.Core;
using HHSGame.Core.Classes;
using HHSGame.Core.Dialogue;
using HHSGame.Core.Engine.Catalogs;
using HHSGame.Core.Engine.Config;
using HHSGame.Core.Map;
using HHSGame.Core.Npcs;
using HHSGame.Core.Quests;
using HHSGame.Core.Stats;
using HHSGame.UI;

namespace HHSGame.Core.Engine
{
    public sealed class GameConfigMapper(GameCatalog catalogs)
    {
        private readonly ClassCatalog classCatalog = catalogs.ClassCatalog;
        private readonly EnemyCatalog enemyCatalog = catalogs.EnemyCatalog;
        private readonly Items.ItemCatalog itemCatalog = catalogs.ItemCatalog;

        public GameParameters ToParameters(GameConfig config)
        {
            MapStyle style = MapStyle.Cave;
            if (!string.IsNullOrWhiteSpace(config.Map.Style)
                && Enum.TryParse(config.Map.Style, true, out MapStyle parsedStyle))
            {
                style = parsedStyle;
            }

            GameParameters parameters = new()
            {
                MapStyle = style,
                MapWidth = config.Map.Width,
                MapHeight = config.Map.Height,
                UseCustomMap = config.Map.UseCustomMap,
                CustomMapPath = config.Map.CustomMapPath,
                PlayerClass = classCatalog.Resolve(config.Player.Class),
                PlayerAttributes = config.Player.Attributes,
                PlayerSkills = config.Player.Skills.Count == 0 ? null : SkillMapper.BuildSkills(config.Player.Skills),
                StartingItems = ValidateStartingItems(config.Player.StartingItems),
                PlayerSpawns = BuildPlayerSpawns(config.Players),
                EnemySpawns = BuildEnemySpawns(config.Enemies),
                MapItems = BuildMapItems(config.Items),
                NpcSpawns = BuildNpcSpawns(config.Npcs),
                DialogueDefinitions = BuildDialogues(config.Dialogues),
                QuestDefinitions = BuildQuestDefinitions(config.Quests),
                AchievementDefinitions = BuildAchievementDefinitions(config.Achievements)
            };

            if (config.Player.StartPosition != null)
            {
                parameters.PlayerStartPosition = new Coordinate(
                    config.Player.StartPosition.X,
                    config.Player.StartPosition.Y);
            }

            if (parameters.NpcSpawns.Count > 0)
            {
                HashSet<string> dialogueIds = parameters.DialogueDefinitions
                    .Select(def => def.Id)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (NpcSpawn npc in parameters.NpcSpawns)
                {
                    if (!dialogueIds.Contains(npc.DialogueId))
                    {
                        throw new InvalidDataException($"Npc '{npc.Id}' references missing dialogue '{npc.DialogueId}'.");
                    }
                }
            }

            return parameters;
        }

        private List<EnemySpawn> BuildEnemySpawns(List<EnemySpawnConfig>? spawns)
        {
            List<EnemySpawn> result = [];
            if (spawns == null || spawns.Count == 0)
            {
                return result;
            }

            foreach (EnemySpawnConfig spawn in spawns)
            {
                string id = RequireId(spawn.Id, "enemy");
                if (!enemyCatalog.TryGetDefinition(id, out _))
                {
                    throw new InvalidDataException($"Unknown enemy id '{id}'.");
                }

                if (spawn.Position == null)
                {
                    throw new InvalidDataException($"Enemy position is required for {id}.");
                }

                result.Add(new EnemySpawn(
                    id,
                    new Coordinate(spawn.Position.X, spawn.Position.Y),
                    spawn.Count));
            }

            return result;
        }

        private List<MapItemSpawn> BuildMapItems(List<ItemConfig>? items)
        {
            List<MapItemSpawn> result = [];
            if (items == null || items.Count == 0)
            {
                return result;
            }

            foreach (ItemConfig item in items)
            {
                string id = RequireId(item.Id, "item");
                if (item.Position == null)
                {
                    throw new InvalidDataException($"Item position is required for {id}.");
                }

                if (!itemCatalog.TryCreateItem(id, out _))
                {
                    throw new InvalidDataException($"Unknown item id '{id}'.");
                }

                result.Add(new MapItemSpawn(
                    id,
                    new Coordinate(item.Position.X, item.Position.Y),
                    item.Quantity));
            }

            return result;
        }

        private List<PlayerSpawn> BuildPlayerSpawns(List<PlayerEntryConfig>? players)
        {
            List<PlayerSpawn> result = [];
            if (players == null || players.Count == 0)
            {
                return result;
            }

            for (int i = 0; i < players.Count; i++)
            {
                PlayerEntryConfig entry = players[i];
                if (entry.StartPosition == null)
                {
                    throw new InvalidDataException("Player start position is required.");
                }

                string name = string.IsNullOrWhiteSpace(entry.Name)
                    ? $"Player {i + 1}"
                    : entry.Name.Trim();

                char glyph = string.IsNullOrWhiteSpace(entry.Glyph) ? '\0' : entry.Glyph.Trim()[0];

                ClassConfig classConfig = ResolvePlayerClass(entry.Class, name);

                List<string> startingItems = ValidateStartingItems(entry.StartingItems);
                Skills? skills = entry.Skills.Count == 0 ? null : SkillMapper.BuildSkills(entry.Skills);

                result.Add(new PlayerSpawn(
                    name,
                    glyph,
                    classConfig,
                    entry.Attributes,
                    skills,
                    new Coordinate(entry.StartPosition.X, entry.StartPosition.Y),
                    startingItems));
            }

            return result;
        }

        private List<NpcSpawn> BuildNpcSpawns(List<NpcConfig>? npcs)
        {
            List<NpcSpawn> result = [];
            if (npcs == null || npcs.Count == 0)
            {
                return result;
            }

            foreach (NpcConfig npc in npcs)
            {
                string id = RequireId(npc.Id, "npc");
                if (npc.Position == null)
                {
                    throw new InvalidDataException($"Npc position is required for {id}.");
                }

                string name = string.IsNullOrWhiteSpace(npc.Name) ? id : npc.Name.Trim();
                char glyph = string.IsNullOrWhiteSpace(npc.Glyph) ? 'N' : npc.Glyph.Trim()[0];
                Attributes attributes = npc.Attributes ?? new Attributes
                {
                    Strength = 5,
                    Perception = 5,
                    Agility = 5,
                    Charisma = 5,
                    Intelligence = 5
                };
                Skills skills = npc.Skills.Count == 0 ? new Skills() : SkillMapper.BuildSkills(npc.Skills);
                string dialogueId = RequireId(npc.DialogueId, $"npc {id} dialogue");
                List<string> startingItems = ValidateStartingItems(npc.StartingItems);

                result.Add(new NpcSpawn(
                    id,
                    name,
                    glyph,
                    ResolveNpcColor(npc.ColorKey),
                    attributes,
                    skills,
                    new Coordinate(npc.Position.X, npc.Position.Y),
                    dialogueId,
                    startingItems));
            }

            return result;
        }

        private static List<DialogueDefinition> BuildDialogues(List<DialogueConfig>? dialogues)
        {
            List<DialogueDefinition> result = [];
            if (dialogues == null || dialogues.Count == 0)
            {
                return result;
            }

            foreach (DialogueConfig config in dialogues)
            {
                string id = RequireId(config.Id, "dialogue");
                if (config.Nodes.Count == 0)
                {
                    throw new InvalidDataException($"Dialogue '{id}' has no nodes.");
                }

                Dictionary<string, DialogueNode> nodes = new(StringComparer.OrdinalIgnoreCase);
                foreach (DialogueNodeConfig node in config.Nodes)
                {
                    string nodeId = RequireId(node.Id, $"dialogue {id} node");
                    string text = string.IsNullOrWhiteSpace(node.Text) ? string.Empty : node.Text.Trim();
                    List<DialogueOption> options = [];
                    foreach (DialogueOptionConfig option in node.Options)
                    {
                        string optionText = RequireId(option.Text, $"dialogue {id} option");
                        string? nextNodeId = string.IsNullOrWhiteSpace(option.NextNodeId) ? null : option.NextNodeId.Trim();
                        List<DialogueRequirement> requirements = ParseDialogueRequirements(option.Requirements, id);
                        List<DialogueEffect> effects = ParseDialogueEffects(option.Effects, id);
                        options.Add(new DialogueOption(optionText, nextNodeId, requirements, effects));
                    }

                    nodes[nodeId] = new DialogueNode(nodeId, text, options);
                }

                string startNodeId = string.IsNullOrWhiteSpace(config.StartNodeId)
                    ? nodes.Keys.First()
                    : config.StartNodeId.Trim();

                if (!nodes.ContainsKey(startNodeId))
                {
                    throw new InvalidDataException($"Dialogue '{id}' start node '{startNodeId}' is missing.");
                }

                result.Add(new DialogueDefinition(id, startNodeId, nodes));
            }

            return result;
        }

        private static List<DialogueRequirement> ParseDialogueRequirements(List<DialogueRequirementConfig>? requirements, string dialogueId)
        {
            List<DialogueRequirement> result = [];
            if (requirements == null || requirements.Count == 0)
            {
                return result;
            }

            foreach (DialogueRequirementConfig requirement in requirements)
            {
                string typeValue = RequireId(requirement.Type, $"dialogue {dialogueId} requirement type");
                if (!Enum.TryParse(typeValue, true, out DialogueRequirementType reqType))
                {
                    throw new InvalidDataException($"Unknown dialogue requirement type '{typeValue}' in {dialogueId}.");
                }

                string idValue = RequireId(requirement.Id, $"dialogue {dialogueId} requirement id");
                switch (reqType)
                {
                    case DialogueRequirementType.Skill:
                        if (!Enum.TryParse(idValue, true, out SkillType skill))
                        {
                            throw new InvalidDataException($"Unknown skill '{idValue}' in {dialogueId}.");
                        }
                        result.Add(new DialogueRequirement(reqType, skill, null, null, null, requirement.Min));
                        break;
                    case DialogueRequirementType.Attribute:
                        if (!Enum.TryParse(idValue, true, out AttributeType attribute))
                        {
                            throw new InvalidDataException($"Unknown attribute '{idValue}' in {dialogueId}.");
                        }
                        result.Add(new DialogueRequirement(reqType, null, attribute, null, null, requirement.Min));
                        break;
                    case DialogueRequirementType.QuestStatus:
                        if (string.IsNullOrWhiteSpace(requirement.Status)
                            || !Enum.TryParse(requirement.Status.Trim(), true, out QuestStatus status))
                        {
                            throw new InvalidDataException($"Unknown quest status '{requirement.Status}' in {dialogueId}.");
                        }
                        result.Add(new DialogueRequirement(reqType, null, null, idValue, status, 0));
                        break;
                    case DialogueRequirementType.QuestObjectiveComplete:
                        result.Add(new DialogueRequirement(reqType, null, null, idValue, null, 0));
                        break;
                    default:
                        throw new InvalidDataException($"Unknown requirement type '{typeValue}' in {dialogueId}.");
                }
            }

            return result;
        }

        private static List<DialogueEffect> ParseDialogueEffects(List<DialogueEffectConfig>? effects, string dialogueId)
        {
            List<DialogueEffect> result = [];
            if (effects == null || effects.Count == 0)
            {
                return result;
            }

            foreach (DialogueEffectConfig effect in effects)
            {
                string typeValue = RequireId(effect.Type, $"dialogue {dialogueId} effect type");
                if (!Enum.TryParse(typeValue, true, out DialogueEffectType effectType))
                {
                    throw new InvalidDataException($"Unknown dialogue effect type '{typeValue}' in {dialogueId}.");
                }

                string target = effect.Target?.Trim() ?? string.Empty;
                int amount = effect.Amount;
                switch (effectType)
                {
                    case DialogueEffectType.StartQuest:
                    case DialogueEffectType.CompleteQuest:
                    case DialogueEffectType.GiveItem:
                    case DialogueEffectType.UnlockAchievement:
                        if (string.IsNullOrWhiteSpace(target))
                        {
                            throw new InvalidDataException($"Dialogue effect '{typeValue}' in {dialogueId} requires a target.");
                        }
                        break;
                    case DialogueEffectType.AddCurrency:
                        break;
                    default:
                        break;
                }

                result.Add(new DialogueEffect(effectType, target, amount));
            }

            return result;
        }

        private static List<QuestDefinition> BuildQuestDefinitions(List<QuestConfig>? quests)
        {
            List<QuestDefinition> result = [];
            if (quests == null || quests.Count == 0)
            {
                return result;
            }

            foreach (QuestConfig quest in quests)
            {
                string id = RequireId(quest.Id, "quest");
                string name = string.IsNullOrWhiteSpace(quest.Name) ? id : quest.Name.Trim();
                string description = quest.Description?.Trim() ?? string.Empty;
                string hint = quest.Hint?.Trim() ?? string.Empty;
                List<QuestObjectiveDefinition> objectives = ParseObjectives(quest.Objectives, $"quest {id}");
                List<QuestRewardDefinition> rewards = ParseRewards(quest.Rewards, $"quest {id}");
                result.Add(new QuestDefinition(id, name, description, hint, objectives, rewards));
            }

            return result;
        }

        private static List<AchievementDefinition> BuildAchievementDefinitions(List<AchievementConfig>? achievements)
        {
            List<AchievementDefinition> result = [];
            if (achievements == null || achievements.Count == 0)
            {
                return result;
            }

            foreach (AchievementConfig achievement in achievements)
            {
                string id = RequireId(achievement.Id, "achievement");
                string name = string.IsNullOrWhiteSpace(achievement.Name) ? id : achievement.Name.Trim();
                string description = achievement.Description?.Trim() ?? string.Empty;
                List<QuestObjectiveDefinition> objectives = ParseObjectives(achievement.Objectives, $"achievement {id}");
                result.Add(new AchievementDefinition(id, name, description, objectives));
            }

            return result;
        }

        private static List<QuestObjectiveDefinition> ParseObjectives(List<QuestObjectiveConfig>? objectives, string context)
        {
            List<QuestObjectiveDefinition> result = [];
            if (objectives == null || objectives.Count == 0)
            {
                return result;
            }

            foreach (QuestObjectiveConfig objective in objectives)
            {
                string typeValue = RequireId(objective.Type, $"{context} objective type");
                if (!Enum.TryParse(typeValue, true, out QuestObjectiveKind kind))
                {
                    throw new InvalidDataException($"Unknown objective type '{typeValue}' in {context}.");
                }

                string target = RequireId(objective.Target, $"{context} objective target");
                int count = objective.Count <= 0 ? 1 : objective.Count;
                result.Add(new QuestObjectiveDefinition(kind, target, count));
            }

            return result;
        }

        private static List<QuestRewardDefinition> ParseRewards(List<QuestRewardConfig>? rewards, string context)
        {
            List<QuestRewardDefinition> result = [];
            if (rewards == null || rewards.Count == 0)
            {
                return result;
            }

            foreach (QuestRewardConfig reward in rewards)
            {
                string typeValue = RequireId(reward.Type, $"{context} reward type");
                if (!Enum.TryParse(typeValue, true, out QuestRewardKind kind))
                {
                    throw new InvalidDataException($"Unknown reward type '{typeValue}' in {context}.");
                }

                string target = reward.Target?.Trim() ?? string.Empty;
                if (kind == QuestRewardKind.Item && string.IsNullOrWhiteSpace(target))
                {
                    throw new InvalidDataException($"{context} reward item requires a target id.");
                }

                result.Add(new QuestRewardDefinition(kind, target, reward.Amount));
            }

            return result;
        }

        private List<string> ValidateStartingItems(List<string>? items)
        {
            List<string> result = [];
            if (items == null || items.Count == 0)
            {
                return result;
            }

            foreach (string itemId in items)
            {
                string id = RequireId(itemId, "starting item");
                if (!itemCatalog.TryCreateItem(id, out _))
                {
                    throw new InvalidDataException($"Unknown starting item id '{id}'.");
                }

                result.Add(id);
            }

            return result;
        }

        private ClassConfig ResolvePlayerClass(string? classId, string playerName)
        {
            if (string.IsNullOrWhiteSpace(classId))
            {
                return classCatalog.GetDefault();
            }

            if (!classCatalog.TryResolve(classId, out ClassConfig config))
            {
                throw new InvalidDataException($"Unknown class id '{classId}' for {playerName}.");
            }

            return config;
        }

        private static string RequireId(string value, string context)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidDataException($"Missing {context} id.");
            }

            return value.Trim();
        }

        private static Terminal.Gui.Drawing.Attribute ResolveNpcColor(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return ColorPresets.Npcs.Default;
            }

            return key.Trim().ToLowerInvariant() switch
            {
                "default" => ColorPresets.Npcs.Default,
                "trader" => ColorPresets.Npcs.Trader,
                "scholar" => ColorPresets.Npcs.Scholar,
                "guard" => ColorPresets.Npcs.Guard,
                _ => ColorPresets.Npcs.Default
            };
        }

    }
}
