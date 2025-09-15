using UnityEngine;
using UnityEngine.EventSystems;

namespace TilePathGame.Tiles
{
    public class TileInstance : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private TileData _data;
        [SerializeField] private GameObject[] _lineObjs = new GameObject[4];

        public TileData Data => _data;
        private Collider2D _coll;
        private Transform _originalParent;
        private Vector3 _originalPos;
        public ConnectionInfo connectionInfo;

        private void Awake()
        {
            _coll = GetComponent<Collider2D>();
        }

        public void Init(TileData tileData)
        {
            _data = tileData;
            gameObject.name = _data != null ? $"Tile_{_data.Id}" : "Tile_Uninitialized";
            UpdateVisuals();
        }

        private void OnValidate()
        {
            if (Application.isPlaying) return;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (_data == null)
            {
                SetLines(false, false, false, false);
                return;
            }

            switch (_data.TileType)
            {
                case TileType.DeadEndUp:
                    SetLines(true, false, false, false);
                    break;
                case TileType.DeadEndDown:
                    SetLines(false, false, true, false);
                    break;
                case TileType.DeadEndLeft:
                    SetLines(false, false, false, true);
                    break;
                case TileType.DeadEndRight:
                    SetLines(false, true, false, false);
                    break;
                case TileType.StraightHorizontal:
                    SetLines(false, true, false, true);
                    break;
                case TileType.StraightVertical:
                    SetLines(true, false, true, false);
                    break;
                case TileType.CornerUpRight:
                    SetLines(true, true, false, false);
                    break;
                case TileType.CornerUpLeft:
                    SetLines(true, false, false, true);
                    break;
                case TileType.CornerDownRight:
                    SetLines(false, true, true, false);
                    break;
                case TileType.CornerDownLeft:
                    SetLines(false, false, true, true);
                    break;
                case TileType.TJunctionUpLeftRight:
                    SetLines(true, true, false, true);
                    break;
                case TileType.TJunctionDownLeftRight:
                    SetLines(false, true, true, true);
                    break;
                case TileType.TJunctionLeftUpDown:
                    SetLines(true, false, true, true);
                    break;
                case TileType.TJunctionRightUpDown:
                    SetLines(true, true, true, false);
                    break;
                case TileType.Cross:
                    SetLines(true, true, true, true);
                    break;
                default:
                    SetLines(false, false, false, false);
                    break;
            }
        }

        private void SetLines(bool up, bool right, bool down, bool left)
        {
            if (_lineObjs == null || _lineObjs.Length != 4) return;

            if (_lineObjs[0] != null) _lineObjs[0].SetActive(up);
            if (_lineObjs[1] != null) _lineObjs[1].SetActive(right);
            if (_lineObjs[2] != null) _lineObjs[2].SetActive(down);
            if (_lineObjs[3] != null) _lineObjs[3].SetActive(left);
            connectionInfo = new(up, right, down, left);
        }

        private bool droppedOnTile = false;

        public void OnDrag(PointerEventData eventData)
        {
            Vector3 screenPos = new Vector3(eventData.position.x, eventData.position.y, Camera.main.nearClipPlane);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            worldPos.z = transform.position.z;
            transform.position = worldPos;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _originalParent = transform.parent;
            _originalPos = transform.position;
            droppedOnTile = false;
            if (_coll != null) _coll.enabled = false;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_coll != null) _coll.enabled = true;

            if (!droppedOnTile)
            {
                // Không thả lên target => quay về chỗ cũ
                transform.SetParent(_originalParent);
                transform.position = _originalPos;
            }
        }

        public void MarkDropped()
        {
            droppedOnTile = true;
        }
    }
}

