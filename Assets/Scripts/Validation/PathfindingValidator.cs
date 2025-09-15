using System;
using System.Collections.Generic;
using TilePathGame.Tiles;
using UnityEngine;

namespace TilePathGame.Validation
{
    public static class PathfindingValidator
    {
        public struct PathfindingResult
        {
            public bool isPathPossible;
            public bool isCurrentlyConnected;
            public List<string> issues;
            public int estimatedMovesNeeded;

            public PathfindingResult(bool pathPossible, bool connected, List<string> issues = null, int moves = -1)
            {
                isPathPossible = pathPossible;
                isCurrentlyConnected = connected;
                this.issues = issues ?? new List<string>();
                estimatedMovesNeeded = moves;
            }
        }

        public static PathfindingResult IsPathPossible(
            Tuple<Vector2Int, TileInstance> start,
            Tuple<Vector2Int, TileInstance> goal,
            Dictionary<Vector2Int, TileInstance> grid)
        {
            Queue<Tuple<Vector2Int, TileInstance>> queue = new();
            var visited = new HashSet<Vector2Int>();
            visited.Add(start.Item1);
            queue.Enqueue(start);
            Vector2Int[] dirs =
            {
                new Vector2Int(-1, 0), //up
                new Vector2Int(0, 1), //right
                new Vector2Int(1, 0), // down
                new Vector2Int(0, -1), // left
            };

            while (queue.Count > 0)
            {
                Tuple<Vector2Int, TileInstance> current = queue.Dequeue();
                if (current.Item1 == goal.Item1)
                {
                    return new()
                    {
                        isPathPossible = true,
                    };
                }
                foreach (var dir in dirs)
                {
                    if (grid.TryGetValue(current.Item1 + dir, out TileInstance neighbor))
                    {
                        if (AreNeighborsConnected(current.Item2, neighbor, current.Item1 + dir - current.Item1) && visited.Contains(current.Item1 + dir))
                        {
                            queue.Enqueue(Tuple.Create(current.Item1 + dir, neighbor));
                            visited.Add(current.Item1 + dir);
                        }
                    }
                }
            }
            return new()
            {
                isPathPossible = false,
            };
        }

        public static bool AreNeighborsConnected(TileInstance start, TileInstance neighbor, Vector2Int dir)
        {
            if (start == null || neighbor == null) return false;

            Direction direction = Direction.Up;
            if (dir == Vector2Int.up) direction = Direction.Up;
            else if (dir == Vector2Int.right) direction = Direction.Right;
            else if (dir == Vector2Int.down) direction = Direction.Down;
            else if (dir == Vector2Int.left) direction = Direction.Left;

            Direction opposite = GetOppositeDirection(direction);

            return start.connectionInfo.HasConnection(opposite)
                && neighbor.connectionInfo.HasConnection(direction);
        }

        public static Direction GetOppositeDirection(Direction direction)
        {
            if (direction == Direction.Left) return Direction.Right;
            else if (direction == Direction.Right) return Direction.Left;
            else if (direction == Direction.Up) return Direction.Down;
            else return Direction.Up;
        }
    }
}
