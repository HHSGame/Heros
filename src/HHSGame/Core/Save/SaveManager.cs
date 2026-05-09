using System.Text.Json;
using HHSGame.Core.Combat;
using HHSGame.Core.Dialogue;
using HHSGame.Core.Enemies;
using HHSGame.Core.Items;
using HHSGame.Core.Map;
using HHSGame.Core.Quests;
using HHSGame.Core.Stats;
using HHSGame.UI;
using Microsoft.Extensions.Logging;

namespace HHSGame.Core.Save
{
    public sealed class SaveManager
    {
        public static readonly string DefaultSaveDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".hhs-game", "saves");

        private const int MaxSlots = 3;
        private const string SaveFileExtension = ".json";
        private const int CurrentSaveVersion = 1;

        private readonly ILogger<SaveManager> logger;
        private readonly string saveDirectory;

        public SaveManager(ILogger<SaveManager> logger, string? saveDirectory = null)
        {
            this.logger = logger;
            this.saveDirectory = saveDirectory ?? DefaultSaveDirectory;
            Directory.CreateDirectory(this.saveDirectory);
        }

        public string SaveDirectory => saveDirectory;

        public string GetSlotPath(int slot)
        {
            return Path.Combine(saveDirectory, $"slot-{slot}{SaveFileExtension}");
        }

        public bool SlotExists(int slot)
        {
            return File.Exists(GetSlotPath(slot));
        }

