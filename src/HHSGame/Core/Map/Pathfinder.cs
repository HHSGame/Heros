using System.Collections.Generic;

namespace HHSGame.Core.Map
{
    public class Pathfinder(MapState mapState)
    {
        private readonly MapState mapState = mapState;

        public List<Coordinate> FindPath(Coordinate start, Coordinate end)
        {
            var openSet = new PriorityQueue<Coordinate, float>();
            var closedSet = new HashSet<Coordinate>();
            var cameFrom = new Dictionary<Coordinate, Coordinate>();
            var gScore = new Dictionary<Coordinate, float>();
            var fScore = new Dictionary<Coordinate, float>();

            gScore[start] = 0;
            fScore[start] = HeuristicCostEstimate(start, end);
            openSet.Enqueue(start, fScore[start]);

            while (openSet.Count > 0)
            {
                var current = openSet.Dequeue();

                if (current == end)
                {
                    return ReconstructPath(cameFrom, current);
                }

                closedSet.Add(current);

                foreach (var neighbor in GetNeighbors(current))
                {
                    if (closedSet.Contains(neighbor))
                        continue;

                    var tentativeGScore = gScore[current] + 1;

                    if (!gScore.TryGetValue(neighbor, out float value) || tentativeGScore < value)
                    {
                        cameFrom[neighbor] = current;
                        value = tentativeGScore;
                        gScore[neighbor] = value;
                        fScore[neighbor] = tentativeGScore + HeuristicCostEstimate(neighbor, end);

                        if (!openSet.UnorderedItems.Any(n => n.Element == neighbor))
                        {
                            openSet.Enqueue(neighbor, fScore[neighbor]);
                        }
                    }
                }
            }

            return []; // No path found
        }

        private static List<Coordinate> ReconstructPath(Dictionary<Coordinate, Coordinate> cameFrom, Coordinate current)
        {
            var path = new List<Coordinate> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }
            return path;
        }

        private List<Coordinate> GetNeighbors(Coordinate pos)
        {
            var neighbors = new List<Coordinate>();

            // Check four directions
            if (mapState.IsWalkable(pos.X - 1, pos.Y)) neighbors.Add(new Coordinate(pos.X - 1, pos.Y));
            if (mapState.IsWalkable(pos.X + 1, pos.Y)) neighbors.Add(new Coordinate(pos.X + 1, pos.Y));
            if (mapState.IsWalkable(pos.X, pos.Y - 1)) neighbors.Add(new Coordinate(pos.X, pos.Y - 1));
            if (mapState.IsWalkable(pos.X, pos.Y + 1)) neighbors.Add(new Coordinate(pos.X, pos.Y + 1));

            return neighbors;
        }

        private static float HeuristicCostEstimate(Coordinate a, Coordinate b)
        {
            // Manhattan distance
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }
    }
}
