using HHSGame.Core.Rendering;
using Attribute = Terminal.Gui.Drawing.Attribute;

namespace HHSGame.Core.Engine.Catalogs
{
    public sealed class EnemyCatalog
    {
        private readonly Dictionary<string, EnemyDefinition> definitions;

        public EnemyCatalog(IEnumerable<EnemyDefinition> definitions)
        {
            this.definitions = definitions.ToDictionary(def => def.Id, StringComparer.OrdinalIgnoreCase);
        }

        public EnemyDefinition GetDefinition(string id)
        {
            if (!TryGetDefinition(id, out EnemyDefinition definition))
            {
                throw new InvalidDataException($"Enemy definition not found for '{id}'.");
            }

            return definition;
        }

        public bool TryGetDefinition(string id, out EnemyDefinition definition)
        {
            definition = null!;
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            if (definitions.TryGetValue(id.Trim(), out EnemyDefinition? found) && found != null)
            {
                definition = found;
                return true;
            }

            return false;
        }

        public static Attribute ResolveEnemyColor(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return ColorPresets.Enemies.Occupier;
            }

            return key.Trim().ToLowerInvariant() switch
            {
                "default" => ColorPresets.Enemies.Occupier,
                "occupier" => ColorPresets.Enemies.Occupier,
                "officer" => ColorPresets.Enemies.Officer,
                "gestapo" => ColorPresets.Enemies.Gestapo,
                "collaborator" => ColorPresets.Enemies.Collaborator,
                "informer" => ColorPresets.Enemies.Informer,
                "bandit" => ColorPresets.Enemies.Bandit,
                "banditboss" => ColorPresets.Enemies.BanditBoss,
                "deserter" => ColorPresets.Enemies.Deserter,
                "mgnest" => ColorPresets.Enemies.Sniper,
                "sniper" => ColorPresets.Enemies.Sniper,
                "patrol" => ColorPresets.Enemies.Patrol,
                "medic" => ColorPresets.Enemies.Medic,
                "dog" => ColorPresets.Enemies.Dog,
                "tank" => ColorPresets.Enemies.Tank,
                "turncoat" => ColorPresets.Enemies.Turncoat,
                _ => ColorPresets.Enemies.Occupier
            };
        }
    }
}
