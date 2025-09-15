using System.Collections.Generic;
using UnityEngine;
using TilePathGame.Tiles;
using TilePathGame.Map;

namespace TilePathGame.Validation
{
    public static class TileValidator
    {
        #region Enums and Data Structures

        public struct ValidationResult
        {
            public bool isValid;
            public List<string> issues;

            public ValidationResult(bool valid, List<string> issues = null)
            {
                isValid = valid;
                this.issues = issues ?? new List<string>();
            }
        }

        #endregion

        #region Static Data

        private static readonly Dictionary<TileType, ConnectionInfo> TileConnections = new()
        {
            // Dead End Tiles (1 connection)
            { TileType.DeadEndUp, new ConnectionInfo(true, false, false, false) },
            { TileType.DeadEndDown, new ConnectionInfo(false, false, true, false) },
            { TileType.DeadEndLeft, new ConnectionInfo(false, false, false, true) },
            { TileType.DeadEndRight, new ConnectionInfo(false, true, false, false) },
            
            // Straight Tiles (2 connections)
            { TileType.StraightHorizontal, new ConnectionInfo(false, true, false, true) },
            { TileType.StraightVertical, new ConnectionInfo(true, false, true, false) },
            
            // Corner Tiles (2 connections)
            { TileType.CornerUpRight, new ConnectionInfo(true, true, false, false) },
            { TileType.CornerUpLeft, new ConnectionInfo(true, false, false, true) },
            { TileType.CornerDownRight, new ConnectionInfo(false, true, true, false) },
            { TileType.CornerDownLeft, new ConnectionInfo(false, false, true, true) },
            
            // T-Junction Tiles (3 connections)
            { TileType.TJunctionUpLeftRight, new ConnectionInfo(true, true, false, true) },
            { TileType.TJunctionDownLeftRight, new ConnectionInfo(false, true, true, true) },
            { TileType.TJunctionLeftUpDown, new ConnectionInfo(true, false, true, true) },
            { TileType.TJunctionRightUpDown, new ConnectionInfo(true, true, true, false) },
            
            // Cross Tile (4 connections)
            { TileType.Cross, new ConnectionInfo(true, true, true, true) }
        };

        private static readonly Vector2Int[] DirectionVectors = {
            Vector2Int.up,      // Up
            Vector2Int.right,   // Right
            Vector2Int.down,    // Down
            Vector2Int.left     // Left
        };

        #endregion

        #region Public Methods

