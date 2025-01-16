namespace HHSGame.Core.Classes
{
    using Enemies;
    using Items;

    public abstract class AbstractClass
    {
        public string Name { get; set; } = "Unknown";
        public int BaseHealth { get; set; }
        public int BaseStrength { get; set; }
        public int BaseDefense { get; set; }
        public abstract void ApplyClassBonuses(Player player);
        public abstract void ApplyStartupEquipment(Player player);

        public virtual void ApplySecretSkill(Player player, Enemy enemy)
        {
            // Default implementation: no secret skill
        }
    }

    public class TypedClass : AbstractClass
    {

        public Weapon Weapon { get; set; } = new Weapon("Unknown", ItemRarity.Common, 0, 0, 0, 0);
        public Armor Armor { get; set; } = new Armor("Unknown", ItemRarity.Common, 0, 0, 0);

        public override void ApplyClassBonuses(Player player)
        {
            player.MaxHealth = BaseHealth;
            player.Health = BaseHealth;
            player.Strength = BaseStrength;
            player.Defense = BaseDefense;
        }
        public override void ApplyStartupEquipment(Player player)
        {
            player.EquipWeapon(Weapon);
            player.EquipArmor(Armor);
        }
    }

    public record ClassConfig(string Name, int BaseHealth, int BaseStrength, int BaseDefense, Weapon startupWeapon, Armor startupArmor)
    {

        public TypedClass ToClass()
        {
            return new TypedClass()
            {
                Name = Name,
                BaseHealth = BaseHealth,
                BaseStrength = BaseStrength,
                BaseDefense = BaseDefense,
                Weapon = startupWeapon,
                Armor = startupArmor
            };
        }
    }

    public class Weapons
    {
        public static readonly Weapon UnknownWeapon = new("Unknown", ItemRarity.Common, 0, 0, 0, 0);
        public static readonly Weapon PocketKnife = new("Pocket Knife", ItemRarity.Common, 50, 0.4f, 4, 0.7f);
        public static readonly Weapon Wrench = new("Wrench", ItemRarity.Common, 90, 1.5f, 7, 1.1f);
        public static readonly Weapon Stethoscope = new("Stethoscope", ItemRarity.Common, 60, 0.7f, 4, 0.8f);
        public static readonly Weapon Scalpel = new("Scalpel", ItemRarity.Common, 70, 0.7f, 5, 0.9f);
        public static readonly Weapon MechanicWrench = new("Wrench", ItemRarity.Common, 80, 1.2f, 8, 1.0f);
        public static readonly Weapon Beaker = new("Beaker", ItemRarity.Common, 60, 0.6f, 5, 0.8f);
        public static readonly Weapon Sword = new("Sword", ItemRarity.Common, 100, 2.0f, 10, 1.0f);
        public static readonly Weapon Rifle = new("Rifle", ItemRarity.Common, 120, 1.5f, 12, 1.2f);
        public static readonly Weapon TankCannon = new("Tank Cannon", ItemRarity.Rare, 200, 3.0f, 20, 1.5f);
        public static readonly Weapon Syringe = new("Syringe", ItemRarity.Common, 50, 0.5f, 5, 0.5f);
        public static readonly Weapon SniperRifle = new("Sniper Rifle", ItemRarity.Rare, 180, 2.5f, 25, 1.8f);
        public static readonly Weapon ArtilleryCannon = new("Artillery Cannon", ItemRarity.Rare, 150, 2.5f, 15, 1.5f);
        public static readonly Weapon Mine = new("Mine", ItemRarity.Rare, 80, 1.0f, 12, 1.2f);
        public static readonly Weapon Plane = new("Plane", ItemRarity.Epic, 200, 3.0f, 20, 1.8f);
        public static readonly Weapon ScoutRifle = new("Scout Rifle", ItemRarity.Common, 100, 1.0f, 10, 1.0f);
        public static readonly Weapon Pickaxe = new("Pickaxe", ItemRarity.Common, 120, 2.5f, 8, 1.2f);
        public static readonly Weapon Cleaver = new("Cleaver", ItemRarity.Common, 90, 1.8f, 7, 1.1f);
        public static readonly Weapon Knife = new("Knife", ItemRarity.Common, 70, 0.8f, 5, 0.9f);
        public static readonly Weapon Hammer = new("Hammer", ItemRarity.Common, 110, 3.0f, 9, 1.3f);
        public static readonly Weapon Pitchfork = new("Pitchfork", ItemRarity.Common, 80, 1.5f, 6, 1.0f);
        public static readonly Weapon WoodenClub = new("Wooden Club", ItemRarity.Common, 60, 1.2f, 4, 0.8f);
        public static readonly Weapon FishingRod = new("Fishing Rod", ItemRarity.Common, 60, 0.6f, 4, 0.8f);
        public static readonly Weapon Axe = new("Axe", ItemRarity.Common, 100, 2.0f, 8, 1.1f);
        public static readonly Weapon Scissors = new("Scissors", ItemRarity.Common, 50, 0.3f, 3, 0.7f);
        public static readonly Weapon RollingPin = new("Rolling Pin", ItemRarity.Common, 60, 0.7f, 4, 0.9f);
        public static readonly Weapon Bow = new("Bow", ItemRarity.Common, 90, 1.5f, 7, 1.2f);
        public static readonly Weapon CarpenterHammer = new("Hammer", ItemRarity.Common, 100, 2.0f, 8, 1.1f);
        public static readonly Weapon Dagger = new("Dagger", ItemRarity.Common, 60, 0.5f, 5, 0.8f);
        public static readonly Weapon Quill = new("Quill", ItemRarity.Common, 50, 0.3f, 3, 0.7f);
        public static readonly Weapon Potion = new("Potion", ItemRarity.Common, 70, 0.7f, 4, 0.9f);
        public static readonly Weapon LetterOpener = new("Letter Opener", ItemRarity.Common, 50, 0.4f, 3, 0.7f);
        public static readonly Weapon Chalk = new("Chalk", ItemRarity.Common, 40, 0.3f, 2, 0.6f);
        public static readonly Weapon Book = new("Book", ItemRarity.Common, 30, 0.2f, 1, 0.5f);
        public static readonly Weapon Baton = new("Baton", ItemRarity.Common, 70, 1.0f, 6, 0.9f);
        public static readonly Weapon FirefighterAxe = new("Axe", ItemRarity.Common, 100, 1.0f, 10, 0.8f);

    }

    public class Armors
    {
        // Armor constants
        public static readonly Armor UnknownArmor = new("Unknown", ItemRarity.Common, 0, 0, 0);
        public static readonly Armor Disguise = new("Disguise", ItemRarity.Common, 80, 0.6f, 2);
        public static readonly Armor HardHat = new("Hard Hat", ItemRarity.Common, 120, 1.0f, 3);
        public static readonly Armor Scrubs = new("Scrubs", ItemRarity.Common, 90, 0.7f, 2);
        public static readonly Armor WhiteCoat = new("White Coat", ItemRarity.Common, 100, 1.0f, 2);
        public static readonly Armor Overalls = new("Overalls", ItemRarity.Common, 110, 1.5f, 4);
        public static readonly Armor LabCoat = new("Lab Coat", ItemRarity.Common, 90, 1.0f, 3);
        public static readonly Armor Shield = new("Shield", ItemRarity.Common, 150, 3.0f, 5);
        public static readonly Armor Helmet = new("Helmet", ItemRarity.Common, 100, 2.0f, 3);
        public static readonly Armor TankArmor = new("Tank Armor", ItemRarity.Rare, 250, 4.0f, 10);
        public static readonly Armor Bandage = new("Bandage", ItemRarity.Common, 30, 1.0f, 2);
        public static readonly Armor Binoculars = new("Binoculars", ItemRarity.Rare, 120, 2.0f, 4);
        public static readonly Armor ArtilleryVest = new("Artillery Vest", ItemRarity.Rare, 180, 3.0f, 6);
        public static readonly Armor MineDetector = new("Mine Detector", ItemRarity.Rare, 120, 2.0f, 4);
        public static readonly Armor PilotHelmet = new("Pilot Helmet", ItemRarity.Epic, 150, 2.5f, 5);
        public static readonly Armor ScoutBinoculars = new("Scout Binoculars", ItemRarity.Common, 80, 1.5f, 3);
        public static readonly Armor MinerHelmet = new("Helmet", ItemRarity.Common, 180, 1.5f, 4);
        public static readonly Armor Apron = new("Apron", ItemRarity.Common, 130, 2.2f, 3);
        public static readonly Armor OvenMitts = new("Oven Mitts", ItemRarity.Common, 100, 1.0f, 2);
        public static readonly Armor LeatherApron = new("Leather Apron", ItemRarity.Common, 160, 2.5f, 4);
        public static readonly Armor StrawHat = new("Straw Hat", ItemRarity.Common, 120, 0.8f, 3);
        public static readonly Armor RaggedCloth = new("Ragged Cloth", ItemRarity.Common, 100, 0.7f, 1);
        public static readonly Armor Raincoat = new("Raincoat", ItemRarity.Common, 90, 1.2f, 2);
        public static readonly Armor FlannelShirt = new("Flannel Shirt", ItemRarity.Common, 140, 1.5f, 3);
        public static readonly Armor MeasuringTape = new("Measuring Tape", ItemRarity.Common, 70, 0.5f, 1);
        public static readonly Armor OvenGloves = new("Oven Gloves", ItemRarity.Common, 90, 1.0f, 2);
        public static readonly Armor LeatherArmor = new("Leather Armor", ItemRarity.Common, 130, 1.8f, 3);
        public static readonly Armor WorkGloves = new("Work Gloves", ItemRarity.Common, 120, 1.0f, 2);
        public static readonly Armor SilkRobe = new("Silk Robe", ItemRarity.Common, 100, 0.8f, 2);
        public static readonly Armor Robe = new("Robe", ItemRarity.Common, 70, 0.5f, 1);
        public static readonly Armor Cloak = new("Cloak", ItemRarity.Common, 80, 0.6f, 2);
        public static readonly Armor AlchemistLabCoat = new("Lab Coat", ItemRarity.Common, 90, 1.0f, 2);
        public static readonly Armor Uniform = new("Uniform", ItemRarity.Common, 80, 0.6f, 2);
        public static readonly Armor Chalkboard = new("Chalkboard", ItemRarity.Common, 60, 0.5f, 1);
        public static readonly Armor Glasses = new("Glasses", ItemRarity.Common, 50, 0.4f, 1);
        public static readonly Armor PoliceUniform = new("Uniform", ItemRarity.Common, 100, 1.2f, 3);
        public static readonly Armor FirefighterUniform = new("Uniform", ItemRarity.Common, 100, 1.2f, 3);

    }

    public class Classes
    {
        public static readonly ClassConfig Unemployed = new("Unemployed", 10, 10, 10, Weapons.UnknownWeapon, Armors.UnknownArmor);
        public static readonly ClassConfig Spy = new("Spy", 70, 6, 3, Weapons.PocketKnife, Armors.Disguise);
        public static readonly ClassConfig Engineer = new("Engineer", 100, 10, 5, Weapons.Wrench, Armors.HardHat);
        public static readonly ClassConfig Nurse = new("Nurse", 80, 7, 3, Weapons.Stethoscope, Armors.Scrubs);
        public static readonly ClassConfig Doctor = new("Doctor", 70, 6, 2, Weapons.Scalpel, Armors.WhiteCoat);
        public static readonly ClassConfig Mechanic = new("Mechanic", 95, 12, 6, Weapons.MechanicWrench, Armors.Overalls);
        public static readonly ClassConfig Chemist = new("Chemist", 85, 8, 4, Weapons.Beaker, Armors.LabCoat);
        public static readonly ClassConfig Soldier = new("Soldier", 120, 15, 7, Weapons.Sword, Armors.Shield);
        public static readonly ClassConfig Infantry = new("Infantry", 100, 12, 5, Weapons.Rifle, Armors.Helmet);
        public static readonly ClassConfig Tanker = new("Tanker", 200, 20, 10, Weapons.TankCannon, Armors.TankArmor);
        public static readonly ClassConfig Medic = new("Medic", 80, 8, 3, Weapons.Syringe, Armors.Bandage);
        public static readonly ClassConfig Sniper = new("Sniper", 90, 18, 4, Weapons.SniperRifle, Armors.Binoculars);
        public static readonly ClassConfig ArtillerySoldier = new("Artillery Soldier", 110, 14, 6, Weapons.ArtilleryCannon, Armors.ArtilleryVest);
        public static readonly ClassConfig MineLayer = new("Mine Layer", 90, 10, 4, Weapons.Mine, Armors.MineDetector);
        public static readonly ClassConfig Pilot = new("Pilot", 100, 13, 5, Weapons.Plane, Armors.PilotHelmet);
        public static readonly ClassConfig Scout = new("Scout", 80, 10, 3, Weapons.ScoutRifle, Armors.ScoutBinoculars);
        public static readonly ClassConfig Miner = new("Miner", 100, 12, 6, Weapons.Pickaxe, Armors.MinerHelmet);
        public static readonly ClassConfig Butcher = new("Butcher", 90, 10, 5, Weapons.Cleaver, Armors.Apron);
        public static readonly ClassConfig Cook = new("Cook", 80, 8, 4, Weapons.Knife, Armors.OvenMitts);
        public static readonly ClassConfig Blacksmith = new("Blacksmith", 110, 14, 6, Weapons.Hammer, Armors.LeatherApron);
        public static readonly ClassConfig Farmer = new("Farmer", 95, 10, 5, Weapons.Pitchfork, Armors.StrawHat);
        public static readonly ClassConfig Peasant = new("Peasant", 100, 8, 6, Weapons.WoodenClub, Armors.RaggedCloth);
        public static readonly ClassConfig Fisherman = new("Fisherman", 85, 7, 3, Weapons.FishingRod, Armors.Raincoat);
        public static readonly ClassConfig Lumberjack = new("Lumberjack", 105, 13, 5, Weapons.Axe, Armors.FlannelShirt);
        public static readonly ClassConfig Tailor = new("Tailor", 75, 6, 3, Weapons.Scissors, Armors.MeasuringTape);
        public static readonly ClassConfig Baker = new("Baker", 80, 7, 3, Weapons.RollingPin, Armors.OvenGloves);
        public static readonly ClassConfig Hunter = new("Hunter", 95, 11, 4, Weapons.Bow, Armors.LeatherArmor);
        public static readonly ClassConfig Carpenter = new("Carpenter", 100, 10, 5, Weapons.CarpenterHammer, Armors.WorkGloves);
        public static readonly ClassConfig Merchant = new("Merchant", 85, 7, 3, Weapons.Dagger, Armors.SilkRobe);
        public static readonly ClassConfig Scholar = new("Scholar", 75, 6, 3, Weapons.Quill, Armors.Robe);
        public static readonly ClassConfig Thief = new("Thief", 80, 8, 4, Weapons.Dagger, Armors.Cloak);
        public static readonly ClassConfig Alchemist = new("Alchemist", 85, 7, 3, Weapons.Potion, Armors.AlchemistLabCoat);
        public static readonly ClassConfig Postman = new("Postman", 80, 7, 3, Weapons.LetterOpener, Armors.Uniform);
        public static readonly ClassConfig Teacher = new("Teacher", 75, 6, 3, Weapons.Chalk, Armors.Chalkboard);
        public static readonly ClassConfig Librarian = new("Librarian", 70, 5, 3, Weapons.Book, Armors.Glasses);
        public static readonly ClassConfig PoliceOfficer = new("Police Officer", 90, 9, 4, Weapons.Baton, Armors.PoliceUniform);
        public static readonly ClassConfig Firefighter = new("Firefighter", 100, 11, 5, Weapons.FirefighterAxe, Armors.FirefighterUniform);
    }
}
