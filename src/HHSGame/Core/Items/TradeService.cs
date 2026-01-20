using HHSGame.Core.Stats;

namespace HHSGame.Core.Items
{
    public interface ITrader
    {
        InventoryManager Inventory { get; }
        CharacterStats Stats { get; }
    }

    public static class TradeService
    {
        public static int GetBuyPrice(Item item, int barterSkill)
        {
            double price = item.Value * (1.5 - (barterSkill * 0.02));
            return Math.Max(1, (int)Math.Round(price));
        }

        public static int GetSellPrice(Item item, int barterSkill)
        {
            double price = item.Value * (0.3 + (barterSkill * 0.02));
            return Math.Max(1, (int)Math.Round(price));
        }

        public static bool TryBuy(ITrader buyer, ITrader seller, Item item)
        {
            int barter = buyer.Stats.Skills.GetValue(SkillType.Barter, buyer.Stats.Attributes);
            int price = GetBuyPrice(item, barter);
            if (!seller.Inventory.ObservableItems.Contains(item))
            {
                return false;
            }

            if (!buyer.Inventory.TrySpendCurrency(price))
            {
                return false;
            }

            seller.Inventory.AddCurrency(price);
            seller.Inventory.RemoveItem(item);
            buyer.Inventory.AddItem(item);
            return true;
        }

        public static bool TrySell(ITrader seller, ITrader buyer, Item item)
        {
            int barter = seller.Stats.Skills.GetValue(SkillType.Barter, seller.Stats.Attributes);
            int price = GetSellPrice(item, barter);
            if (!seller.Inventory.ObservableItems.Contains(item))
            {
                return false;
            }

            if (!buyer.Inventory.TrySpendCurrency(price))
            {
                return false;
            }

            seller.Inventory.AddCurrency(price);
            seller.Inventory.RemoveItem(item);
            buyer.Inventory.AddItem(item);
            return true;
        }
    }
}
