using HHSGame.Core.Combat;
using HHSGame.Core.Items;
using HHSGame.Core.Map;
using HHSGame.UI;
using HHSGame.Core.Rendering;

namespace HHSGame.Core
{
    [TestClass]
    public class CombatTargetingTests
    {
        [TestMethod]
        public void LineWeaponsAreBlockedByWalls()
        {
            MapState mapState = BuildMap(".#..");
            Weapon weapon = new("Bow", "Bow", ItemRarity.Common, 0, 0, 5, 0, 4, WeaponType.RangedAimed, WeaponTrajectory.Line);

            bool canAttack = CombatTargeting.CanAttack(mapState, new Coordinate(0, 0), new Coordinate(3, 0), weapon);

            Assert.IsFalse(canAttack);
        }

        [TestMethod]
        public void LineWeaponsCanShootThroughWater()
        {
            MapState mapState = BuildMap(".~..");
            Weapon weapon = new("Bow", "Bow", ItemRarity.Common, 0, 0, 5, 0, 4, WeaponType.RangedAimed, WeaponTrajectory.Line);

            bool canAttack = CombatTargeting.CanAttack(mapState, new Coordinate(0, 0), new Coordinate(3, 0), weapon);

            Assert.IsTrue(canAttack);
        }

        [TestMethod]
        public void ArcWeaponsIgnoreLineOfSight()
        {
            MapState mapState = BuildMap(".#..");
            Weapon weapon = new("ArcaneBolt", "Arcane Bolt", ItemRarity.Common, 0, 0, 5, 0, 4, WeaponType.RangedAimed, WeaponTrajectory.Arc);

            bool canAttack = CombatTargeting.CanAttack(mapState, new Coordinate(0, 0), new Coordinate(3, 0), weapon);

            Assert.IsTrue(canAttack);
        }

        private static MapState BuildMap(string row)
        {
            Cell[,] map = new Cell[1, row.Length];
            for (int x = 0; x < row.Length; x++)
            {
                char terrain = row[x];
                map[0, x] = new Cell
                {
                    Character = terrain,
                    Attribute = TilePresets.GetTerrainColor(terrain)
                };
            }

            MapData data = new(map, []);
            MapState state = new();
            state.Init(data);
            return state;
        }
    }
}
