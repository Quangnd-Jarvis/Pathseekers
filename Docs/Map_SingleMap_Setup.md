# Map_SingleMap_Setup
Mục tiêu: Sinh ra 1 bản đồ dạng lưới chỉ gồm các MapTile (không UI, không hệ Tiles), dựa trên prefab MapTile tối giản và 3 script:
- MapTile (MonoBehaviour): lưu toạ độ lưới, gizmo
- MapSettings (ScriptableObject): cấu hình size/level/prefab
- SimpleMapGenerator (MonoBehaviour): sinh map theo grid

Lưu ý:
- Code tuân thủ C# Coding Standards, bình luận tiếng Anh.
- Hạn chế LINQ, không dùng Coroutine, không cần UniTask.
- Mặc định dùng mặt phẳng XZ (3D). Nếu làm 2D, bật Use XY Plane để dùng XY.

1) Chuẩn bị thư mục (khuyến nghị)
- Assets/Scripts/Map
- Assets/ScriptableObjects/Map
- Assets/Prefabs/Map
- Assets/Materials (tuỳ chọn)

2) Tạo prefab MapTile (tối giản)
Cách A (3D - Quad):
- GameObject → 3D Object → Quad (đổi tên: MapTile)
- Reset Transform, Scale = (1,1,1)
- (Tuỳ chọn) Tạo material màu nhạt để dễ nhìn
- Kéo thả vào Assets/Prefabs/Map để tạo prefab

Cách B (2D - Sprite):
- GameObject → 2D Object → Sprite → chọn Square (hoặc sprite vuông bất kỳ), đổi tên MapTile
- Reset Transform, Scale = (1,1,1)
- (Tuỳ chọn) gán màu
- Tạo prefab như trên

3) Thêm scripts

3.1) MapTile.cs (Assets/Scripts/Map/MapTile.cs)
```csharp
using UnityEngine;

namespace TilePathGame.Map
{
    [DisallowMultipleComponent]
    public class MapTile : MonoBehaviour
    {
        [SerializeField] private Vector2Int _gridPosition;

        public Vector2Int GridPosition => _gridPosition;

        // Initialize tile with grid coordinates
        public void Init(Vector2Int gridPosition)
        {
            _gridPosition = gridPosition;
            UpdateName();
        }

        [ContextMenu("Update Name")]
        private void UpdateName()
        {
            gameObject.name = $"MapTile[{_gridPosition.x},{_gridPosition.y}]";
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Simple visual feedback in Scene view
            Gizmos.color = new Color(1f, 1f, 1f, 0.8f);
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.95f);
        }
#endif
    }
}
```

3.2) MapSettings.cs (Assets/Scripts/Map/MapSettings.cs)
```csharp
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
```

3.3) SimpleMapGenerator.cs (Assets/Scripts/Map/SimpleMapGenerator.cs)
```csharp
using UnityEngine;

namespace TilePathGame.Map
{
    public class SimpleMapGenerator : MonoBehaviour
    {
        [SerializeField] private MapSettings _settings;
        [SerializeField] private Transform _container;
        [SerializeField] private bool _generateOnStart = true;
        [SerializeField] private bool _clearBeforeGenerate = true;

        // Entry point to generate a single map made of MapTile instances
        public void Generate()
        {
            if (_settings == null)
            {
                Debug.LogError("MapSettings is null.", this);
                return;
            }
            if (_settings.TilePrefab == null)
            {
                Debug.LogError("Tile Prefab is null in MapSettings.", _settings);
                return;
            }

            Transform parent = EnsureContainer();
            if (_clearBeforeGenerate)
            {
                ClearChildren(parent);
            }

            int n = _settings.ResolveSize();
            float cell = _settings.CellSize;

            Vector3 origin = Vector3.zero;
            if (_settings.Centered)
            {
                float half = (n - 1) * 0.5f;
                if (_settings.UseXYPlane)
                    origin = new Vector3(-half * cell, -half * cell, 0f);
                else
                    origin = new Vector3(-half * cell, 0f, -half * cell);
            }

            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    Vector3 pos;
                    if (_settings.UseXYPlane)
                        pos = new Vector3(x * cell, y * cell, 0f) + origin; // 2D grid on XY
                    else
                        pos = new Vector3(x * cell, 0f, y * cell) + origin; // 3D grid on XZ

                    MapTile tile = Instantiate(_settings.TilePrefab, pos, Quaternion.identity, parent);
                    tile.Init(new Vector2Int(x, y));
                }
            }
        }

        private Transform EnsureContainer()
        {
            if (_container != null) return _container;

            Transform t = transform.Find("Tiles");
            if (t == null)
            {
                GameObject go = new GameObject("Tiles");
                go.transform.SetParent(transform, false);
                t = go.transform;
            }
            _container = t;
            return _container;
        }

        private void ClearChildren(Transform parent)
        {
            int count = parent.childCount;
            for (int i = count - 1; i >= 0; i--)
            {
                Transform c = parent.GetChild(i);
                if (Application.isPlaying)
                    Destroy(c.gameObject);
                else
                    DestroyImmediate(c.gameObject);
            }
        }

        private void Start()
        {
            if (_generateOnStart)
                Generate();
        }

#if UNITY_EDITOR
        [ContextMenu("Generate Map (Editor)")]
        private void GenerateEditor()
        {
            Generate();
        }
#endif
    }
}
```

4) Tạo asset MapSettings
- Chuột phải trong Project → Create → TilePathGame → Map → MapSettings
- Đặt tên: MapSettings_Sample
- Gán Tile Prefab = Prefabs/Map/MapTile
- Nếu muốn dùng công thức size = 4 + Level:
  - Bật Use Level Formula
  - Chọn Level (ví dụ Level=1 → Size=5)
- Nếu muốn đặt size trực tiếp:
  - Tắt Use Level Formula, nhập Size (ví dụ 6)
- Chọn Cell Size (khoảng cách giữa tiles, mặc định 1)
- 2D: Bật Use XY Plane; 3D: tắt
- Giữ Centered để map cân giữa gốc toạ độ

5) Setup scene
- Tạo Empty GameObject tên: MapRoot
- Add Component: SimpleMapGenerator
- Kéo thả MapSettings_Sample vào ô Settings của SimpleMapGenerator
- Tuỳ chọn: để Generate On Start bật sẵn (Play là sinh map)
- Tuỳ chọn: Nhấn vào ba chấm component, chọn Generate Map (Editor) để sinh map trực tiếp ngoài Play

6) Kiểm tra nhanh
- Nhấn Play (hoặc dùng Context Menu) → thấy lưới các MapTile được tạo đều, đặt đúng khoảng cách.
- Trong Hierarchy, các tile sẽ có tên dạng MapTile[x,y]
- Có wireframe Gizmo viền ô trong Scene view.

7) Troubleshooting
- Không thấy gì: kiểm tra MapSettings đã gán Tile Prefab chưa.
- Sai mặt phẳng (tile không thấy trong 2D): bật/tắt Use XY Plane cho phù hợp.
- Vị trí lệch: tắt Centered nếu muốn map bắt đầu từ (0,0,0).
- Quá dày/loãng: điều chỉnh Cell Size.

8) Mở rộng (tuỳ chọn)
- Đổi màu prefab theo (x+y) để tạo pattern dễ nhìn.
- Lưu seed/data bằng ScriptableObject khác nếu cần.
- Thêm object pooling nếu bạn cần tái sinh map nhiều lần trong Play.

Hoàn tất. Bạn đã có 1 map chỉ gồm các MapTile theo lưới.
