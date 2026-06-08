using System.IO;
using HHSGame.Core.Engine.Catalogs;

namespace HHSGame.Core.Items
{
    public sealed class ItemCatalog
    {
        private readonly Dictionary<string, WeaponDefinition> weapons;
        private readonly Dictionary<string, ArmorDefinition> armors;
        private readonly Dictionary<string, ItemDefinition> items;

        public string UnknownWeaponId { get; } = "UnknownWeapon";
        public string UnknownArmorId { get; } = "UnknownArmor";

        public ItemCatalog(
            IEnumerable<WeaponDefinition> weaponDefinitions,
            IEnumerable<ArmorDefinition> armorDefinitions,
            IEnumerable<ItemDefinition> itemDefinitions)
        {
            weapons = weaponDefinitions.ToDictionary(def => def.Id, StringComparer.OrdinalIgnoreCase);
            armors = armorDefinitions.ToDictionary(def => def.Id, StringComparer.OrdinalIgnoreCase);
            items = itemDefinitions.ToDictionary(def => def.Id, StringComparer.OrdinalIgnoreCase);
        }

        public bool TryCreateItem(string id, out Item item)
        {
            return TryCreateItem(id, null, out item);
        }

        public bool TryCreateItem(string id, int? amountOverride, out Item item)
        {
            item = null!;
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            if (weapons.TryGetValue(id, out WeaponDefinition? weaponDefinition) && weaponDefinition != null)
            {
                item = BuildWeapon(weaponDefinition);
                return true;
            }

            if (armors.TryGetValue(id, out ArmorDefinition? armorDefinition) && armorDefinition != null)
            {
                item = BuildArmor(armorDefinition);
                return true;
            }

            if (items.TryGetValue(id, out ItemDefinition? itemDefinition) && itemDefinition != null)
            {
                item = BuildItem(itemDefinition, amountOverride);
                return true;
            }

            return false;
        }

        public Weapon CreateWeapon(string id)
        {
            if (!weapons.TryGetValue(id, out WeaponDefinition? definition) || definition == null)
            {
                if (!weapons.TryGetValue(UnknownWeaponId, out WeaponDefinition? fallback) || fallback == null)
                {
                    fallback = new WeaponDefinition(UnknownWeaponId, "Unknown", ItemRarity.Common, 0, 0, 0, 0, 1, WeaponType.MeleeLight, WeaponTrajectory.Line);
                }

                definition = fallback;
            }

            return BuildWeapon(definition);
        }

        public Armor CreateArmor(string id)
        {
            if (!armors.TryGetValue(id, out ArmorDefinition? definition) || definition == null)
            {
                if (!armors.TryGetValue(UnknownArmorId, out ArmorDefinition? fallback) || fallback == null)
                {
                    fallback = new ArmorDefinition(UnknownArmorId, "Unknown", ItemRarity.Common, 0, 0, 0, EquipmentSlot.Body);
                }

                definition = fallback;
            }

            return BuildArmor(definition);
        }

        public bool TryGetWeaponDefinition(string id, out WeaponDefinition definition)
        {
            definition = null!;
            if (weapons.TryGetValue(id, out WeaponDefinition? found) && found != null)
            {
                definition = found;
                return true;
            }

            return false;
        }

        public bool TryGetArmorDefinition(string id, out ArmorDefinition definition)
        {
            definition = null!;
            if (armors.TryGetValue(id, out ArmorDefinition? found) && found != null)
            {
                definition = found;
                return true;
            }

            return false;
        }

        private static Weapon BuildWeapon(WeaponDefinition definition)
        {
            return new Weapon(
                definition.Id,
                definition.Name,
                definition.Rarity,
                definition.Value,
                definition.Weight,
                definition.Damage,
                definition.Penetration,
                definition.Range,
                definition.WeaponType,
                definition.Trajectory);
        }

        private static Armor BuildArmor(ArmorDefinition definition)
        {
            return new Armor(
                definition.Id,
                definition.Name,
                definition.Rarity,
                definition.Value,
                definition.Weight,
                definition.ArmorValue,
                definition.Slot);
        }

        private static HealthPotion BuildItem(ItemDefinition definition, int? amountOverride)
        {
            string kind = definition.Kind.Trim();
            switch (kind)
            {
                case "HealthPotion":
                    int amount = amountOverride ?? definition.HealAmount;
                    return new HealthPotion(definition.Id, definition.Name, definition.Rarity, definition.Value, definition.Weight, amount);
                default:
                    throw new InvalidDataException($"Unknown item kind '{definition.Kind}'.");
            }
        }
    }
}
