using HHSGame.Core.Enemies;
using HHSGame.Core.Items;
using HHSGame.Core.Stats;
using Attribute = Terminal.Gui.Drawing.Attribute;

namespace HHSGame.Core.Engine.Catalogs
{
    public sealed record WeaponDefinition(
        string Id,
        string Name,
        ItemRarity Rarity,
        int Value,
        float Weight,
        int Damage,
        int Penetration,
        int Range,
        WeaponType WeaponType,
        WeaponTrajectory Trajectory);

    public sealed record ArmorDefinition(
        string Id,
        string Name,
        ItemRarity Rarity,
        int Value,
        float Weight,
        int ArmorValue);

    public sealed record ItemDefinition(
        string Id,
        string Kind,
        string Name,
        ItemRarity Rarity,
        int Value,
        float Weight,
        int HealAmount);

    public sealed record ClassDefinition(
        string Id,
        string Name,
        Attributes Attributes,
        Skills Skills,
        string WeaponId,
        string ArmorId);

    public sealed record EnemyLootEntry(
        string ItemId,
        int Chance,
        int Amount,
        int Quantity);

    public sealed record EnemyAbilityDefinition(
        EnemyAbilityKind Kind,
        int Chance,
        int Amount,
        int Duration,
        string Message);

    public sealed record EnemyDefinition(
        string Id,
        string Name,
        Attributes Attributes,
        Skills Skills,
        string WeaponId,
        string ArmorId,
        int ExperienceValue,
        char Glyph,
        Attribute Attribute,
        IReadOnlyList<EnemyLootEntry> Loot,
        IReadOnlyList<EnemyAbilityDefinition> Abilities);
}
