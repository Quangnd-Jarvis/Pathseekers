using UnityEngine;
using System.Collections.Generic;

namespace TilePathGame.Tiles
{
    [CreateAssetMenu(fileName = "TileData", menuName = "TilePathGame/Tiles/TileData")]
    public class TileData : ScriptableObject
    {
        [Header("Identification")]
        [SerializeField] private string _id;
        [SerializeField] private TileType _tileType;

        [Header("Shape Definition")]
        [Tooltip("Defines the shape relative to a center point (0,0). Each Vector2Int is a cell of the tile.")]
        [SerializeField] private List<Vector2Int> _shape = new List<Vector2Int> { Vector2Int.zero };

        public string Id => _id;
public TileType TileType => _tileType;
        public List<Vector2Int> Shape => _shape;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_id))
            {
                _id = name;
            }
        }
#endif
    }
}

