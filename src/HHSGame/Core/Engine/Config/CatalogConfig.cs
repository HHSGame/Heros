using HHSGame.Core.Stats;

namespace HHSGame.Core.Engine.Config
{
    public sealed class CatalogPathsConfig
    {
        public string Weapons { get; init; } = "data/catalogs/weapons.json";
        public string Armors { get; init; } = "data/catalogs/armors.json";
        public string Items { get; init; } = "data/catalogs/items.json";
        public string Classes { get; init; } = "data/catalogs/classes.json";
        public string Enemies { get; init; } = "data/catalogs/enemies.json";
    }

    public sealed class WeaponCatalogConfig
    {
        public List<WeaponDefinitionConfig> Weapons { get; init; } = [];
    }

    public sealed class WeaponDefinitionConfig
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Rarity { get; init; } = "Common";
        public int Value { get; init; }
        public float Weight { get; init; }
        public int Damage { get; init; }
        public int Penetration { get; init; }
        public int Range { get; init; }
        public string WeaponType { get; init; } = "MeleeLight";
        public string Trajectory { get; init; } = "Line";
    }

    public sealed class ArmorCatalogConfig
    {
        public List<ArmorDefinitionConfig> Armors { get; init; } = [];
    }

    public sealed class ArmorDefinitionConfig
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Rarity { get; init; } = "Common";
        public int Value { get; init; }
        public float Weight { get; init; }
        public int ArmorValue { get; init; }
    }

    public sealed class ItemCatalogConfig
    {
        public List<ItemDefinitionConfig> Items { get; init; } = [];
    }

    public sealed class ItemDefinitionConfig
    {
        public string Id { get; init; } = string.Empty;
        public string Kind { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Rarity { get; init; } = "Common";
        public int Value { get; init; }
        public float Weight { get; init; }
        public int HealAmount { get; init; }
    }

    public sealed class ClassCatalogConfig
    {
        public List<ClassDefinitionConfig> Classes { get; init; } = [];
    }

    public sealed class ClassDefinitionConfig
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public Attributes Attributes { get; init; } = new();
        public Dictionary<string, int> Skills { get; init; } = new(StringComparer.OrdinalIgnoreCase);
        public string WeaponId { get; init; } = string.Empty;
        public string ArmorId { get; init; } = string.Empty;
    }

    public sealed class EnemyCatalogConfig
    {
        public List<EnemyDefinitionConfig> Enemies { get; init; } = [];
    }

    public sealed class EnemyDefinitionConfig
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public Attributes Attributes { get; init; } = new();
        public Dictionary<string, int> Skills { get; init; } = new(StringComparer.OrdinalIgnoreCase);
        public string WeaponId { get; init; } = string.Empty;
        public string ArmorId { get; init; } = string.Empty;
        public int ExperienceValue { get; init; }
        public string Glyph { get; init; } = "g";
        public string ColorKey { get; init; } = "Default";
        public List<EnemyAbilityConfig> Abilities { get; init; } = [];
        public List<LootEntryConfig> Loot { get; init; } = [];
    }

    public sealed class EnemyAbilityConfig
    {
        public string Kind { get; init; } = string.Empty;
        public int Chance { get; init; }
        public int Duration { get; init; }
        public int Amount { get; init; }
        public string Message { get; init; } = string.Empty;
    }

    public sealed class LootEntryConfig
    {
        public string ItemId { get; init; } = string.Empty;
        public int Chance { get; init; }
        public int Amount { get; init; }
        public int Quantity { get; init; } = 1;
    }
}
