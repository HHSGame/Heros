using HHSGame.UI;

namespace HHSGame.Core.Items
{
    public class ItemManager : IDrawable
    {
        public List<Item> Loot { get; }
        public int Count => Loot.Count;

        public ItemManager()
        {
            Loot = [];
        }

        public List<Item> GetItemsAt(int x, int y)
        {
            return Loot.Where(item => item.X == x && item.Y == y).ToList();
        }

        public void RemoveItemsAt(int x, int y)
        {
            Loot.RemoveAll(item => item.X == x && item.Y == y);
        }

        public void AddLoot(Item item)
        {
            Events.RaiseGameMessage($"Dropped {item.Name} at ({item.X}, {item.Y}).");
            Loot.Add(item);
        }

        public void Draw(IDrawingContext ctx)
        {
            Viewport viewport = ctx.Viewport;

            foreach (Item item in Loot)
            {
                if (viewport.Contains((item.X, item.Y)))
                {
                    ctx.DrawAt((item.X, item.Y), '*');
                }
            }
        }

        public void AddItem(Item item)
        {
            Loot.Add(item);
        }
    }
}