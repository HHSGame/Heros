namespace RpgGame.Core {

    public abstract class Item
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Name { get; protected set; }
        
        public Item(string name)
        {
            Name = name;
        }

        public abstract void Use(Player player);
    }

    public class HealthPotion : Item
    {
        private int _healAmount;

        public HealthPotion(int healAmount) : base("Health Potion")
        {
            _healAmount = healAmount;
        }

        public override void Use(Player player)
        {
            player.Heal(_healAmount);
        }
    }
}