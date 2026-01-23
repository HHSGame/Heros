namespace HHSGame.Core.Map
{
    public class Pathfinder(MapState mapState)
    {
        private readonly MapState mapState = mapState;

        public List<Coordinate> FindPath(Coordinate start, Coordinate end)
        {
            PriorityQueue<Coordinate, float> openSet = new();
            HashSet<Coordinate> closedSet = [];
            Dictionary<Coordinate, Coordinate> cameFrom = [];
            Dictionary<Coordinate, float> gScore = [];
            Dictionary<Coordinate, float> fScore = [];

            gScore[start] = 0;
            fScore[start] = HeuristicCostEstimate(start, end);
            openSet.Enqueue(start, fScore[start]);

            while (openSet.Count > 0)
            {
                Coordinate current = openSet.Dequeue();

                if (current == end)
                {
                    return ReconstructPath(cameFrom, current);
                }

                closedSet.Add(current);

                foreach ((Coordinate neighbor, float stepCost) in GetNeighbors(current))
                {
                    if (closedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    float tentativeGScore = gScore[current] + stepCost;

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
            List<Coordinate> path = [current];
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }
            return path;
        }

        private List<(Coordinate Neighbor, float Cost)> GetNeighbors(Coordinate pos)
        {
            List<(Coordinate Neighbor, float Cost)> neighbors = [];

            // Check four directions
            if (mapState.IsWalkable(pos.X - 1, pos.Y))
            {
                neighbors.Add((new Coordinate(pos.X - 1, pos.Y), 1f));
            }

            if (mapState.IsWalkable(pos.X + 1, pos.Y))
            {
                neighbors.Add((new Coordinate(pos.X + 1, pos.Y), 1f));
            }

            if (mapState.IsWalkable(pos.X, pos.Y - 1))
            {
                neighbors.Add((new Coordinate(pos.X, pos.Y - 1), 1f));
            }

            if (mapState.IsWalkable(pos.X, pos.Y + 1))
            {
                neighbors.Add((new Coordinate(pos.X, pos.Y + 1), 1f));
            }

            if (mapState.IsWalkable(pos.X - 1, pos.Y - 1))
            {
                neighbors.Add((new Coordinate(pos.X - 1, pos.Y - 1), 1.5f));
            }

            if (mapState.IsWalkable(pos.X + 1, pos.Y - 1))
            {
                neighbors.Add((new Coordinate(pos.X + 1, pos.Y - 1), 1.5f));
            }

            if (mapState.IsWalkable(pos.X - 1, pos.Y + 1))
            {
                neighbors.Add((new Coordinate(pos.X - 1, pos.Y + 1), 1.5f));
            }

            if (mapState.IsWalkable(pos.X + 1, pos.Y + 1))
            {
                neighbors.Add((new Coordinate(pos.X + 1, pos.Y + 1), 1.5f));
            }

            return neighbors;
        }

        private static float HeuristicCostEstimate(Coordinate a, Coordinate b)
        {
            int dx = Math.Abs(a.X - b.X);
            int dy = Math.Abs(a.Y - b.Y);
            int diagonal = Math.Min(dx, dy);
            int straight = Math.Max(dx, dy) - diagonal;
            return (diagonal * 1.5f) + straight;
        }
    }
}
