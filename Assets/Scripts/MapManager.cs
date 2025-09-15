using System.Collections.Generic;
using TilePathGame.Tiles;
using TilePathGame.Validation;
using TilePathGame.Map;
using UnityEngine;
using System.Linq;

public class MapManager : MonoBehaviour
{
    [SerializeField] private List<TileData> _tileDatas = new List<TileData>();

    [Header("Scene References")]
    [SerializeField] private Transform _tileContainer;
    [SerializeField] private SimpleMapGenerator _mapGenerator;

    [Header("Layout Settings")]
    [SerializeField] private float _tileSpacing = 2.0f;
    [SerializeField] private TileInstance _tileInstancePf;

    [Header("Game Settings")]
    [SerializeField] private bool _enableGlobalValidation = true;
    [SerializeField] private Vector2Int _startPosition = Vector2Int.zero;
    [SerializeField] private Vector2Int _goalPosition = new Vector2Int(4, 4);

    // Internal systems
    private TilePool _tilePool;
    private Dictionary<TileInstance, TileData> _spawnedTileMap = new Dictionary<TileInstance, TileData>();

    // Properties
    public ITilePool TilePool => _tilePool;
    public Vector2Int StartPosition => _startPosition;
    public Vector2Int GoalPosition => _goalPosition;
    public bool IsGlobalValidationEnabled => _enableGlobalValidation;

    void Start()
    {
        InitializeTilePool();
        SpawnTilesFromList();
    }

    private void InitializeTilePool()
    {
        _tilePool = new TilePool(_tileDatas);
    }

    private void SetupStartAndGoalTiles()
    {
        if (_mapGenerator == null) return;

        var mapTiles = SimpleMapGenerator.GetMapTiles();
        if (mapTiles == null || mapTiles.Length == 0) return;

        // Find and set start tile
        var startTile = mapTiles.FirstOrDefault(tile => tile != null && tile.GridPosition == _startPosition);
        if (startTile != null)
        {
            startTile.SetSpecialType(ESpecialTileType.StartTile);
            Debug.Log($"[MapManager] Set start tile at {_startPosition}");
        }

        // Find and set goal tile
        var goalTile = mapTiles.FirstOrDefault(tile => tile != null && tile.GridPosition == _goalPosition);
        if (goalTile != null)
        {
            goalTile.SetSpecialType(ESpecialTileType.GoalTile);
            Debug.Log($"[MapManager] Set goal tile at {_goalPosition}");
        }
    }

    private void SpawnTilesFromList()
    {
        // Clear any existing tiles in the container first
        ClearAllTilesFromContainer();
        _spawnedTileMap.Clear();

        // Spawn tiles from available pool
        var availableTiles = _tilePool.GetAvailableTiles();
        for (int i = 0; i < availableTiles.Count; i++)
        {
            TileData data = availableTiles[i];
            if (data == null) continue;

            // Instantiate the tile instance
            GameObject newTileGO = Instantiate(_tileInstancePf.gameObject, _tileContainer);

            // Position it in the container
            newTileGO.transform.localPosition = new Vector3(i * _tileSpacing, 0, 0);
            newTileGO.name = $"DraggableTile_{data.TileType}_{i}";

            // Initialize the instance with its data
            var tileInstance = newTileGO.GetComponent<TileInstance>();
            tileInstance.Init(data);

            // Track the spawned tile
            _spawnedTileMap[tileInstance] = data;
        }
    }

    private void CheckWinCondition()
    {
        if (!_enableGlobalValidation) return;

        var mapTiles = SimpleMapGenerator.GetMapTiles();
        if (mapTiles == null) return;

        var grid = TileValidator.CreatePartialGridFromMapTiles(mapTiles);
        var pathResult = PathfindingValidator.IsPathPossible(_startPosition, _goalPosition, grid);

        if (pathResult.isCurrentlyConnected)
        {
            OnGameWon();
        }
        else if (!pathResult.isPathPossible)
        {
            OnGameLost();
        }
    }

    private void OnGameWon()
    {
    }

    private void OnGameLost()
    {
    }

    public void RespawnTiles()
    {
        if (_tilePool != null)
        {
            _tilePool.Reset();
        }
        else
        {
            InitializeTilePool();
        }
        SpawnTilesFromList();
    }

    private void ClearAllTilesFromContainer()
    {
        if (_tileContainer == null) return;

        Transform[] children = new Transform[_tileContainer.childCount];
        for (int i = 0; i < _tileContainer.childCount; i++)
        {
            children[i] = _tileContainer.GetChild(i);
        }

        foreach (Transform child in children)
        {
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }
    }

    [ContextMenu("Check Win Condition")]
    private void DebugCheckWinCondition()
    {
        CheckWinCondition();
    }
}
