using System.IO;
using HHSGame.Core.Engine.Catalogs;
using HHSGame.Core.Engine.Config;
using HHSGame.Core.Enemies;
using HHSGame.Core.Items;

namespace HHSGame.Core.Engine
{
    public sealed class GameCatalogLoader(GameDataLoader dataLoader)
    {
        public GameCatalog Load(CatalogPathsConfig paths)
        {
            WeaponCatalogConfig weaponConfig = dataLoader.LoadData<WeaponCatalogConfig>(paths.Weapons)
                ?? new WeaponCatalogConfig();
            ArmorCatalogConfig armorConfig = dataLoader.LoadData<ArmorCatalogConfig>(paths.Armors)
                ?? new ArmorCatalogConfig();
            ItemCatalogConfig itemConfig = dataLoader.LoadData<ItemCatalogConfig>(paths.Items)
                ?? new ItemCatalogConfig();
            ClassCatalogConfig classConfig = dataLoader.LoadData<ClassCatalogConfig>(paths.Classes)
                ?? new ClassCatalogConfig();
            EnemyCatalogConfig enemyConfig = dataLoader.LoadData<EnemyCatalogConfig>(paths.Enemies)
                ?? new EnemyCatalogConfig();

            List<WeaponDefinition> weapons = weaponConfig.Weapons.Select(ParseWeapon).ToList();
            List<ArmorDefinition> armors = armorConfig.Armors.Select(ParseArmor).ToList();
            List<ItemDefinition> items = itemConfig.Items.Select(ParseItem).ToList();

            ItemCatalog itemCatalog = new(weapons, armors, items);

            List<ClassDefinition> classes = classConfig.Classes.Select(config =>
                new ClassDefinition(
                    RequireId(config.Id, "class"),
                    string.IsNullOrWhiteSpace(config.Name) ? config.Id : config.Name,
                    config.Attributes,
                    SkillMapper.BuildSkills(config.Skills),
                    RequireWeapon(itemCatalog, config.WeaponId, $"class {config.Id} weapon"),
                    RequireArmor(itemCatalog, config.ArmorId, $"class {config.Id} armor")))
                .ToList();

            ClassCatalog classCatalog = new(classes, itemCatalog);

            List<EnemyDefinition> enemies = enemyConfig.Enemies.Select(config =>
            {
                string id = RequireId(config.Id, "enemy");
                char glyph = string.IsNullOrWhiteSpace(config.Glyph) ? 'e' : config.Glyph[0];
                List<EnemyAbilityDefinition> abilities = ParseAbilities(config.Abilities, id);
                return new EnemyDefinition(
                    id,
                    string.IsNullOrWhiteSpace(config.Name) ? id : config.Name,
                    string.IsNullOrWhiteSpace(config.Faction) ? "Neutral" : config.Faction,                    config.Attributes,
                    SkillMapper.BuildSkills(config.Skills),
                    RequireWeapon(itemCatalog, config.WeaponId, $"enemy {id} weapon"),
                    RequireArmor(itemCatalog, config.ArmorId, $"enemy {id} armor"),
                    config.ExperienceValue,
                    glyph,
                    EnemyCatalog.ResolveEnemyColor(config.ColorKey),
                    config.Loot.Select(loot => new EnemyLootEntry(
                        RequireItem(itemCatalog, loot.ItemId, $"enemy {id} loot item"),
                        loot.Chance,
                        loot.Amount,
                        loot.Quantity)).ToList(),
                    abilities);
            }).ToList();

            EnemyCatalog enemyCatalog = new(enemies);

            return new GameCatalog(itemCatalog, classCatalog, enemyCatalog);
        }

        private static WeaponDefinition ParseWeapon(WeaponDefinitionConfig config)
        {
            return new WeaponDefinition(
                RequireId(config.Id, "weapon"),
                string.IsNullOrWhiteSpace(config.Name) ? config.Id : config.Name,
                ParseEnum<ItemRarity>(config.Rarity, "weapon rarity"),
                config.Value,
                config.Weight,
                config.Damage,
                config.Penetration,
                config.Range,
                ParseEnum<WeaponType>(config.WeaponType, "weapon type"),
                ParseEnum<WeaponTrajectory>(string.IsNullOrWhiteSpace(config.Trajectory) ? "Line" : config.Trajectory, "weapon trajectory"));
        }