        /// <summary>
        /// Validates a single tile placement for PARTIAL CONNECTION puzzle gameplay
        /// Tiles only need AT LEAST 1 valid connection, other connections can be open-ended
        /// </summary>
        /// <param name="mapTile">The MapTile slot where tile was just placed</param>
        /// <param name="grid">Current grid state (may have empty slots)</param>
        /// <returns>ValidationResult for this placement</returns>
        public static ValidationResult ValidateTilePlacement(MapTile mapTile, Dictionary<Vector2Int, TileInstance> grid)
        {
            List<string> issues = new List<string>();

            if (mapTile == null)
            {
                issues.Add("MapTile is null");
                return new ValidationResult(false, issues);
            }

            TileInstance tileInstance = mapTile.GetComponentInChildren<TileInstance>();
            if (tileInstance == null || tileInstance.Data == null)
            {
                // Empty slot is OK during gameplay
                return new ValidationResult(true);
            }

            ConnectionInfo tileConnections = GetTileConnections(tileInstance.Data.TileType);
            bool hasValidConnection = false;

            // RULE 1: Check for connection conflicts with existing neighbors
            // AND count valid connections
            for (int i = 0; i < 4; i++)
            {
                Direction direction = (Direction)i;
                Vector2Int neighborPos = mapTile.GridPosition + DirectionVectors[i];
                bool tileWantsConnection = tileConnections.HasConnection(direction);
                
                // Check if there's an existing neighbor at this position
                if (grid.ContainsKey(neighborPos) && grid[neighborPos] != null)
                {
                    TileInstance neighbor = grid[neighborPos];
                    
                    if (neighbor?.Data != null)
                    {
                        ConnectionInfo neighborConnections = GetTileConnections(neighbor.Data.TileType);
                        Direction oppositeDirection = GetOppositeDirection(direction);
                        bool neighborWantsConnection = neighborConnections.HasConnection(oppositeDirection);

                        // Connection mismatch is invalid
                        if (tileWantsConnection && !neighborWantsConnection)
                        {
                            issues.Add($"Connection conflict: {tileInstance.Data.TileType} at {mapTile.GridPosition} wants to connect {direction}, but {neighbor.Data.TileType} at {neighborPos} has no connection back");
                        }
                        else if (!tileWantsConnection && neighborWantsConnection)
                        {
                            issues.Add($"Connection conflict: {neighbor.Data.TileType} at {neighborPos} expects connection from {tileInstance.Data.TileType} at {mapTile.GridPosition}, but tile has no connection in that direction");
                        }
                        // If both want connection, it's a VALID connection!
                        else if (tileWantsConnection && neighborWantsConnection)
                        {
                            hasValidConnection = true;
                        }
                    }
                }
                // If tile wants connection but neighbor doesn't exist (boundary or empty)
                // This is OK - open-ended connection allowed
            }

            // RULE 2: Must have at least ONE valid connection (except for first tile)
            if (!IsFirstTile(grid) && !hasValidConnection)
            {
                issues.Add($"No valid connections: {tileInstance.Data.TileType} at {mapTile.GridPosition} has no valid connections to existing network");
            }
            
            // RULE 3: Global Pathfinding Validation (if enabled)
            // Check if path from start to goal is still possible with this placement
            var pathfindingResult = ValidateGlobalPathfinding(mapTile, grid);
            if (!pathfindingResult.isValid)
            {
                issues.AddRange(pathfindingResult.issues);
            }

            return new ValidationResult(issues.Count == 0, issues);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Global pathfinding validation - checks if start can still reach goal
        /// </summary>
        /// <param name="mapTile">Recently placed tile</param>
        /// <param name="grid">Current grid state</param>
        /// <returns>Validation result for pathfinding</returns>
        private static ValidationResult ValidateGlobalPathfinding(MapTile mapTile, Dictionary<Vector2Int, TileInstance> grid)
        {
            var issues = new List<string>();
            
            // Find start and goal positions
            Vector2Int? startPos = FindSpecialTilePosition(grid, ESpecialTileType.StartTile);
            Vector2Int? goalPos = FindSpecialTilePosition(grid, ESpecialTileType.GoalTile);
            
            // Skip validation if start or goal not defined
            if (!startPos.HasValue || !goalPos.HasValue)
            {
                // No global validation needed if no start/goal defined
                return new ValidationResult(true);
            }
            
            // Get available tile pool (this needs to be injected or accessed)
            var tilePool = GetCurrentTilePool();
            if (tilePool == null)
            {
                // Skip pathfinding validation if no pool available
                return new ValidationResult(true);
            }
            
            // Use PathfindingValidator to check if path is still possible
            var pathResult = PathfindingValidator.IsPathPossible(startPos.Value, goalPos.Value, grid);
            
            if (!pathResult.isPathPossible)
            {
                issues.Add($"Global validation failed: No possible path from Start to Goal after placing {mapTile.GridPosition}");
                if (pathResult.issues != null)
                {
                    issues.AddRange(pathResult.issues);
                }
            }
            
            return new ValidationResult(issues.Count == 0, issues);
        }
        
        /// <summary>
        /// Find position of special tile type in grid
        /// </summary>
        private static Vector2Int? FindSpecialTilePosition(Dictionary<Vector2Int, TileInstance> grid, ESpecialTileType specialType)
        {
            // This requires access to MapTile information
            // For now, return null - this needs to be implemented based on your map structure
            return null;
        }
        
        /// <summary>
        /// Get current tile pool - needs to be injected or accessed from MapManager
        /// </summary>
        private static ITilePool GetCurrentTilePool()
        {
            // This needs to be implemented based on your architecture
            // Could be singleton, dependency injection, or passed as parameter
            return null;
        }
        
        /// <summary>
        /// Checks if this is the first tile being placed on the grid
        /// </summary>
        /// <param name="grid">Current grid state</param>
        /// <returns>True if this is the first tile (no other tiles exist)</returns>
        private static bool IsFirstTile(Dictionary<Vector2Int, TileInstance> grid)
        {
            if (grid == null) return true;
            
            // Count existing placed tiles (non-null)
            int existingTileCount = 0;
            foreach (var kvp in grid)
            {
                if (kvp.Value != null) existingTileCount++;
            }
            
            // If count <= 1, it means this is the first tile (the 1 would be the tile we just placed)
            return existingTileCount <= 1;
        }

        /// <summary>
        /// Gets connection information for a specific tile type
        /// </summary>
        /// <param name="tileType">Type of tile</param>
        /// <returns>ConnectionInfo for the tile type</returns>
        public static ConnectionInfo GetTileConnections(TileType tileType)
        {
            return TileConnections.TryGetValue(tileType, out ConnectionInfo connections) 
                ? connections 
                : new ConnectionInfo(false, false, false, false);
        }

        /// <summary>
        /// Gets the opposite direction
        /// </summary>
        /// <param name="direction">Original direction</param>
        /// <returns>Opposite direction</returns>
        public static Direction GetOppositeDirection(Direction direction)
        {
            return direction switch
            {
                Direction.Up => Direction.Down,
                Direction.Right => Direction.Left,
                Direction.Down => Direction.Up,
                Direction.Left => Direction.Right,
                _ => direction
            };
        }
        
        /// <summary>
        /// Gets the Vector2Int offset for a direction
        /// </summary>
        /// <param name="direction">Direction to get vector for</param>
        /// <returns>Vector2Int offset</returns>
        public static Vector2Int GetDirectionVector(Direction direction)
        {
            return DirectionVectors[(int)direction];
        }
        
        /// <summary>
        /// Checks if a tile can be validly placed at a specific position
        /// Uses the same logic as ValidateTilePlacement but for prediction/simulation
        /// </summary>
        /// <param name="tileType">Type of tile to place</param>
        /// <param name="position">Grid position to place at</param>
        /// <param name="grid">Current grid state</param>
        /// <returns>True if placement would be valid</returns>
        public static bool CanPlaceTileAt(TileType tileType, Vector2Int position, Dictionary<Vector2Int, TileInstance> grid)
        {
            if (grid == null) return false;
            
            // Check if position already occupied
            if (grid.ContainsKey(position) && grid[position] != null)
            {
                return false; // Position already occupied
            }
            
            ConnectionInfo tileConnections = GetTileConnections(tileType);
            bool hasValidConnection = false;
            
            // Check all 4 directions for connection conflicts and valid connections
            for (int i = 0; i < 4; i++)
            {
                Direction direction = (Direction)i;
                Vector2Int neighborPos = position + DirectionVectors[i];
                bool tileWantsConnection = tileConnections.HasConnection(direction);
                
                // Check if there's an existing neighbor at this position
                if (grid.ContainsKey(neighborPos) && grid[neighborPos] != null)
                {
                    TileInstance neighbor = grid[neighborPos];
                    
                    if (neighbor?.Data != null)
                    {
                        ConnectionInfo neighborConnections = GetTileConnections(neighbor.Data.TileType);
                        Direction oppositeDirection = GetOppositeDirection(direction);
                        bool neighborWantsConnection = neighborConnections.HasConnection(oppositeDirection);
                        
                        // Connection conflict check
                        if (tileWantsConnection && !neighborWantsConnection)
                        {
                            return false; // Tile wants to connect but neighbor doesn't
                        }
                        if (!tileWantsConnection && neighborWantsConnection)
                        {
                            return false; // Neighbor wants to connect but tile doesn't
                        }
                        
                        // If both want connection, it's valid!
                        if (tileWantsConnection && neighborWantsConnection)
                        {
                            hasValidConnection = true;
                        }
                    }
                }
                // Open-ended connections (to empty spaces) are allowed
            }
            
            // Must have at least one valid connection (except for first tile)
            if (!IsFirstTileAtPosition(position, grid) && !hasValidConnection)
            {
                return false; // No valid connections to existing network
            }
            
            return true;
        }
        
        /// <summary>
        /// Helper method to check if placing at position would be the first tile
        /// </summary>
        private static bool IsFirstTileAtPosition(Vector2Int position, Dictionary<Vector2Int, TileInstance> grid)
        {
            if (grid == null) return true;
            
            // Count existing placed tiles (non-null)
            int existingTileCount = 0;
            foreach (var kvp in grid)
            {
                if (kvp.Value != null && kvp.Key != position) // Exclude the position we're checking
                {
                    existingTileCount++;
                }
            }
            
            return existingTileCount == 0; // This would be the first tile
        }
        
        /// <summary>
        /// Advanced version: Check if tile can be placed with global pathfinding validation
        /// </summary>
        /// <param name="tileType">Type of tile to place</param>
        /// <param name="position">Grid position to place at</param>
        /// <param name="grid">Current grid state</param>
        /// <param name="startPos">Start position for pathfinding</param>
        /// <param name="goalPos">Goal position for pathfinding</param>
        /// <param name="tilePool">Available tile pool</param>
        /// <returns>True if placement is valid and path is still possible</returns>
        public static bool CanPlaceTileAt(TileType tileType, Vector2Int position, Dictionary<Vector2Int, TileInstance> grid, 
            Vector2Int? startPos, Vector2Int? goalPos, ITilePool tilePool = null)
        {
            // First do basic placement validation
            if (!CanPlaceTileAt(tileType, position, grid))
            {
                return false;
            }
            
            // If no start/goal or pool defined, skip global validation
            if (!startPos.HasValue || !goalPos.HasValue || tilePool == null)
            {
                return true;
            }
            
            // Create simulated grid with the new tile placed
            var simulatedGrid = CreateSimulatedGrid(grid, position, tileType);
            
            // Remove the tile from pool temporarily for simulation
            var tempPool = CreateTempPool(tilePool, tileType);
            
            // Check if path is still possible with this placement
            var pathResult = PathfindingValidator.IsPathPossible(startPos.Value, goalPos.Value, simulatedGrid);
            
            return pathResult.isPathPossible;
        }
        
        /// <summary>
        /// Create simulated grid with a tile placed at position
        /// </summary>
        private static Dictionary<Vector2Int, TileInstance> CreateSimulatedGrid(Dictionary<Vector2Int, TileInstance> originalGrid, Vector2Int position, TileType tileType)
        {
            var simulatedGrid = new Dictionary<Vector2Int, TileInstance>(originalGrid);
            
            // Create mock TileInstance for simulation
            var mockTileData = ScriptableObject.CreateInstance<TileData>();
            // Set tile type - this would need proper initialization in real implementation
            
            var mockGameObject = new GameObject($"Mock_{tileType}_at_{position}");
            var mockTileInstance = mockGameObject.AddComponent<TileInstance>();
            mockTileInstance.Init(mockTileData);
            
            simulatedGrid[position] = mockTileInstance;
            
            return simulatedGrid;
        }
        
        /// <summary>
        /// Create temporary pool with one tile removed
        /// </summary>
        private static ITilePool CreateTempPool(ITilePool originalPool, TileType tileType)
        {
            // This would need proper implementation based on your TilePool structure
            // For now, return the original pool
            return originalPool;
        }
        
        /// <summary>
        /// Quick validation for UI hover feedback - checks only basic placement rules
        /// </summary>
        /// <param name="tileType">Type of tile being hovered</param>
        /// <param name="position">Position being hovered over</param>
        /// <param name="grid">Current grid state</param>
        /// <returns>Result with quick validation info</returns>
        public static (bool canPlace, string reason) CanPlaceTileQuick(TileType tileType, Vector2Int position, Dictionary<Vector2Int, TileInstance> grid)
        {
            if (grid == null) 
                return (false, "Grid is null");
            
            // Check if position exists in valid map area
            if (!grid.ContainsKey(position))
                return (false, "Position outside map bounds");
                
            // Check if position already occupied
            if (grid[position] != null)
                return (false, "Position already occupied");
            
            // Quick connection check with immediate neighbors
            ConnectionInfo tileConnections = GetTileConnections(tileType);
            bool hasConflict = false;
            bool hasValidConnection = false;
            string conflictReason = "";
            
            for (int i = 0; i < 4; i++)
            {
                Direction direction = (Direction)i;
                Vector2Int neighborPos = position + DirectionVectors[i];
                bool tileWantsConnection = tileConnections.HasConnection(direction);
                
                if (grid.ContainsKey(neighborPos) && grid[neighborPos] != null)
                {
                    TileInstance neighbor = grid[neighborPos];
                    if (neighbor?.Data != null)
                    {
                        ConnectionInfo neighborConnections = GetTileConnections(neighbor.Data.TileType);
                        Direction oppositeDirection = GetOppositeDirection(direction);
                        bool neighborWantsConnection = neighborConnections.HasConnection(oppositeDirection);
                        
                        if (tileWantsConnection && !neighborWantsConnection)
                        {
                            hasConflict = true;
                            conflictReason = $"Connection conflict with {neighbor.Data.TileType} to the {direction}";
                            break;
                        }
                        if (!tileWantsConnection && neighborWantsConnection)
                        {
                            hasConflict = true;
                            conflictReason = $"{neighbor.Data.TileType} to the {direction} expects connection";
                            break;
                        }
                        
                        if (tileWantsConnection && neighborWantsConnection)
                        {
                            hasValidConnection = true;
                        }
                    }
                }
            }
            
            if (hasConflict)
                return (false, conflictReason);
                
            if (!IsFirstTileAtPosition(position, grid) && !hasValidConnection)
                return (false, "No valid connections to existing network");
            
            return (true, "Valid placement");
        }

        /// <summary>
        /// Gets the position of the first tile in the grid
        /// </summary>
        /// <param name="grid">Grid dictionary</param>
        /// <returns>Position of first tile</returns>
        private static Vector2Int GetFirstTilePosition(Dictionary<Vector2Int, TileInstance> grid)
        {
            foreach (var kvp in grid)
            {
                return kvp.Key;
            }
            return Vector2Int.zero;
        }

        /// <summary>
        /// Creates a grid dictionary from MapTiles for validation
        /// </summary>
        /// <param name="mapTiles">Array of MapTile objects</param>
        /// <returns>Dictionary mapping positions to TileInstances</returns>
        public static Dictionary<Vector2Int, TileInstance> CreateGridFromMapTiles(MapTile[] mapTiles)
        {
            Dictionary<Vector2Int, TileInstance> grid = new Dictionary<Vector2Int, TileInstance>();

            foreach (MapTile mapTile in mapTiles)
            {
                if (mapTile != null)
                {
                    TileInstance tileInstance = mapTile.GetComponentInChildren<TileInstance>();
                    if (tileInstance != null)
                    {
                        grid[mapTile.GridPosition] = tileInstance;
                    }
                }
            }

            return grid;
        }

        /// <summary>
        /// Creates a grid dictionary that includes empty slots (for partial validation)
        /// </summary>
        /// <param name="mapTiles">Array of all MapTile objects (including empty ones)</param>
        /// <returns>Dictionary mapping positions to TileInstances (null for empty slots)</returns>
        public static Dictionary<Vector2Int, TileInstance> CreatePartialGridFromMapTiles(MapTile[] mapTiles)
        {
            Dictionary<Vector2Int, TileInstance> grid = new Dictionary<Vector2Int, TileInstance>();

            if (mapTiles == null)
            {
                Debug.LogWarning("MapTiles array is null in CreatePartialGridFromMapTiles");
                return grid;
            }

            foreach (MapTile mapTile in mapTiles)
            {
                if (mapTile != null)
                {
                    TileInstance tileInstance = mapTile.GetComponentInChildren<TileInstance>();
                    // Include position even if tileInstance is null (empty slot)
                    grid[mapTile.GridPosition] = tileInstance;
                }
            }

            return grid;
        }

        #endregion
    }
}
