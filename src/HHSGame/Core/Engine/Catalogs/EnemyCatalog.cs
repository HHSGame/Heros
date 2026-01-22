using HHSGame.UI;
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
                return ColorPresets.Enemies.Gangster;
            }

            return key.Trim().ToLowerInvariant() switch
            {
                "default" => ColorPresets.Enemies.Gangster,
                "gangster" => ColorPresets.Enemies.Gangster,
                "bandit" => ColorPresets.Enemies.Bandit,
                "banditleader" => ColorPresets.Enemies.BanditLeader,
                "thug" => ColorPresets.Enemies.Thug,
                "soldier" => ColorPresets.Enemies.Soldier,
                "sniper" => ColorPresets.Enemies.Sniper,
                _ => ColorPresets.Enemies.Gangster
            };
        }
    }
}
