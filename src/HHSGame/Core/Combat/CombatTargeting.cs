using HHSGame.Core.Enemies;
using HHSGame.Core.Items;
using HHSGame.Core.Map;

namespace HHSGame.Core.Combat
{
    public static class CombatTargeting
    {
        public static bool IsWithinRange(Coordinate origin, Coordinate target, int range)
        {
            if (range <= 0)
            {
                return false;
            }

            int dx = target.X - origin.X;
            int dy = target.Y - origin.Y;
            return (dx * dx + dy * dy) <= range * range;
        }

        public static bool CanAttack(MapState mapState, Coordinate origin, Coordinate target, Weapon weapon)
        {
            if (!IsWithinRange(origin, target, weapon.Range))
            {
                return false;
            }

            if (WeaponRules.RequiresLineOfSight(weapon) && !HasLineOfSight(mapState, origin, target))
            {
                return false;
            }

            return true;
        }

        public static bool HasLineOfSight(MapState mapState, Coordinate origin, Coordinate target)
        {
            int x0 = origin.X;
            int y0 = origin.Y;
            int x1 = target.X;
            int y1 = target.Y;

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (x0 != x1 || y0 != y1)
            {
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }

                if (x0 == x1 && y0 == y1)
                {
                    break;
                }

                if (mapState.IsProjectileBlocking(x0, y0))
                {
                    return false;
                }
            }

            return true;
        }

        public static List<Enemy> GetTargetsInRange(MapState mapState, Coordinate origin, IEnumerable<Enemy> enemies, Weapon weapon)
        {
            List<Enemy> targets = [];
            foreach (Enemy enemy in enemies)
            {
                if (!mapState.IsVisible(enemy.Position))
                {
                    continue;
                }

                if (CanAttack(mapState, origin, enemy.Position, weapon))
                {
                    targets.Add(enemy);
                }
            }

            targets.Sort((a, b) =>
            {
                int distanceA = GetDistanceSquared(origin, a.Position);
                int distanceB = GetDistanceSquared(origin, b.Position);
                int comparison = distanceA.CompareTo(distanceB);
                if (comparison != 0)
                {
                    return comparison;
                }

                return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
            });

            return targets;
        }

        public static List<Coordinate> GetRangeCells(MapState mapState, Coordinate origin, Weapon weapon)
        {
            List<Coordinate> cells = [];
            int range = weapon.Range;
            if (range <= 0)
            {
                return cells;
            }

            int rangeSquared = range * range;
            for (int dx = -range; dx <= range; dx++)
            {
                for (int dy = -range; dy <= range; dy++)
                {
                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    if (dx * dx + dy * dy > rangeSquared)
                    {
                        continue;
                    }

                    Coordinate target = origin.Target(dx, dy);
                    if (!mapState.IsInBounds(target))
                    {
                        continue;
                    }

                    if (!mapState.IsVisible(target))
                    {
                        continue;
                    }

                    if (WeaponRules.RequiresLineOfSight(weapon) && !HasLineOfSight(mapState, origin, target))
                    {
                        continue;
                    }

                    cells.Add(target);
                }
            }

            return cells;
        }

        private static int GetDistanceSquared(Coordinate origin, Coordinate target)
        {
            int dx = target.X - origin.X;
            int dy = target.Y - origin.Y;
            return dx * dx + dy * dy;
        }
    }
}