        private static ArmorDefinition ParseArmor(ArmorDefinitionConfig config)
        {
            EquipmentSlot slot = EquipmentSlot.Body;
            if (!string.IsNullOrWhiteSpace(config.Slot))
            {
                EquipmentSlot? parsed = EquipmentSlotRules.ParseSlot(config.Slot);
                if (parsed.HasValue)
                {
                    slot = parsed.Value;
                }
            }

            return new ArmorDefinition(
                RequireId(config.Id, "armor"),
                string.IsNullOrWhiteSpace(config.Name) ? config.Id : config.Name,
                ParseEnum<ItemRarity>(config.Rarity, "armor rarity"),
                config.Value,
                config.Weight,
                config.ArmorValue,
                slot);
        }

        private static ItemDefinition ParseItem(ItemDefinitionConfig config)
        {
            return new ItemDefinition(
                RequireId(config.Id, "item"),
                RequireId(config.Kind, $"item {config.Id} kind"),
                string.IsNullOrWhiteSpace(config.Name) ? config.Id : config.Name,
                ParseEnum<ItemRarity>(config.Rarity, "item rarity"),
                config.Value,
                config.Weight,
                config.HealAmount);
        }

        private static T ParseEnum<T>(string value, string context)
            where T : struct, Enum
        {
            if (!Enum.TryParse(value, true, out T parsed))
            {
                throw new InvalidDataException($"Unknown {context} '{value}'.");
            }

            return parsed;
        }

        private static string RequireId(string value, string context)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidDataException($"Missing {context} id.");
            }

            return value.Trim();
        }

        private static string RequireWeapon(ItemCatalog catalog, string value, string context)
        {
            string id = RequireId(value, context);
            if (!catalog.TryGetWeaponDefinition(id, out _))
            {
                throw new InvalidDataException($"Unknown weapon id '{id}' for {context}.");
            }

            return id;
        }

        private static List<EnemyAbilityDefinition> ParseAbilities(List<EnemyAbilityConfig>? abilities, string enemyId)
        {
            if (abilities == null || abilities.Count == 0)
            {
                return [];
            }

            List<EnemyAbilityDefinition> result = [];
            foreach (EnemyAbilityConfig ability in abilities)
            {
                string kindValue = RequireId(ability.Kind, $"enemy {enemyId} ability kind");
                EnemyAbilityKind kind = ParseEnum<EnemyAbilityKind>(kindValue, $"enemy {enemyId} ability kind");

                if (ability.Chance < 0 || ability.Chance > 100)
                {
                    throw new InvalidDataException($"Enemy {enemyId} ability '{kindValue}' chance must be between 0 and 100.");
                }

                if (ability.Amount < 0)
                {
                    throw new InvalidDataException($"Enemy {enemyId} ability '{kindValue}' amount cannot be negative.");
                }

                if (ability.Duration < 0)
                {
                    throw new InvalidDataException($"Enemy {enemyId} ability '{kindValue}' duration cannot be negative.");
                }

                result.Add(new EnemyAbilityDefinition(
                    kind,
                    ability.Chance,
                    ability.Amount,
                    ability.Duration,
                    ability.Message?.Trim() ?? string.Empty));
            }

            return result;
        }

        private static string RequireArmor(ItemCatalog catalog, string value, string context)
        {
            string id = RequireId(value, context);
            if (!catalog.TryGetArmorDefinition(id, out _))
            {
                throw new InvalidDataException($"Unknown armor id '{id}' for {context}.");
            }

            return id;
        }

        private static string RequireItem(ItemCatalog catalog, string value, string context)
        {
            string id = RequireId(value, context);
            if (!catalog.TryCreateItem(id, out _))
            {
                throw new InvalidDataException($"Unknown item id '{id}' for {context}.");
            }

            return id;
        }
    }
}
