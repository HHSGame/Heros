using System.Text.Json;
using HHSGame.Core.Save;

namespace HHSGameTest.HHSGame
{
    [TestClass]
    public class SaveLoadTests
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        // ── SaveGameData round-trip ──────────────────────────────────────

        [TestMethod]
        public void SaveGameData_RoundTrip_PreservesBasicFields()
        {
            SaveGameData original = new()
            {
                Version = 1,
                SaveName = "Hero - Turn 42",
                SaveTime = new DateTime(2026, 5, 8, 12, 0, 0, DateTimeKind.Utc),
                TurnNumber = 42,
                GameState = "Exploration"
            };

            string json = JsonSerializer.Serialize(original, JsonOptions);
            SaveGameData? restored = JsonSerializer.Deserialize<SaveGameData>(json);

            Assert.IsNotNull(restored);
            Assert.AreEqual(1, restored.Version);
            Assert.AreEqual("Hero - Turn 42", restored.SaveName);
            Assert.AreEqual(original.TurnNumber, restored.TurnNumber);
            Assert.AreEqual(original.GameState, restored.GameState);
            Assert.AreEqual(original.SaveTime, restored.SaveTime);
        }

        [TestMethod]
        public void SaveGameData_RoundTrip_PreservesPlayerData()
        {
            SaveGameData original = new()
            {
                Players =
                [
                    new PlayerSaveData
                    {
                        Name = "Hero",
                        Glyph = '@',
                        X = 10,
                        Y = 20,
                        Level = 5,
                        Experience = 1200,
                        UnspentSkillPoints = 3,
                        CurrentHp = 80,
                        CurrentSp = 40,
                        Strength = 14,
                        Perception = 10,
                        Agility = 12,
                        Charisma = 8,
                        Intelligence = 11,
                        Skills = new Dictionary<string, int>
                        {
                            { "Melee", 3 },
                            { "Dodge", 2 },
                            { "Stealth", 1 }
                        },
                        Inventory =
                        [
                            new ItemSaveData { Id = "sword_iron", Name = "Iron Sword" },
                            new ItemSaveData { Id = "potion_health", Name = "Health Potion" }
                        ],
                        EquippedWeaponId = "sword_iron",
                        EquippedArmorId = "leather_armor",
                        Effects =
                        [
                            new ActiveEffectSaveData { EffectType = "Poison", Duration = 3 }
                        ],
                        IsActive = true,
                        IsSneaking = false
                    }
                ]
            };

            string json = JsonSerializer.Serialize(original, JsonOptions);
            SaveGameData? restored = JsonSerializer.Deserialize<SaveGameData>(json);

            Assert.IsNotNull(restored);
            Assert.AreEqual(1, restored.Players.Count);

            PlayerSaveData player = restored.Players[0];
            Assert.AreEqual("Hero", player.Name);
            Assert.AreEqual('@', player.Glyph);
            Assert.AreEqual(10, player.X);
            Assert.AreEqual(20, player.Y);
            Assert.AreEqual(5, player.Level);
            Assert.AreEqual(1200, player.Experience);
            Assert.AreEqual(3, player.UnspentSkillPoints);
            Assert.AreEqual(80, player.CurrentHp);
            Assert.AreEqual(40, player.CurrentSp);
            Assert.AreEqual(14, player.Strength);
            Assert.AreEqual(10, player.Perception);
            Assert.AreEqual(12, player.Agility);
            Assert.AreEqual(8, player.Charisma);
            Assert.AreEqual(11, player.Intelligence);
            Assert.AreEqual(3, player.Skills["Melee"]);
            Assert.AreEqual(2, player.Skills["Dodge"]);
            Assert.AreEqual(1, player.Skills["Stealth"]);
            Assert.AreEqual(2, player.Inventory.Count);
            Assert.AreEqual("sword_iron", player.EquippedWeaponId);
            Assert.AreEqual("leather_armor", player.EquippedArmorId);
            Assert.AreEqual(1, player.Effects.Count);
            Assert.AreEqual("Poison", player.Effects[0].EffectType);
            Assert.AreEqual(3, player.Effects[0].Duration);
            Assert.IsTrue(player.IsActive);
            Assert.IsFalse(player.IsSneaking);
        }

