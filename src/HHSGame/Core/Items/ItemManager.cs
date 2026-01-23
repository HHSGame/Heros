using HHSGame.UI;
using HHSGame.Core;

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
            return Loot.Where(item => item.X == x && item.Y == y && !item.IsHidden).ToList();
        }

        public List<Item> GetItemsAt(int x, int y, bool includeHidden)
        {
            return Loot.Where(item => item.X == x && item.Y == y && (includeHidden || !item.IsHidden)).ToList();
        }

        public List<Item> GetItemsInRadius(Coordinate origin, int radius, bool includeHidden)
        {
            List<Item> result = [];
            foreach (Item item in Loot)
            {
                if (!includeHidden && item.IsHidden)
                {
                    continue;
                }

                int dx = Math.Abs(item.X - origin.X);
                int dy = Math.Abs(item.Y - origin.Y);
                if (dx <= radius && dy <= radius)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        public List<Item> RevealHiddenItems(Coordinate origin, int radius)
        {
            List<Item> revealed = [];
            foreach (Item item in Loot)
            {
                if (!item.IsHidden)
                {
                    continue;
                }

                int dx = Math.Abs(item.X - origin.X);
                int dy = Math.Abs(item.Y - origin.Y);
                if (dx <= radius && dy <= radius)
                {
                    item.IsHidden = false;
                    revealed.Add(item);
                }
            }

            return revealed;
        }

        public void RemoveItemsAt(int x, int y, bool includeHidden = false)
        {
            Loot.RemoveAll(item => item.X == x && item.Y == y && (includeHidden || !item.IsHidden));
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
                if (item.IsHidden)
                {
                    continue;
                }

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
