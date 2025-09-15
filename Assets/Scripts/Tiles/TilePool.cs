using System.Collections.Generic;
using TilePathGame.Validation;

namespace TilePathGame.Tiles
{
    public class TilePool : ITilePool
    {
        private List<TileData> _availableTiles;
        private readonly List<TileData> _originalPool; // Backup for reset functionality

        public int Count => _availableTiles.Count;
        public bool IsEmpty => _availableTiles.Count == 0;

        public TilePool(IEnumerable<TileData> initialTiles)
        {
            _originalPool = new List<TileData>(initialTiles);
            _availableTiles = new List<TileData>(_originalPool);
        }

        public IReadOnlyList<TileData> GetAvailableTiles()
        {
            return _availableTiles.AsReadOnly();
        }

        public bool CanProvideConnection(Direction direction)
        {
            foreach (var tile in _availableTiles)
            {
                if (tile == null) continue;
                
                var connections = TileValidator.GetTileConnections(tile.TileType);
                if (connections.HasConnection(direction))
                {
                    return true;
                }
            }
            return false;
        }

        public bool CanProvideTileForConnections(bool needUp, bool needRight, bool needDown, bool needLeft)
        {
            foreach (var tile in _availableTiles)
            {
                if (tile == null) continue;
                
                var connections = TileValidator.GetTileConnections(tile.TileType);
                
                // Check if tile can satisfy all required connections
                bool satisfiesUp = !needUp || connections.up;
                bool satisfiesRight = !needRight || connections.right;
                bool satisfiesDown = !needDown || connections.down;
                bool satisfiesLeft = !needLeft || connections.left;
                
                // Also check that tile doesn't have unwanted connections
                // (This prevents placing a Cross tile when only a Corner is needed)
                bool noExtraUp = needUp || !connections.up;
                bool noExtraRight = needRight || !connections.right;
                bool noExtraDown = needDown || !connections.down;
                bool noExtraLeft = needLeft || !connections.left;
                
                if (satisfiesUp && satisfiesRight && satisfiesDown && satisfiesLeft &&
                    noExtraUp && noExtraRight && noExtraDown && noExtraLeft)
                {
                    return true;
                }
            }
            return false;
        }

        public List<TileData> GetTilesForConnections(bool needUp, bool needRight, bool needDown, bool needLeft)
        {
            var matchingTiles = new List<TileData>();
            
            foreach (var tile in _availableTiles)
            {
                if (tile == null) continue;
                
                var connections = TileValidator.GetTileConnections(tile.TileType);
                
                // Check if tile matches exactly what we need
                if (connections.up == needUp && 
                    connections.right == needRight && 
                    connections.down == needDown && 
                    connections.left == needLeft)
                {
                    matchingTiles.Add(tile);
                }
            }
            
            return matchingTiles;
        }

        public void RemoveTile(TileData tileData)
        {
            if (tileData != null && _availableTiles.Contains(tileData))
            {
                _availableTiles.Remove(tileData);
            }
        }

        public void AddTile(TileData tileData)
        {
            if (tileData != null && !_availableTiles.Contains(tileData))
            {
                _availableTiles.Add(tileData);
            }
        }

        public void Reset()
        {
            _availableTiles.Clear();
            _availableTiles.AddRange(_originalPool);
        }
    }
}
