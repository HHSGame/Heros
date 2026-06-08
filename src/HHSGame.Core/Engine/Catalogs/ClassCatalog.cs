using HHSGame.Core.Classes;
using HHSGame.Core.Items;

namespace HHSGame.Core.Engine.Catalogs
{
    public sealed class ClassCatalog
    {
        private readonly Dictionary<string, ClassDefinition> definitions;
        private readonly ItemCatalog itemCatalog;

        public ClassCatalog(IEnumerable<ClassDefinition> definitions, ItemCatalog itemCatalog)
        {
            this.definitions = definitions.ToDictionary(def => def.Id, StringComparer.OrdinalIgnoreCase);
            this.itemCatalog = itemCatalog;
        }

        public IReadOnlyList<ClassConfig> GetAll()
        {
            return definitions.Values
                .Select(BuildClassConfig)
                .ToList();
        }

        public ClassConfig GetDefault()
        {
            ClassDefinition? definition = definitions.Values.FirstOrDefault();
            if (definition == null)
            {
                return new ClassConfig(
                    "Unassigned",
                    new Stats.Attributes(),
                    new Stats.Skills(),
                    itemCatalog.CreateWeapon(itemCatalog.UnknownWeaponId),
                    itemCatalog.CreateArmor(itemCatalog.UnknownArmorId));
            }

            return BuildClassConfig(definition);
        }

        public ClassConfig Resolve(string? id)
        {
            if (TryResolve(id, out ClassConfig config))
            {
                return config;
            }

            return GetDefault();
        }

        public bool TryResolve(string? id, out ClassConfig config)
        {
            config = null!;
            if (!string.IsNullOrWhiteSpace(id)
                && definitions.TryGetValue(id, out ClassDefinition? definition)
                && definition != null)
            {
                config = BuildClassConfig(definition);
                return true;
            }

            return false;
        }

        private ClassConfig BuildClassConfig(ClassDefinition definition)
        {
            return new ClassConfig(
                definition.Name,
                definition.Attributes,
                definition.Skills,
                itemCatalog.CreateWeapon(definition.WeaponId),
                itemCatalog.CreateArmor(definition.ArmorId));
        }
    }
}
