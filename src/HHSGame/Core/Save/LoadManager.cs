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
    public sealed class LoadManager
    {
        private readonly ILogger<LoadManager> logger;
        private readonly string saveDirectory;

        public LoadManager(ILogger<LoadManager> logger, string? saveDirectory = null)
        {
            this.logger = logger;
            this.saveDirectory = saveDirectory ?? SaveManager.DefaultSaveDirectory;
        }

        public string GetSlotPath(int slot)
        {
            return Path.Combine(saveDirectory, $"slot-{slot}.json");
        }

        public bool SlotExists(int slot)
        {
            return File.Exists(GetSlotPath(slot));
        }

        public SaveGameData? LoadSlot(int slot)
        {
            string path = GetSlotPath(slot);
            if (!File.Exists(path))
            {
                logger.LogWarning("Save slot {Slot} does not exist", slot);
                return null;
            }

            try
            {
                string json = File.ReadAllText(path);
                SaveGameData? data = JsonSerializer.Deserialize<SaveGameData>(json);
                if (data == null)
                {
                    logger.LogWarning("Save slot {Slot} deserialized to null", slot);
                }
                else if (data.Version != 1)
                {
                    logger.LogWarning("Save slot {Slot} has incompatible version {Version}", slot, data.Version);
                    return null;
                }

                return data;
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Save file for slot {Slot} is corrupted", slot);
                return null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to load save slot {Slot}", slot);
                return null;
            }
        }

        public static bool CanLoadInCurrentState(GameStateType state)
        {
            return state is GameStateType.Exploration or GameStateType.GameOver;
        }

        public bool DeleteSlot(int slot)
        {
            string path = GetSlotPath(slot);
            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                File.Delete(path);
                logger.LogInformation("Deleted save slot {Slot}", slot);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete save slot {Slot}", slot);
                return false;
            }
        }

        /// <summary>
        /// Restores game state from save data. This rebuilds the entire game world.
        /// </summary>
        public bool RestoreGameState(Game game, SaveGameData data)
        {
            try
            {
                GameContext ctx = game.Context;

                // 1. Restore map
                RestoreMap(ctx, data.Map);

                // 2. Restore players
                List<Player> players = RestorePlayers(ctx, data.Players);

                // 3. Restore enemies
                RestoreEnemies(ctx, data.Enemies);

                // 4. Restore NPCs
                RestoreNpcs(ctx, data.Npcs);

                // 5. Restore ground items
                RestoreGroundItems(ctx, data.GroundItems);

                // 6. Restore context with restored players (without re-spawning enemies/NPCs)
                Player activePlayer = players.FirstOrDefault(p => p.IsActive) ?? players[0];
                ctx.RestoreContext(players, activePlayer);

                // 7. Restore quests (after quest definitions are loaded)
                RestoreQuests(ctx, data.Quests);

                // 8. Restore achievements (after quest definitions are loaded)
                RestoreAchievements(ctx, data.Achievements);

                // 9. Restore dialogue state
                RestoreDialogue(ctx, data.Dialogue);

                // 10. Restore turn count
                ctx.TurnManager.SetTurnNumber(data.TurnNumber);

                // 11. Restore game state
                if (!string.IsNullOrEmpty(data.GameState)
                    && Enum.TryParse<GameStateType>(data.GameState, true, out GameStateType state))
                {
                    ctx.StateMachine.TryChangeState(state);
                }

                // 12. Recalculate FOV for active player
                activePlayer.UpdateFOV();

                logger.LogInformation("Game state restored successfully (turn {Turn})", data.TurnNumber);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to restore game state");
                return false;
            }
        }

        private static void RestoreMap(GameContext ctx, MapSaveData mapData)
        {
            MapState mapState = ctx.MapState;

            // If we have a map path, reload the base map from file
            if (!string.IsNullOrEmpty(mapData.MapPath) && File.Exists(mapData.MapPath))
            {
                MapData baseMap = MapLoader.LoadFromFile(mapData.MapPath);
                mapState.Init(baseMap);
            }
            else
            {
                // Reconstruct from saved cells
                Cell[,] cells = new Cell[mapData.Height, mapData.Width];
                // Initialize with walls
                for (int y = 0; y < mapData.Height; y++)
                {
                    for (int x = 0; x < mapData.Width; x++)
                    {
                        cells[y, x] = new Cell { Character = '#', Attribute = TilePresets.GetTerrainColor('#') };
                    }
                }

                MapData reconstructed = new(cells, []);
                mapState.Init(reconstructed);
            }

            // Apply saved cell overrides
            foreach (CellSaveData cellData in mapData.Cells)
            {
                if (mapState.IsInBounds(cellData.X, cellData.Y))
                {
                    mapState.SetCell(cellData.X, cellData.Y, new Cell
                    {
                        Character = cellData.Character,
                        Attribute = TilePresets.GetTerrainColor(cellData.Character)
                    });
                }
            }

            // Restore visited tiles
            foreach (CellSaveData cellData in mapData.Cells)
            {
                if (cellData.IsExplored)
                {
                    mapState.MarkExplored(cellData.X, cellData.Y);
                }
            }

            // Restore hidden features
            foreach (HiddenFeatureSaveData hfData in mapData.HiddenFeatures)
            {
                HiddenFeature feature = new(
                    new Coordinate(hfData.X, hfData.Y),
                    hfData.RevealedGlyph,
                    hfData.Description
                )
                {
                    Revealed = hfData.IsRevealed
                };
                mapState.AddHiddenFeature(feature);

                // If revealed, update the cell
                if (hfData.IsRevealed && mapState.IsInBounds(hfData.X, hfData.Y))
                {
                    mapState.SetCell(hfData.X, hfData.Y, new Cell
                    {
                        Character = hfData.RevealedGlyph,
                        Attribute = TilePresets.GetTerrainColor(hfData.RevealedGlyph)
                    });
                }
            }
        }

        private static List<Player> RestorePlayers(GameContext ctx, List<PlayerSaveData> playersData)
        {
            List<Player> players = [];

            foreach (PlayerSaveData pData in playersData)
            {
                // Build attributes
                Attributes attributes = new()
                {
                    Strength = pData.Strength,
                    Perception = pData.Perception,
                    Agility = pData.Agility,
                    Charisma = pData.Charisma,
                    Intelligence = pData.Intelligence
                };

                // Build skills
                Skills skills = new();
                foreach ((string skillName, int rank) in pData.Skills)
                {
                    if (Enum.TryParse<SkillType>(skillName, true, out SkillType skillType))
                    {
                        skills.SetRank(skillType, rank);
                    }
                }

                // Create player
                Player player = new(pData.X, pData.Y, ctx, pData.Name, pData.Glyph, attributes, skills);

                // Restore HP/SP
                player.Stats.SetHp(pData.CurrentHp);
                player.Stats.SetSp(pData.CurrentSp);

                // Restore progression
                player.Stats.Progression.SetLevel(pData.Level);
                player.Stats.Progression.SetExperience(pData.Experience);
                player.Stats.Progression.SetUnspentSkillPoints(pData.UnspentSkillPoints);

                // Restore inventory
                foreach (ItemSaveData itemData in pData.Inventory)
                {
                    Item item = CreateItemFromSaveData(ctx, itemData);
                    player.AddItem(item);
                }

                // Restore equipped weapon
                if (!string.IsNullOrEmpty(pData.EquippedWeaponId))
                {
                    if (ctx.ItemCatalog.TryCreateItem(pData.EquippedWeaponId, out Item weaponItem)
                        && weaponItem is Weapon weapon)
                    {
                        player.EquipWeapon(weapon);
                    }
                }

                // Restore equipped armor (legacy single slot)
                if (!string.IsNullOrEmpty(pData.EquippedArmorId))
                {
                    if (ctx.ItemCatalog.TryCreateItem(pData.EquippedArmorId, out Item armorItem)
                        && armorItem is Armor armor)
                    {
                        player.EquipArmor(armor);
                    }
                }

                // Restore equipment slots (new multi-slot system)
                foreach (EquipmentSlotSaveData slotData in pData.EquipmentSlots)
                {
                    if (Enum.TryParse<EquipmentSlot>(slotData.Slot, true, out EquipmentSlot slot))
                    {
                        if (ctx.ItemCatalog.TryCreateItem(slotData.ArmorId, out Item armorItem)
                            && armorItem is Armor armor)
                        {
                            player.Equipment.RestoreEquipment(slot, armor);
                        }
                    }
                }

                // Restore active effects
                foreach (ActiveEffectSaveData effectData in pData.Effects)
                {
                    ActiveEffect? effect = effectData.EffectType switch
                    {
                        "Poison" => new PoisonEffect(effectData.Duration, 0),
                        "Stun" => new StunEffect(effectData.Duration),
                        _ => null
                    };
                    if (effect != null)
                    {
                        player.ApplyEffect(effect);
                    }
                }

                // Restore sneaking state
                if (pData.IsSneaking)
                {
                    player.SetSneaking(true);
                }

                players.Add(player);
            }

            return players;
        }

        private void RestoreEnemies(GameContext ctx, List<EnemySaveData> enemiesData)
        {
            List<Enemy> enemies = [];

            foreach (EnemySaveData eData in enemiesData)
            {
                if (!eData.IsAlive)
                {
                    continue; // Skip dead enemies
                }

                if (string.IsNullOrEmpty(eData.CatalogId))
                {
                    continue; // Can't reconstruct without catalog ID
                }

                // Create enemy from factory/catalog
                Enemy enemy = ctx.EnemyFactory.CreateEnemy(
                    eData.CatalogId,
                    eData.X,
                    eData.Y
                );

                if (enemy == null)
                {
                    logger.LogWarning("Could not create enemy with catalog ID {CatalogId}", eData.CatalogId);
                    continue;
                }

                // Restore HP (apply damage if needed)
                int damage = enemy.Stats.CurrentHp - eData.CurrentHp;
                if (damage > 0)
                {
                    enemy.TakeDamage(damage);
                }

                // Restore skip turns
                if (eData.SkipTurns > 0)
                {
                    enemy.SkipNextTurns(eData.SkipTurns);
                }

                enemies.Add(enemy);
            }

            ctx.EnemyManager.SetEnemies(enemies);
        }

        private void RestoreNpcs(GameContext ctx, List<NpcSaveData> npcsData)
        {
            List<Npc> npcs = [];
            Dictionary<string, NpcSpawn> spawnsById = ctx.Parameters.NpcSpawns
                .ToDictionary(s => s.Id, s => s);

            foreach (NpcSaveData nData in npcsData)
            {
                // Find original spawn definition for this NPC
                if (!spawnsById.TryGetValue(nData.Id, out NpcSpawn? spawn))
                {
                    logger.LogWarning("Could not find NPC spawn definition for {Id}", nData.Id);
                    continue;
                }

                // Create NPC using factory with original definition but saved position
                Npc npc = new(
                    spawn.Id,
                    spawn.Name,
                    spawn.Glyph,
                    spawn.Attribute,
                    nData.X,
                    nData.Y,
                    spawn.Attributes,
                    spawn.Skills,
                    ctx.ItemCatalog,
                    spawn.DialogueId);

                // Restore NPC inventory (clear any starting items first)
                npc.Inventory.ObservableItems.Clear();
                foreach (ItemSaveData itemData in nData.Inventory)
                {
                    Item item = CreateItemFromSaveData(ctx, itemData);
                    npc.Inventory.ObservableItems.Add(item);
                }

                npcs.Add(npc);
            }

            ctx.NpcManager.SetNpcs(npcs);
        }

        private static void RestoreGroundItems(GameContext ctx, List<ItemSaveData> itemsData)
        {
            foreach (ItemSaveData itemData in itemsData)
            {
                Item item = CreateItemFromSaveData(ctx, itemData);
                item.X = itemData.X;
                item.Y = itemData.Y;
                ctx.ItemManager.AddItem(item);
            }
        }

        private static void RestoreQuests(GameContext ctx, List<QuestSaveData> questsData)
        {
            foreach (QuestSaveData qData in questsData)
            {
                if (Enum.TryParse<QuestStatus>(qData.Status, true, out QuestStatus status))
                {
                    ctx.QuestManager.RestoreQuest(qData.QuestId, status, qData.Progress);
                }
            }
        }

        private static void RestoreAchievements(GameContext ctx, List<AchievementSaveData> achievementsData)
        {
            foreach (AchievementSaveData aData in achievementsData)
            {
                ctx.QuestManager.RestoreAchievement(aData.AchievementId, aData.IsUnlocked, aData.Progress);
            }
        }

        private static void RestoreDialogue(GameContext ctx, DialogueSaveData dialogueData)
        {
            ctx.DialogueManager.RestoreTalkedNpcs(dialogueData.TalkedNpcs);
            ctx.DialogueManager.RestoreCompletedDialogues(dialogueData.CompletedDialogues);
            ctx.DialogueManager.RestoreDialogueCounters(dialogueData.DialogueCounters);
            if (dialogueData.VisitedNodes.Count > 0)
            {
                ctx.DialogueManager.RestoreVisitedNodes(dialogueData.VisitedNodes);
            }

            if (dialogueData.VisitedOptions.Count > 0)
            {
                ctx.DialogueManager.RestoreVisitedOptions(dialogueData.VisitedOptions);
            }
        }

        private static Item CreateItemFromSaveData(GameContext ctx, ItemSaveData itemData)
        {
            // Try to create from catalog first (preferred for proper type reconstruction)
            if (ctx.ItemCatalog.TryCreateItem(itemData.Id, out Item catalogItem))
            {
                return catalogItem;
            }

            // Fallback: create a basic HealthPotion (most common loose item)
            return new HealthPotion(itemData.Id, itemData.Name, ItemRarity.Common, 0, 0f, 0);
        }
    }
}