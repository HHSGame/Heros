using HHSGame.Core.Combat;
using HHSGame.Core.Items;
using HHSGame.Core.Stats;
using Terminal.Gui.Drawing;

namespace HHSGame.Core
{
    public class Npc : IGameActor, ICombatant, ITrader
    {
        private readonly ItemCatalog itemCatalog;
        private Weapon? equippedWeapon;
        private Armor? equippedArmor;

        public Npc(
            string id,
            string name,
            char glyph,
            Terminal.Gui.Drawing.Attribute attribute,
            int x,
            int y,
            Attributes attributes,
            Skills skills,
            ItemCatalog itemCatalog,
            string dialogueId)
        {
            Id = id;
            Name = name;
            Glyph = glyph;
            Attribute = attribute;
            X = x;
            Y = y;
            Stats = new CharacterStats(attributes with { }, skills.Clone());
            Inventory = new InventoryManager();
            this.itemCatalog = itemCatalog;
            DialogueId = dialogueId;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public string Id { get; }
        public string DialogueId { get; }
        public string Name { get; }
        public char Glyph { get; }
        public Terminal.Gui.Drawing.Attribute Attribute { get; }
        public Coordinate Position => new(X, Y);

        public CharacterStats Stats { get; }
        public InventoryManager Inventory { get; }
        public int ArmorValue => equippedArmor?.ArmorValue ?? 0;
        public Weapon EquippedWeapon => equippedWeapon ?? itemCatalog.CreateWeapon(itemCatalog.UnknownWeaponId);
        public int EvasionBonus => Stats.EvasionBonus;

        public void EquipWeapon(Weapon weapon)
        {
            equippedWeapon = weapon;
        }

        public void EquipArmor(Armor armor)
        {
            equippedArmor = armor;
        }

        public void TakeDamage(int damage)
        {
            Stats.ApplyDamage(damage);
        }
    }
}
