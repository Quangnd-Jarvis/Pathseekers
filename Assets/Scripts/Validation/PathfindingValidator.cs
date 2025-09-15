using System;
using System.Collections.Generic;
using System.Linq;
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
            Vector2Int startPos,
            Vector2Int goalPos,
            Dictionary<Vector2Int, TileInstance> currentGrid)
        {
            Queue<Vector2Int> queue = new();
            Dictionary<Vector2Int, bool> visited = new();
            Vector2Int start = startPos;
            queue.Enqueue(start);

            Vector2Int[] dirs =
            {
                new Vector2Int(0, 1), //up
                new Vector2Int(1, 0), //right
                new Vector2Int(0, -1), // down
                new Vector2Int(-1, 0), // left
            };

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();

            }
            return new();
        }

        public static bool AreNeighborsConnected(Tuple<Vector2Int, TileInstance> current, Tuple<Vector2Int, TileInstance> neighbor)
        {
            return false;
        }

    }
}