        [TestMethod]
        public void SaveGameData_RoundTrip_PreservesMultiplePlayers()
        {
            SaveGameData original = new()
            {
                Players =
                [
                    new PlayerSaveData { Name = "Warrior", Glyph = '@', IsActive = true },
                    new PlayerSaveData { Name = "Mage", Glyph = 'M', IsActive = false }
                ]
            };

            string json = JsonSerializer.Serialize(original, JsonOptions);
            SaveGameData? restored = JsonSerializer.Deserialize<SaveGameData>(json);

            Assert.IsNotNull(restored);
            Assert.AreEqual(2, restored.Players.Count);
            Assert.AreEqual("Warrior", restored.Players[0].Name);
            Assert.IsTrue(restored.Players[0].IsActive);
            Assert.AreEqual("Mage", restored.Players[1].Name);
            Assert.IsFalse(restored.Players[1].IsActive);
        }

        // ── Map data round-trip ──────────────────────────────────────────

        [TestMethod]
        public void SaveGameData_RoundTrip_PreservesMapData()
        {
            SaveGameData original = new()
            {
                Map = new MapSaveData
                {
                    Width = 80,
                    Height = 40,
                    MapPath = "data/maps/forest-01.txt",
                    Cells =
                    [
                        new CellSaveData { X = 0, Y = 0, Character = '#', IsExplored = true },
                        new CellSaveData { X = 1, Y = 1, Character = '.', IsExplored = true },
                        new CellSaveData { X = 5, Y = 5, Character = '~', IsExplored = false }
                    ],
                    HiddenFeatures =
                    [
                        new HiddenFeatureSaveData
                        {
                            X = 3, Y = 4,
                            RevealedGlyph = 'C',
                            Description = "A hidden cache",
                            IsRevealed = false
                        }
                    ],
                    SpecialPositions =
                    [
                        new SpecialPositionSaveData { X = 0, Y = 0, Symbol = '>' }
                    ]
                }
            };

            string json = JsonSerializer.Serialize(original, JsonOptions);
            SaveGameData? restored = JsonSerializer.Deserialize<SaveGameData>(json);

            Assert.IsNotNull(restored);
            Assert.AreEqual(80, restored.Map.Width);
            Assert.AreEqual(40, restored.Map.Height);
            Assert.AreEqual("data/maps/forest-01.txt", restored.Map.MapPath);
            Assert.AreEqual(3, restored.Map.Cells.Count);
            Assert.AreEqual('#', restored.Map.Cells[0].Character);
            Assert.IsTrue(restored.Map.Cells[0].IsExplored);
            Assert.AreEqual('~', restored.Map.Cells[2].Character);
            Assert.IsFalse(restored.Map.Cells[2].IsExplored);
            Assert.AreEqual(1, restored.Map.HiddenFeatures.Count);
            Assert.AreEqual('C', restored.Map.HiddenFeatures[0].RevealedGlyph);
            Assert.IsFalse(restored.Map.HiddenFeatures[0].IsRevealed);
            Assert.AreEqual(1, restored.Map.SpecialPositions.Count);
            Assert.AreEqual('>', restored.Map.SpecialPositions[0].Symbol);
        }

        // ── Enemy data round-trip ────────────────────────────────────────

        [TestMethod]
        public void SaveGameData_RoundTrip_PreservesEnemies()
        {
            SaveGameData original = new()
            {
                Enemies =
                [
                    new EnemySaveData
                    {
                        Name = "Goblin",
                        Glyph = 'g',
                        X = 5, Y = 10,
                        CurrentHp = 15,
                        MaxHp = 30,
                        IsAlive = true,
                        SkipTurns = 1,
                        CatalogId = "goblin"
                    },
                    new EnemySaveData
                    {
                        Name = "Dead Rat",
                        Glyph = 'r',
                        X = 3, Y = 3,
                        CurrentHp = 0,
                        MaxHp = 10,
                        IsAlive = false,
                        SkipTurns = 0,
                        CatalogId = "rat"
                    }
                ]
            };

            string json = JsonSerializer.Serialize(original, JsonOptions);
            SaveGameData? restored = JsonSerializer.Deserialize<SaveGameData>(json);

            Assert.IsNotNull(restored);
            Assert.AreEqual(2, restored.Enemies.Count);
            Assert.AreEqual("Goblin", restored.Enemies[0].Name);
            Assert.AreEqual('g', restored.Enemies[0].Glyph);
            Assert.AreEqual(15, restored.Enemies[0].CurrentHp);
            Assert.AreEqual(30, restored.Enemies[0].MaxHp);
            Assert.IsTrue(restored.Enemies[0].IsAlive);
            Assert.AreEqual(1, restored.Enemies[0].SkipTurns);
            Assert.AreEqual("goblin", restored.Enemies[0].CatalogId);
            Assert.IsFalse(restored.Enemies[1].IsAlive);
        }

