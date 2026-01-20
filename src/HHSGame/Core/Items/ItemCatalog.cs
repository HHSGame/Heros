namespace HHSGame.Core.Items
{
    public static class ItemCatalog
    {
        public static bool TryCreate(string id, out Item item)
        {
            item = null!;
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            string normalized = id.Trim();
            switch (normalized)
            {
                case "HealthPotion":
                    item = new HealthPotion(10);
                    return true;
                default:
                    return false;
            }
        }
    }
}