        public SaveSlotInfo[] ListSlots()
        {
            SaveSlotInfo[] slots = new SaveSlotInfo[MaxSlots];
            for (int i = 0; i < MaxSlots; i++)
            {
                string path = GetSlotPath(i + 1);
                if (File.Exists(path))
                {
                    try
                    {
                        string json = File.ReadAllText(path);
                        SaveGameData? data = JsonSerializer.Deserialize<SaveGameData>(json);
                        if (data != null)
                        {
                            slots[i] = new SaveSlotInfo
                            {
                                SlotNumber = i + 1,
                                IsOccupied = true,
                                SaveTime = data.SaveTime,
                                TurnNumber = data.TurnNumber,
                                PlayerName = data.Players.Count > 0 ? data.Players[0].Name : "Unknown",
                                PlayerLevel = data.Players.Count > 0 ? data.Players[0].Level : 0,
                                MapName = data.Map.MapPath ?? "Generated"
                            };
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to read save slot {Slot}", i + 1);
                    }
                }

                slots[i] = new SaveSlotInfo
                {
                    SlotNumber = i + 1,
                    IsOccupied = false
                };
            }

            return slots;
        }

        public bool Save(Game game, int slot)
        {
            try
            {
                SaveGameData data = CaptureGameState(game);
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                string path = GetSlotPath(slot);
                File.WriteAllText(path, json);
                logger.LogInformation("Game saved to slot {Slot}", slot);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save game to slot {Slot}", slot);
                return false;
            }
        }

        public static bool CanSaveInCurrentState(GameStateType state)
        {
            return state == GameStateType.Exploration;
        }

        private static SaveGameData CaptureGameState(Game game)
        {
            GameContext ctx = game.Context;

            string playerName = game.Player?.Name ?? "Unknown";
            int turnNumber = ctx.TurnManager.GetTurnNumber();

            SaveGameData data = new()
            {
                Version = CurrentSaveVersion,
                SaveName = $"{playerName} - Turn {turnNumber}",
                SaveTime = DateTime.UtcNow,
                TurnNumber = turnNumber,
                GameState = ctx.StateMachine.CurrentState.ToString(),
                Players = CapturePlayers(game),
                Map = CaptureMap(ctx),
                Enemies = CaptureEnemies(ctx),
                Npcs = CaptureNpcs(ctx),
                GroundItems = CaptureGroundItems(ctx),
                Quests = CaptureQuests(ctx),
                Achievements = CaptureAchievements(ctx),
                Dialogue = CaptureDialogue(ctx)
            };

            return data;
        }

        private static List<PlayerSaveData> CapturePlayers(Game game)
        {
            List<PlayerSaveData> result = [];
            IReadOnlyList<Player> players = game.Context.PartyState.Players;

            foreach (Player player in players)
            {
                PlayerSaveData data = new()
                {
                    Name = player.Name,
                    Glyph = player.Glyph,
                    X = player.X,
                    Y = player.Y,
                    Level = player.Stats.Progression.Level,
                    Experience = player.Stats.Progression.Experience,
                    UnspentSkillPoints = player.Stats.Progression.UnspentSkillPoints,
                    CurrentHp = player.Stats.CurrentHp,
                    CurrentSp = player.Stats.CurrentSp,
                    Strength = player.Stats.Attributes.Strength,
                    Perception = player.Stats.Attributes.Perception,
                    Agility = player.Stats.Attributes.Agility,
                    Charisma = player.Stats.Attributes.Charisma,
                    Intelligence = player.Stats.Attributes.Intelligence,
                    Skills = CaptureSkills(player),
                    Inventory = CaptureInventory(player.Inventory),
                    EquippedWeaponId = player.EquippedWeapon.Id,
                    EquippedArmorId = player.EquippedArmor?.Id,
                    Effects = CaptureEffects(player),
                    IsActive = player == game.Player,
                    IsSneaking = player.IsSneaking
                };

                // Capture all equipped armor slots
                foreach (EquipmentSlot slot in player.Equipment.GetOccupiedSlots())
                {
                    Armor? armor = player.Equipment.GetEquipped(slot);
                    if (armor != null)
                    {
                        data.EquipmentSlots.Add(new EquipmentSlotSaveData
                        {
                            Slot = slot.ToString(),
                            ArmorId = armor.Id
                        });
                    }
                }

                result.Add(data);
            }

            return result;
        }

        private static Dictionary<string, int> CaptureSkills(Player player)
        {
            Dictionary<string, int> skills = [];
            foreach (SkillType skillType in Enum.GetValues<SkillType>())
            {
                int rank = player.Stats.Skills.GetRank(skillType);
                if (rank != 0)
                {
                    skills[skillType.ToString()] = rank;
                }
            }

            return skills;
        }

        private static List<ItemSaveData> CaptureInventory(InventoryManager inventory)
        {
            List<ItemSaveData> items = [];
            foreach (Item item in inventory.ObservableItems)
            {
                items.Add(new ItemSaveData
                {
                    Id = item.Id,
                    Name = item.Name
                });
            }

            return items;
        }

        private static List<ActiveEffectSaveData> CaptureEffects(Player player)
        {
            List<ActiveEffectSaveData> effects = [];
            foreach (ActiveEffect effect in player.ActiveEffects)
            {
                string effectType = effect switch
                {
                    PoisonEffect => "Poison",
                    StunEffect => "Stun",
                    _ => "Unknown"
                };

                effects.Add(new ActiveEffectSaveData
                {
                    EffectType = effectType,
                    Duration = effect.Duration
                });
            }

            return effects;
        }

        private static MapSaveData CaptureMap(GameContext ctx)
        {
            MapState mapState = ctx.MapState;
            List<CellSaveData> cells = [];
            List<HiddenFeatureSaveData> hiddenFeatures = [];

            for (int y = 0; y < mapState.Height; y++)
            {
                for (int x = 0; x < mapState.Width; x++)
                {
                    Cell cell = mapState.GetCell(x, y);
                    if (cell.Character != ' ')
                    {
                        cells.Add(new CellSaveData
                        {
                            X = x,
                            Y = y,
                            Character = cell.Character,
                            IsExplored = mapState.WasVisited(x, y)
                        });
                    }
                }
            }

            foreach (HiddenFeature feature in mapState.HiddenFeatures)
            {
                hiddenFeatures.Add(new HiddenFeatureSaveData
                {
                    X = feature.Position.X,
                    Y = feature.Position.Y,
                    RevealedGlyph = feature.RevealedGlyph,
                    Description = feature.Description,
                    IsRevealed = feature.Revealed
                });
            }

            List<SpecialPositionSaveData> specialPositions = mapState.SpecialPositions
                .Select(sp => new SpecialPositionSaveData
                {
                    X = sp.Position.X,
                    Y = sp.Position.Y,
                    Symbol = sp.Symbol
                })
                .ToList();

            return new MapSaveData
            {
                Width = mapState.Width,
                Height = mapState.Height,
                MapPath = ctx.Parameters.CustomMapPath,
                Cells = cells,
                HiddenFeatures = hiddenFeatures,
                SpecialPositions = specialPositions
            };
        }

        private static List<EnemySaveData> CaptureEnemies(GameContext ctx)
        {
            List<EnemySaveData> enemies = [];
            foreach (Enemy enemy in ctx.EnemyManager.Enemies)
            {
                enemies.Add(new EnemySaveData
                {
                    Name = enemy.Name,
                    Glyph = enemy.Glyph,
                    X = enemy.X,
                    Y = enemy.Y,
                    CurrentHp = enemy.Stats.CurrentHp,
                    MaxHp = enemy.Stats.MaxHp,
                    IsAlive = !enemy.IsDead,
                    SkipTurns = enemy.SkipTurnsRemaining,
                    CatalogId = enemy.Id
                });
            }

            return enemies;
        }

        private static List<NpcSaveData> CaptureNpcs(GameContext ctx)
        {
            List<NpcSaveData> npcs = [];
            foreach (Npc npc in ctx.NpcManager.Npcs)
            {
                NpcSaveData data = new()
                {
                    Id = npc.Id,
                    Name = npc.Name,
                    Glyph = npc.Glyph,
                    X = npc.X,
                    Y = npc.Y,
                    Inventory = CaptureInventory(npc.Inventory)
                };
                npcs.Add(data);
            }

            return npcs;
        }

        private static List<ItemSaveData> CaptureGroundItems(GameContext ctx)
        {
            List<ItemSaveData> items = [];
            foreach (Item item in ctx.ItemManager.Loot)
            {
                items.Add(new ItemSaveData
                {
                    Id = item.Id,
                    Name = item.Name,
                    X = item.X,
                    Y = item.Y
                });
            }

            return items;
        }

        private static List<QuestSaveData> CaptureQuests(GameContext ctx)
        {
            List<QuestSaveData> quests = [];
            foreach (QuestState quest in ctx.QuestManager.Quests)
            {
                quests.Add(new QuestSaveData
                {
                    QuestId = quest.Definition.Id,
                    Status = quest.Status.ToString(),
                    Progress = quest.Progress.ToList()
                });
            }

            return quests;
        }

        private static List<AchievementSaveData> CaptureAchievements(GameContext ctx)
        {
            List<AchievementSaveData> achievements = [];
            foreach (AchievementState achievement in ctx.QuestManager.Achievements)
            {
                achievements.Add(new AchievementSaveData
                {
                    AchievementId = achievement.Definition.Id,
                    IsUnlocked = achievement.IsUnlocked,
                    Progress = achievement.Progress.ToList()
                });
            }

            return achievements;
        }

        private static DialogueSaveData CaptureDialogue(GameContext ctx)
        {
            return new DialogueSaveData
            {
                TalkedNpcs = ctx.DialogueManager.GetTalkedNpcs(),
                CompletedDialogues = ctx.DialogueManager.GetCompletedDialogues(),
                DialogueCounters = ctx.DialogueManager.GetDialogueCounters(),
                VisitedNodes = ctx.DialogueManager.GetVisitedNodes(),
                VisitedOptions = ctx.DialogueManager.GetVisitedOptions()
            };
        }
    }
}