        // ── NPC data round-trip ──────────────────────────────────────────

        [TestMethod]
        public void SaveGameData_RoundTrip_PreservesNpcs()
        {
            SaveGameData original = new()
            {
                Npcs =
                [
                    new NpcSaveData
                    {
                        Id = "merchant_01",
                        Name = "Old Merchant",
                        Glyph = 'M',
                        X = 15, Y = 20,
                        Inventory =
                        [
                            new ItemSaveData { Id = "gold_coin", Name = "Gold Coin" }
                        ]
                    }
                ]
            };

            string json = JsonSerializer.Serialize(original, JsonOptions);
            SaveGameData? restored = JsonSerializer.Deserialize<SaveGameData>(json);

            Assert.IsNotNull(restored);
            Assert.AreEqual(1, restored.Npcs.Count);
            Assert.AreEqual("merchant_01", restored.Npcs[0].Id);
            Assert.AreEqual("Old Merchant", restored.Npcs[0].Name);
            Assert.AreEqual('M', restored.Npcs[0].Glyph);
            Assert.AreEqual(15, restored.Npcs[0].X);
            Assert.AreEqual(20, restored.Npcs[0].Y);
            Assert.AreEqual(1, restored.Npcs[0].Inventory.Count);
        }

        // ── Ground items round-trip ──────────────────────────────────────

        [TestMethod]
        public void SaveGameData_RoundTrip_PreservesGroundItems()
        {
            SaveGameData original = new()
            {
                GroundItems =
                [
                    new ItemSaveData { Id = "sword_iron", Name = "Iron Sword", X = 3, Y = 7 },
                    new ItemSaveData { Id = "potion_health", Name = "Health Potion", X = 5, Y = 12 }
                ]
            };

            string json = JsonSerializer.Serialize(original, JsonOptions);
            SaveGameData? restored = JsonSerializer.Deserialize<SaveGameData>(json);

            Assert.IsNotNull(restored);
            Assert.AreEqual(2, restored.GroundItems.Count);
            Assert.AreEqual("sword_iron", restored.GroundItems[0].Id);
            Assert.AreEqual(3, restored.GroundItems[0].X);
            Assert.AreEqual(7, restored.GroundItems[0].Y);
        }

        // ── Quest data round-trip ────────────────────────────────────────

        [TestMethod]
        public void SaveGameData_RoundTrip_PreservesQuests()
        {
            SaveGameData original = new()
            {
                Quests =
                [
                    new QuestSaveData
                    {
                        QuestId = "main_quest_01",
                        Status = "Active",
                        Progress = [1, 0, 0]
                    },
                    new QuestSaveData
                    {
                        QuestId = "side_quest_01",
                        Status = "Completed",
                        Progress = [1, 1, 1]
                    }
                ]
            };

            string json = JsonSerializer.Serialize(original, JsonOptions);
            SaveGameData? restored = JsonSerializer.Deserialize<SaveGameData>(json);

            Assert.IsNotNull(restored);
            Assert.AreEqual(2, restored.Quests.Count);
            Assert.AreEqual("main_quest_01", restored.Quests[0].QuestId);
            Assert.AreEqual("Active", restored.Quests[0].Status);
            Assert.AreEqual(3, restored.Quests[0].Progress.Count);
            Assert.AreEqual(1, restored.Quests[0].Progress[0]);
            Assert.AreEqual(0, restored.Quests[0].Progress[1]);
            Assert.AreEqual("Completed", restored.Quests[1].Status);
        }

