using UnityEngine;

namespace TilePathGame.Map
{
    [CreateAssetMenu(fileName = "MapSettings", menuName = "TilePathGame/Map/MapSettings")]
    public class MapSettings : ScriptableObject
    {
        [Header("Size")]
        [SerializeField] private bool _useLevelFormula = true;   // If true, size = 4 + level
        [SerializeField, Min(0)] private int _level = 1;         // Level used for formula
        [SerializeField, Min(1)] private int _size = 5;          // Manual size if formula is off

        [Header("Layout")]
        [SerializeField, Min(0.1f)] private float _cellSize = 1f;
        [SerializeField] private bool _centered = true;          // Center map around origin
        [SerializeField] private bool _useXYPlane = false;       // 2D mode on XY plane (z = 0)

        [Header("Prefabs")]
        [SerializeField] private MapTile _tilePrefab;

        public bool UseLevelFormula => _useLevelFormula;
        public int Level => _level;
        public int Size => _size;
        public float CellSize => _cellSize;
        public bool Centered => _centered;
        public bool UseXYPlane => _useXYPlane;
        public MapTile TilePrefab => _tilePrefab;

        public int ResolveSize()
        {
            int n = _useLevelFormula ? 4 + _level : _size;
            if (n < 1) n = 1;
            return n;
        }
    }
}

