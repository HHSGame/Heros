namespace HHSGame.Core.Save
{
    // ── Top-level save container ─────────────────────────────────────────

    public sealed class SaveGameData
    {
        public int Version { get; set; } = 1;
        public string SaveName { get; set; } = string.Empty;
        public DateTime SaveTime { get; set; }
        public int TurnNumber { get; set; }
        public string? GameState { get; set; }
        public List<PlayerSaveData> Players { get; set; } = [];
        public MapSaveData Map { get; set; } = new();
        public List<EnemySaveData> Enemies { get; set; } = [];
        public List<NpcSaveData> Npcs { get; set; } = [];
        public List<ItemSaveData> GroundItems { get; set; } = [];
        public List<QuestSaveData> Quests { get; set; } = [];
        public List<AchievementSaveData> Achievements { get; set; } = [];
        public DialogueSaveData Dialogue { get; set; } = new();
    }

    // ── Save slot metadata (for UI listing) ─────────────────────────────

    public sealed class SaveSlotInfo
    {
        public int SlotNumber { get; set; }
        public bool IsOccupied { get; set; }
        public DateTime SaveTime { get; set; }
        public int TurnNumber { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public int PlayerLevel { get; set; }
        public string MapName { get; set; } = string.Empty;
    }

    // ── Player ──────────────────────────────────────────────────────────

    public sealed class PlayerSaveData
    {
        public string Name { get; set; } = string.Empty;
        public char Glyph { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int UnspentSkillPoints { get; set; }
        public int CurrentHp { get; set; }
        public int CurrentSp { get; set; }
        public int Strength { get; set; }
        public int Perception { get; set; }
        public int Agility { get; set; }
        public int Charisma { get; set; }
        public int Intelligence { get; set; }
        public Dictionary<string, int> Skills { get; set; } = [];
        public List<ItemSaveData> Inventory { get; set; } = [];
        public string? EquippedWeaponId { get; set; }
        public string? EquippedArmorId { get; set; }
        public List<EquipmentSlotSaveData> EquipmentSlots { get; set; } = [];
        public List<ActiveEffectSaveData> Effects { get; set; } = [];
        public bool IsActive { get; set; }
        public bool IsSneaking { get; set; }
    }

    // ── Equipment Slots ────────────────────────────────────────────────

    public sealed class EquipmentSlotSaveData
    {
        public string Slot { get; set; } = string.Empty;
        public string ArmorId { get; set; } = string.Empty;
    }

    // ── Items ───────────────────────────────────────────────────────────

    public sealed class ItemSaveData
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        // For ground items (X/Y may be 0 for inventory items)
        public int X { get; set; }
        public int Y { get; set; }
    }

    // ── Active Effects ──────────────────────────────────────────────────

    public sealed class ActiveEffectSaveData
    {
        public string EffectType { get; set; } = string.Empty;
        public int Duration { get; set; }
    }

    // ── Map ─────────────────────────────────────────────────────────────

    public sealed class MapSaveData
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string? MapPath { get; set; }
        public List<CellSaveData> Cells { get; set; } = [];
        public List<HiddenFeatureSaveData> HiddenFeatures { get; set; } = [];
        public List<SpecialPositionSaveData> SpecialPositions { get; set; } = [];
    }

    public sealed class CellSaveData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public char Character { get; set; }
        public bool IsExplored { get; set; }
    }

    public sealed class HiddenFeatureSaveData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public char RevealedGlyph { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsRevealed { get; set; }
    }

    public sealed class SpecialPositionSaveData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public char Symbol { get; set; }
    }

    // ── Enemies ─────────────────────────────────────────────────────────

    public sealed class EnemySaveData
    {
        public string Name { get; set; } = string.Empty;
        public char Glyph { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int CurrentHp { get; set; }
        public int MaxHp { get; set; }
        public bool IsAlive { get; set; }
        public int SkipTurns { get; set; }
        // Store the catalog ID so we can reconstruct the enemy
        public string? CatalogId { get; set; }
    }

    // ── NPCs ────────────────────────────────────────────────────────────

    public sealed class NpcSaveData
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public char Glyph { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public List<ItemSaveData> Inventory { get; set; } = [];
    }

    // ── Quests ──────────────────────────────────────────────────────────

    public sealed class QuestSaveData
    {
        public string QuestId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<int> Progress { get; set; } = [];
    }

    public sealed class AchievementSaveData
    {
        public string AchievementId { get; set; } = string.Empty;
        public bool IsUnlocked { get; set; }
        public List<int> Progress { get; set; } = [];
    }

    // ── Dialogue ────────────────────────────────────────────────────────

    public sealed class DialogueSaveData
    {
        public List<string> TalkedNpcs { get; set; } = [];
        public List<string> CompletedDialogues { get; set; } = [];
        public Dictionary<string, int> DialogueCounters { get; set; } = [];
        // Key: npcId, Value: list of visited node IDs
        public Dictionary<string, List<string>> VisitedNodes { get; set; } = [];
        // Key: npcId, Value: list of "nodeId:optionText" keys
        public Dictionary<string, List<string>> VisitedOptions { get; set; } = [];
    }
}