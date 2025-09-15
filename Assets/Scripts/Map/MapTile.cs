using System;
using TilePathGame.Tiles;
using TilePathGame.Validation;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TilePathGame.Map
{
    [DisallowMultipleComponent]
    public class MapTile : MonoBehaviour, IDropHandler
    {
        [SerializeField] private Vector2Int _gridPosition;
        [SerializeField] private ESpecialTileType _specialType = ESpecialTileType.None;

        public Vector2Int GridPosition => _gridPosition;
        public ESpecialTileType SpecialType => _specialType;
        public bool IsStartTile => _specialType == ESpecialTileType.StartTile;
        public bool IsGoalTile => _specialType == ESpecialTileType.GoalTile;

        // Initialize tile with grid coordinates
        public void Init(Vector2Int gridPosition, ESpecialTileType specialType = ESpecialTileType.None)
        {
            _gridPosition = gridPosition;
            _specialType = specialType;
            UpdateName();
        }
        
        /// <summary>
        /// Set special tile type (Start/Goal)
        /// </summary>
        public void SetSpecialType(ESpecialTileType specialType)
        {
            _specialType = specialType;
            UpdateName();
            UpdateVisualIndicator();
        }

        [ContextMenu("Update Name")]
        private void UpdateName()
        {
            string specialSuffix = _specialType switch
            {
                ESpecialTileType.StartTile => "_START",
                ESpecialTileType.GoalTile => "_GOAL",
                _ => ""
            };
            gameObject.name = $"MapTile[{_gridPosition.x},{_gridPosition.y}]{specialSuffix}";
        }
        
        private void UpdateVisualIndicator()
        {
            // TODO: Add visual indicators for start/goal tiles
            // Could be different colors, icons, or effects
        }

        private void OnDrawGizmos()
        {
            // Simple visual feedback in Scene view
            Gizmos.color = new Color(1f, 1f, 1f, 0.8f);
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.95f);
        }

        public void OnDrop(PointerEventData eventData)
        {
            var dragTile = eventData.pointerDrag.GetComponent<TileInstance>();
            if (dragTile != null)
            {
                dragTile.transform.SetParent(transform);
                dragTile.transform.localPosition = Vector2.zero;
                dragTile.MarkDropped();
            }

            // Lấy grid hiện tại(bao gồm cả slots trống)
            MapTile[] allSlots = SimpleMapGenerator.GetMapTiles();
            if (allSlots == null || allSlots.Length == 0)
            {
                Debug.LogWarning("Cannot validate tile placement: No map tiles available");
                return;
            }
            
            var grid = TileValidator.CreatePartialGridFromMapTiles(allSlots);

            // Kiểm tra tile vừa đặt
            var result = TileValidator.ValidateTilePlacement(this, grid);

            if (!result.isValid)
            {
                Debug.Log("❌ Tile đặt SAI:");

                // Có thể highlight tile sai bằng màu đỏ
                HighlightTileError();
            }
            else
            {
                Debug.Log("✅ Tile đặt đúng!");
                // Có thể highlight bằng màu xanh
                HighlightTileCorrect();
            }
        }

        private void HighlightTileCorrect()
        {

        }

        private void HighlightTileError()
        {

        }
    }
}