        // ── Achievement data round-trip ──────────────────────────────────

        [TestMethod]
        public void SaveGameData_RoundTrip_PreservesAchievements()
        {
            SaveGameData original = new()
            {
                Achievements =
                [
                    new AchievementSaveData
                    {
                        AchievementId = "first_kill",
                        IsUnlocked = true,
                        Progress = [1]
                    },
                    new AchievementSaveData
                    {
                        AchievementId = "explorer",
                        IsUnlocked = false,
                        Progress = [3, 5]
                    }
                ]
            };

            string json = JsonSerializer.Serialize(original, JsonOptions);
            SaveGameData? restored = JsonSerializer.Deserialize<SaveGameData>(json);

            Assert.IsNotNull(restored);
            Assert.AreEqual(2, restored.Achievements.Count);
            Assert.IsTrue(restored.Achievements[0].IsUnlocked);
            Assert.AreEqual("first_kill", restored.Achievements[0].AchievementId);
            Assert.IsFalse(restored.Achievements[1].IsUnlocked);
            Assert.AreEqual(2, restored.Achievements[1].Progress.Count);
        }

        // ── Dialogue data round-trip ─────────────────────────────────────

        [TestMethod]
        public void SaveGameData_RoundTrip_PreservesDialogueState()
        {
            SaveGameData original = new()
            {
                Dialogue = new DialogueSaveData
                {
                    TalkedNpcs = ["merchant_01", "quest_giver_01"],
                    CompletedDialogues = ["intro_dialogue"],
                    DialogueCounters = new Dictionary<string, int>
                    {
                        { "merchant_01", 3 },
                        { "quest_giver_01", 1 }
                    },
                    VisitedNodes = new Dictionary<string, List<string>>
                    {
                        { "merchant_01", ["node_1", "node_2", "node_3"] }
                    },
                    VisitedOptions = new Dictionary<string, List<string>>
                    {
                        { "merchant_01", ["node_1:Buy", "node_1:Sell"] }
                    }
                }
            };

            string json = JsonSerializer.Serialize(original, JsonOptions);
            SaveGameData? restored = JsonSerializer.Deserialize<SaveGameData>(json);

            Assert.IsNotNull(restored);
            Assert.AreEqual(2, restored.Dialogue.TalkedNpcs.Count);
            Assert.IsTrue(restored.Dialogue.TalkedNpcs.Contains("merchant_01"));
            Assert.AreEqual(1, restored.Dialogue.CompletedDialogues.Count);
            Assert.AreEqual(3, restored.Dialogue.DialogueCounters["merchant_01"]);
            Assert.AreEqual(3, restored.Dialogue.VisitedNodes["merchant_01"].Count);
            Assert.AreEqual(2, restored.Dialogue.VisitedOptions["merchant_01"].Count);
        }

        // ── Empty save data round-trip ───────────────────────────────────

        [TestMethod]
        public void SaveGameData_RoundTrip_EmptyData()
        {
            SaveGameData original = new();

            string json = JsonSerializer.Serialize(original, JsonOptions);
            SaveGameData? restored = JsonSerializer.Deserialize<SaveGameData>(json);

            Assert.IsNotNull(restored);
            Assert.AreEqual(1, restored.Version);
            Assert.AreEqual(string.Empty, restored.SaveName);
            Assert.AreEqual(0, restored.TurnNumber);
            Assert.IsNull(restored.GameState);
            Assert.AreEqual(0, restored.Players.Count);
            Assert.AreEqual(0, restored.Enemies.Count);
            Assert.AreEqual(0, restored.Npcs.Count);
            Assert.AreEqual(0, restored.GroundItems.Count);
            Assert.AreEqual(0, restored.Quests.Count);
            Assert.AreEqual(0, restored.Achievements.Count);
            Assert.IsNotNull(restored.Dialogue);
            Assert.AreEqual(0, restored.Dialogue.TalkedNpcs.Count);
        }

        // ── SaveSlotInfo defaults ────────────────────────────────────────

