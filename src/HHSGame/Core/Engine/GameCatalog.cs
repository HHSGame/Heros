using HHSGame.Core.Engine.Catalogs;
using HHSGame.Core.Items;

namespace HHSGame.Core.Engine
{
    public sealed class GameCatalog(ItemCatalog itemCatalog, ClassCatalog classCatalog, EnemyCatalog enemyCatalog)
    {
        public ItemCatalog ItemCatalog { get; } = itemCatalog;
        public ClassCatalog ClassCatalog { get; } = classCatalog;
        public EnemyCatalog EnemyCatalog { get; } = enemyCatalog;
    }
}
