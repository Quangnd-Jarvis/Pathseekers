namespace TilePathGame.Tiles
{
    public enum TileType
    {
        // Dead End Tiles (1 connection)
        /// <summary>Dead End facing Up: ╹</summary>
        DeadEndUp = 1,
        /// <summary>Dead End facing Down: ╻</summary>
        DeadEndDown = 2,
        /// <summary>Dead End facing Left: ╸</summary>
        DeadEndLeft = 3,
        /// <summary>Dead End facing Right: ╺</summary>
        DeadEndRight = 4,
        
        // Straight Tiles (2 connections)
        /// <summary>Straight Horizontal: ━</summary>
        StraightHorizontal = 5,
        /// <summary>Straight Vertical: ┃</summary>
        StraightVertical = 6,
        
        // Corner Tiles (2 connections)
        /// <summary>Corner Up+Right: ┗</summary>
        CornerUpRight = 7,
        /// <summary>Corner Up+Left: ┛</summary>
        CornerUpLeft = 8,
        /// <summary>Corner Down+Right: ┏</summary>
        CornerDownRight = 9,
        /// <summary>Corner Down+Left: ┓</summary>
        CornerDownLeft = 10,
        
        // T-Junction Tiles (3 connections)
        /// <summary>T-Junction Up+Left+Right: ┻</summary>
        TJunctionUpLeftRight = 11,
        /// <summary>T-Junction Down+Left+Right: ┳</summary>
        TJunctionDownLeftRight = 12,
        /// <summary>T-Junction Left+Up+Down: ┫</summary>
        TJunctionLeftUpDown = 13,
        /// <summary>T-Junction Right+Up+Down: ┣</summary>
        TJunctionRightUpDown = 14,
        
        // Cross Tile (4 connections)
        /// <summary>Cross - All 4 directions: ╋</summary>
        Cross = 15
    }
}

