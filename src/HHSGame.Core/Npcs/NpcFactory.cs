using HHSGame.Core;
using HHSGame.Core.Items;

namespace HHSGame.Core.Npcs
{
    public sealed class NpcFactory(ItemCatalog itemCatalog)
    {
        public List<Npc> CreateNpcs(IEnumerable<NpcSpawn> spawns)
        {
            List<Npc> result = [];
            foreach (NpcSpawn spawn in spawns)
            {
                Npc npc = new(
                    spawn.Id,
                    spawn.Name,
                    spawn.Glyph,
                    spawn.Attribute,
                    spawn.Position.X,
                    spawn.Position.Y,
                    spawn.Attributes,
                    spawn.Skills,
                    itemCatalog,
                    spawn.DialogueId);

                foreach (string itemId in spawn.StartingItems)
                {
                    if (itemCatalog.TryCreateItem(itemId, out Item item))
                    {
                        npc.Inventory.ObservableItems.Add(item);
                    }
                }

                result.Add(npc);
            }

            return result;
        }
    }
}
