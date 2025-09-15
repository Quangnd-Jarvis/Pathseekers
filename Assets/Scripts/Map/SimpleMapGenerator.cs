using UnityEngine;
using System.Collections.Generic;
namespace TilePathGame.Map
{
    public class SimpleMapGenerator : MonoBehaviour
    {
        [SerializeField] private MapSettings _settings;
        [SerializeField] private Transform _container;
        [SerializeField] private bool _generateOnStart = true;
        [SerializeField] private bool _clearBeforeGenerate = true;
        private static MapTile[] mapTiles;

        // Entry point to generate a map with a specific number of connected tiles
        public void Generate()
        {
            if (!ValidateSettings()) return;

            Transform parent = EnsureContainer();
            if (_clearBeforeGenerate)
            {
                ClearChildren(parent);
            }

            List<Vector2Int> generatedTiles = GenerateRandomWalkMap(_settings.ResolveSize());
            // Initialize mapTiles array with actual size
            mapTiles = new MapTile[generatedTiles.Count];
            InstantiateTiles(generatedTiles, parent);
        }

        private bool ValidateSettings()
        {
            if (_settings == null)
            {
                Debug.LogError("MapSettings is null.", this);
                return false;
            }
            if (_settings.TilePrefab == null)
            {
                Debug.LogError("Tile Prefab is null in MapSettings.", _settings);
                return false;
            }
            return true;
        }

        public void SetMapSettings(MapSettings mapSettings)
        {
            _settings = mapSettings;
        }

        // Generates a list of connected tile positions using a random walk algorithm
        private List<Vector2Int> GenerateRandomWalkMap(int mapSize)
        {
            List<Vector2Int> positions = new List<Vector2Int>();
            HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
            List<Vector2Int> frontier = new List<Vector2Int>();

            // Start at the origin
            Vector2Int startPos = Vector2Int.zero;
            positions.Add(startPos);
            occupied.Add(startPos);
            AddNeighbors(startPos, frontier, occupied);

            while (positions.Count < mapSize && frontier.Count > 0)
            {
                // Pick a random frontier tile to expand from
                int randomIndex = Random.Range(0, frontier.Count);
                Vector2Int currentPos = frontier[randomIndex];
                
                frontier.RemoveAt(randomIndex);

                if (occupied.Contains(currentPos)) continue;

                positions.Add(currentPos);
                occupied.Add(currentPos);
                AddNeighbors(currentPos, frontier, occupied);
            }

            return positions;
        }

        // Adds valid, unoccupied neighbors to the frontier list
        private void AddNeighbors(Vector2Int pos, List<Vector2Int> frontier, HashSet<Vector2Int> occupied)
        {
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var dir in directions)
            {
                Vector2Int neighbor = pos + dir;
                if (!occupied.Contains(neighbor))
                {
                    frontier.Add(neighbor);
                }
            }
        }

        // Instantiates the tile prefabs at the generated positions
        private void InstantiateTiles(List<Vector2Int> positions, Transform parent)
        {
            if (positions.Count == 0) return;

            float cell = _settings.CellSize;
            Vector3 offset = Vector3.zero;

            if (_settings.Centered)
            {
                Vector2 min = positions[0];
                Vector2 max = positions[0];
                foreach (var pos in positions)
                {
                    min.x = Mathf.Min(min.x, pos.x);
                    min.y = Mathf.Min(min.y, pos.y);
                    max.x = Mathf.Max(max.x, pos.x);
                    max.y = Mathf.Max(max.y, pos.y);
                }
                
                float centerX = (min.x + max.x) * 0.5f;
                float centerY = (min.y + max.y) * 0.5f;

                if (_settings.UseXYPlane)
                    offset = new Vector3(-centerX * cell, -centerY * cell, 0f);
                else
                    offset = new Vector3(-centerX * cell, 0f, -centerY * cell);
            }

            int i = 0;
            foreach (var gridPos in positions)
            {
                Vector3 worldPos;
                if (_settings.UseXYPlane)
                    worldPos = new Vector3(gridPos.x * cell, gridPos.y * cell, 0f) + offset;
                else
                    worldPos = new Vector3(gridPos.x * cell, 0f, gridPos.y * cell) + offset;

                MapTile tile = Instantiate(_settings.TilePrefab, worldPos, Quaternion.identity, parent);
                tile.Init(gridPos);
                mapTiles[i] = tile;
                i++;
            }
        }

        private Transform EnsureContainer()
        {
            if (_container != null) return _container;

            _container = transform.Find("Tiles") ?? new GameObject("Tiles").transform;
            _container.SetParent(transform, false);
            return _container;
        }

        private void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying)
                    Destroy(parent.GetChild(i).gameObject);
                else
                    DestroyImmediate(parent.GetChild(i).gameObject);
            }
        }

        private void Awake()
        {
            if (_generateOnStart)
                Generate();
        }

        public static MapTile[] GetMapTiles()
        {
            return mapTiles ?? new MapTile[0];
        }
        
        /// <summary>
        /// Clears all placed tiles from map slots (keeping the empty MapTiles)
        /// </summary>
        public void ClearPlacedTiles()
        {
            if (mapTiles == null) return;
            
            foreach (MapTile mapTile in mapTiles)
            {
                if (mapTile != null)
                {
                    // Remove any TileInstance children
                    Transform[] children = new Transform[mapTile.transform.childCount];
                    for (int i = 0; i < mapTile.transform.childCount; i++)
                    {
                        children[i] = mapTile.transform.GetChild(i);
                    }
                    
                    foreach (Transform child in children)
                    {
                        if (Application.isPlaying)
                            Destroy(child.gameObject);
                        else
                            DestroyImmediate(child.gameObject);
                    }
                }
            }
            
            Debug.Log("[SimpleMapGenerator] Cleared all placed tiles from map");
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

