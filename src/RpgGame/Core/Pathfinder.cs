using System.Collections.Generic;

namespace RpgGame.Core
{
    public class Pathfinder
    {
        private readonly GameWorld _world;

        public Pathfinder(GameWorld world)
        {
            _world = world;
        }

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

                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current.Position;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = tentativeGScore + HeuristicCostEstimate(neighbor, end);
                        
                        if (!openSet.UnorderedItems.Any(n => n.Element.Position == neighbor))
                        {
                            openSet.Enqueue(new Node(neighbor.x, neighbor.y), fScore[neighbor]);
                        }
                    }
                }
            }

            return new List<(int x, int y)>(); // No path found
        }

        private List<(int x, int y)> ReconstructPath(Dictionary<(int x, int y), (int x, int y)> cameFrom, (int x, int y) current)
        {
            var path = new List<(int x, int y)> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }
            return path;
        }

        private IEnumerable<(int x, int y)> GetNeighbors((int x, int y) pos)
        {
            var neighbors = new List<(int x, int y)>();
            
            // Check four directions
            if (_world.IsWalkable(pos.x - 1, pos.y)) neighbors.Add((pos.x - 1, pos.y));
            if (_world.IsWalkable(pos.x + 1, pos.y)) neighbors.Add((pos.x + 1, pos.y));
            if (_world.IsWalkable(pos.x, pos.y - 1)) neighbors.Add((pos.x, pos.y - 1));
            if (_world.IsWalkable(pos.x, pos.y + 1)) neighbors.Add((pos.x, pos.y + 1));

            return neighbors;
        }

        private float HeuristicCostEstimate((int x, int y) a, (int x, int y) b)
        {
            // Manhattan distance
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }

        private record Node(int x, int y)
        {
            public (int x, int y) Position => (x, y);
        }
    }
}