        [TestMethod]
        public void SaveSlotInfo_DefaultValues()
        {
            SaveSlotInfo slot = new();

            Assert.AreEqual(0, slot.SlotNumber);
            Assert.IsFalse(slot.IsOccupied);
            Assert.AreEqual(DateTime.MinValue, slot.SaveTime);
            Assert.AreEqual(0, slot.TurnNumber);
            Assert.AreEqual(string.Empty, slot.PlayerName);
            Assert.AreEqual(0, slot.PlayerLevel);
            Assert.AreEqual(string.Empty, slot.MapName);
        }

        [TestMethod]
        public void SaveSlotInfo_OccupiedSlot()
        {
            SaveSlotInfo slot = new()
            {
                SlotNumber = 1,
                IsOccupied = true,
                SaveTime = DateTime.UtcNow,
                TurnNumber = 100,
                PlayerName = "Hero",
                PlayerLevel = 10,
                MapName = "forest-01"
            };

            Assert.AreEqual(1, slot.SlotNumber);
            Assert.IsTrue(slot.IsOccupied);
            Assert.AreEqual(100, slot.TurnNumber);
            Assert.AreEqual("Hero", slot.PlayerName);
            Assert.AreEqual(10, slot.PlayerLevel);
            Assert.AreEqual("forest-01", slot.MapName);
        }

        // ── Complex full save data round-trip ────────────────────────────

        [TestMethod]
        public void SaveGameData_RoundTrip_ComplexScenario()
        {
            SaveGameData original = new()
            {
                Version = 1,
                SaveName = "Adventurer - Turn 150",
                SaveTime = new DateTime(2026, 5, 8, 14, 30, 0, DateTimeKind.Utc),
                TurnNumber = 150,
                GameState = "Combat",
                Players =
                [
                    new PlayerSaveData
                    {
                        Name = "Adventurer",
                        Glyph = '@',
                        X = 25, Y = 15,
                        Level = 8,
                        Experience = 5000,
                        UnspentSkillPoints = 2,
                        CurrentHp = 65,
                        CurrentSp = 30,
                        Strength = 16,
                        Perception = 12,
                        Agility = 14,
                        Charisma = 10,
                        Intelligence = 13,
                        Skills = new Dictionary<string, int>
                        {
                            { "Melee", 5 },
                            { "Defense", 3 },
                            { "Magic", 4 }
                        },
                        Inventory =
                        [
                            new ItemSaveData { Id = "sword_fire", Name = "Fire Sword" },
                            new ItemSaveData { Id = "shield_iron", Name = "Iron Shield" },
                            new ItemSaveData { Id = "potion_mana", Name = "Mana Potion" }
                        ],
                        EquippedWeaponId = "sword_fire",
                        EquippedArmorId = "shield_iron",
                        Effects =
                        [
                            new ActiveEffectSaveData { EffectType = "Stun", Duration = 1 }
                        ],
                        IsActive = true,
                        IsSneaking = false
                    }
                ],
                Map = new MapSaveData
                {
                    Width = 100,
                    Height = 50,
                    MapPath = "data/maps/cave-01.txt",
                    Cells =
                    [
                        new CellSaveData { X = 0, Y = 0, Character = '#', IsExplored = true },
                        new CellSaveData { X = 25, Y = 15, Character = '.', IsExplored = true }
                    ],
                    HiddenFeatures =
                    [
                        new HiddenFeatureSaveData
                        {
                            X = 30, Y = 20,
                            RevealedGlyph = 'C',
                            Description = "Hidden treasure",
                            IsRevealed = true
                        }
                    ],
                    SpecialPositions =
                    [
                        new SpecialPositionSaveData { X = 25, Y = 15, Symbol = '@' }
                    ]
                },
                Enemies =
                [
                    new EnemySaveData
                    {
                        Name = "Dragon",
                        Glyph = 'D',
                        X = 40, Y = 30,
                        CurrentHp = 150,
                        MaxHp = 200,
                        IsAlive = true,
                        SkipTurns = 0,
                        CatalogId = "dragon"
                    }
                ],
                Npcs =
                [
                    new NpcSaveData
                    {
                        Id = "healer_01",
                        Name = "Village Healer",
                        Glyph = 'H',
                        X = 10, Y = 10,
                        Inventory = []
                    }
                ],
                GroundItems =
                [
                    new ItemSaveData { Id = "gem_ruby", Name = "Ruby Gem", X = 35, Y = 25 }
                ],
                Quests =
                [
                    new QuestSaveData
                    {
                        QuestId = "dragon_slayer",
                        Status = "Active",
                        Progress = [0, 1]
                    }
                ],
                Achievements =
                [
                    new AchievementSaveData
                    {
                        AchievementId = "dragon_hunter",
                        IsUnlocked = false,
                        Progress = [0]
                    }
                ],
                Dialogue = new DialogueSaveData
                {
                    TalkedNpcs = ["healer_01"],
                    CompletedDialogues = [],
                    DialogueCounters = new Dictionary<string, int> { { "healer_01", 2 } },
                    VisitedNodes = new Dictionary<string, List<string>>
                    {
                        { "healer_01", ["welcome", "heal_options"] }
                    },
                    VisitedOptions = new Dictionary<string, List<string>>
                    {
                        { "healer_01", ["welcome:Ask for healing"] }
                    }
                }
            };

            string json = JsonSerializer.Serialize(original, JsonOptions);

            // Verify JSON is valid and non-trivial
            Assert.IsTrue(json.Length > 100, "JSON should have meaningful length");

            SaveGameData? restored = JsonSerializer.Deserialize<SaveGameData>(json);

            Assert.IsNotNull(restored);
            Assert.AreEqual(1, restored.Version);
            Assert.AreEqual("Adventurer - Turn 150", restored.SaveName);
            Assert.AreEqual(150, restored.TurnNumber);
            Assert.AreEqual("Combat", restored.GameState);
            Assert.AreEqual(1, restored.Players.Count);
            Assert.AreEqual(1, restored.Enemies.Count);
            Assert.AreEqual(1, restored.Npcs.Count);
            Assert.AreEqual(1, restored.GroundItems.Count);
            Assert.AreEqual(1, restored.Quests.Count);
            Assert.AreEqual(1, restored.Achievements.Count);
            Assert.AreEqual("healer_01", restored.Dialogue.TalkedNpcs[0]);

            // Verify hidden feature was revealed in save
            Assert.IsTrue(restored.Map.HiddenFeatures[0].IsRevealed);
        }

