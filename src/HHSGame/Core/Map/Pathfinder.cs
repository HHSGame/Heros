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

                foreach (Coordinate neighbor in GetNeighbors(current))
                {
                    if (closedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    float tentativeGScore = gScore[current] + 1;

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

        private List<Coordinate> GetNeighbors(Coordinate pos)
        {
            List<Coordinate> neighbors = [];

            // Check four directions
            if (mapState.IsWalkable(pos.X - 1, pos.Y))
            {
                neighbors.Add(new Coordinate(pos.X - 1, pos.Y));
            }

            if (mapState.IsWalkable(pos.X + 1, pos.Y))
            {
                neighbors.Add(new Coordinate(pos.X + 1, pos.Y));
            }

            if (mapState.IsWalkable(pos.X, pos.Y - 1))
            {
                neighbors.Add(new Coordinate(pos.X, pos.Y - 1));
            }

            if (mapState.IsWalkable(pos.X, pos.Y + 1))
            {
                neighbors.Add(new Coordinate(pos.X, pos.Y + 1));
            }

            return neighbors;
        }

        private static float HeuristicCostEstimate(Coordinate a, Coordinate b)
        {
            // Manhattan distance
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }
    }
}
