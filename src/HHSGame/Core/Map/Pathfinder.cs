using System.Collections.Generic;

namespace HHSGame.Core.Map
{
    public class Pathfinder(MapState mapState)
    {
        private readonly MapState mapState = mapState;

        public List<(int x, int y)> FindPath((int x, int y) start, (int x, int y) end)
        {
            var openSet = new PriorityQueue<Node, float>();
            var closedSet = new HashSet<(int x, int y)>();
            var cameFrom = new Dictionary<(int x, int y), (int x, int y)>();
            var gScore = new Dictionary<(int x, int y), float>();
            var fScore = new Dictionary<(int x, int y), float>();

            gScore[start] = 0;
            fScore[start] = HeuristicCostEstimate(start, end);
            openSet.Enqueue(new Node(start.x, start.y), fScore[start]);

            while (openSet.Count > 0)
            {
                var current = openSet.Dequeue();

                if (current.Position == end)
                {
                    return ReconstructPath(cameFrom, current.Position);
                }

                closedSet.Add(current.Position);

                foreach (var neighbor in GetNeighbors(current.Position))
                {
                    if (closedSet.Contains(neighbor))
                        continue;

                    var tentativeGScore = gScore[current.Position] + 1;

                    if (!gScore.TryGetValue(neighbor, out float value) || tentativeGScore < value)
                    {
                        cameFrom[neighbor] = current.Position;
                        value = tentativeGScore;
                        gScore[neighbor] = value;
                        fScore[neighbor] = tentativeGScore + HeuristicCostEstimate(neighbor, end);

                        if (!openSet.UnorderedItems.Any(n => n.Element.Position == neighbor))
                        {
                            openSet.Enqueue(new Node(neighbor.x, neighbor.y), fScore[neighbor]);
                        }
                    }
                }
            }

            return []; // No path found
        }

        private static List<(int x, int y)> ReconstructPath(Dictionary<(int x, int y), (int x, int y)> cameFrom, (int x, int y) current)
        {
            var path = new List<(int x, int y)> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }
            return path;
        }

        private List<(int x, int y)> GetNeighbors((int x, int y) pos)
        {
            var neighbors = new List<(int x, int y)>();

            // Check four directions
            if (mapState.IsWalkable(pos.x - 1, pos.y)) neighbors.Add((pos.x - 1, pos.y));
            if (mapState.IsWalkable(pos.x + 1, pos.y)) neighbors.Add((pos.x + 1, pos.y));
            if (mapState.IsWalkable(pos.x, pos.y - 1)) neighbors.Add((pos.x, pos.y - 1));
            if (mapState.IsWalkable(pos.x, pos.y + 1)) neighbors.Add((pos.x, pos.y + 1));

            return neighbors;
        }

        private static float HeuristicCostEstimate((int x, int y) a, (int x, int y) b)
        {
            // Manhattan distance
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }

        private sealed record Node(int X, int Y)
        {
            public (int x, int y) Position => (X, Y);
        }
    }
}