        // ── Nullable fields ──────────────────────────────────────────────

        [TestMethod]
        public void PlayerSaveData_NullableEquippedArmor()
        {
            PlayerSaveData player = new()
            {
                Name = "Fighter",
                EquippedWeaponId = "sword_basic",
                EquippedArmorId = null
            };

            string json = JsonSerializer.Serialize(player, JsonOptions);
            PlayerSaveData? restored = JsonSerializer.Deserialize<PlayerSaveData>(json);

            Assert.IsNotNull(restored);
            Assert.AreEqual("sword_basic", restored.EquippedWeaponId);
            Assert.IsNull(restored.EquippedArmorId);
        }

        [TestMethod]
        public void EnemySaveData_NullableCatalogId()
        {
            EnemySaveData enemy = new()
            {
                Name = "Unknown",
                Glyph = '?',
                CatalogId = null
            };

            string json = JsonSerializer.Serialize(enemy, JsonOptions);
            EnemySaveData? restored = JsonSerializer.Deserialize<EnemySaveData>(json);

            Assert.IsNotNull(restored);
            Assert.IsNull(restored.CatalogId);
        }

        // ── MapSaveData null MapPath ─────────────────────────────────────

        [TestMethod]
        public void MapSaveData_NullMapPath_RepresentsGeneratedMap()
        {
            MapSaveData map = new()
            {
                Width = 50,
                Height = 30,
                MapPath = null
            };

            string json = JsonSerializer.Serialize(map, JsonOptions);
            MapSaveData? restored = JsonSerializer.Deserialize<MapSaveData>(json);

            Assert.IsNotNull(restored);
            Assert.IsNull(restored.MapPath);
            Assert.AreEqual(50, restored.Width);
            Assert.AreEqual(30, restored.Height);
        }
    }
}