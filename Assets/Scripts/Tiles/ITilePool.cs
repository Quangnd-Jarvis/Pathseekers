using System.Collections.Generic;

namespace TilePathGame.Tiles
{
    public interface ITilePool
    {
        IReadOnlyList<TileData> GetAvailableTiles();
        
        bool CanProvideConnection(Direction direction);
        
        bool CanProvideTileForConnections(bool needUp, bool needRight, bool needDown, bool needLeft);
        
        List<TileData> GetTilesForConnections(bool needUp, bool needRight, bool needDown, bool needLeft);
        
        void RemoveTile(TileData tileData);
        
        void AddTile(TileData tileData);
        
        int Count { get; }
        
        bool IsEmpty { get; }
    }
}
