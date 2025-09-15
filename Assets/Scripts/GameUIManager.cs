using System.Collections;
using System.Collections.Generic;
using TilePathGame.Map;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [SerializeField] private Dropdown selectMapDropDown;
    [SerializeField] private List<MapSettings> mapSettings;
    [SerializeField] private SimpleMapGenerator simpleMapGenerator;
    [SerializeField] private MapManager mapManager; // Reference to tile spawner

    #region UI Element

    [SerializeField] private Button restartBtn;
    [SerializeField] private GameObject completeObj;
    [SerializeField] private GameObject gameLostObj;

    #endregion

    private void Start()
    {
        // Xóa options cũ
        selectMapDropDown.ClearOptions();

        // Lấy danh sách tên map
        List<string> mapNames = new List<string>();
        foreach (var map in mapSettings)
        {
            mapNames.Add($"Map{map.Level}"); // giả sử MapSettings có field MapName
        }

        // Thêm vào dropdown
        selectMapDropDown.AddOptions(mapNames);

        // Lắng nghe sự kiện chọn
        selectMapDropDown.onValueChanged.AddListener(OnMapSelected);
        restartBtn.onClick.AddListener(RestartRound);
    }

    private void OnDisable()
    {
        selectMapDropDown.onValueChanged.RemoveListener(OnMapSelected);
        restartBtn.onClick.RemoveListener(RestartRound);
    }

    private void OnMapSelected(int index)
    {
        MapSettings selectedMap = mapSettings[index];
        Debug.Log("Selected map: " + selectedMap.Level);
        simpleMapGenerator.SetMapSettings(selectedMap);
        simpleMapGenerator.Generate();
        // TODO: load map này hoặc apply setting
    }

    private void RestartRound()
    {
        StartCoroutine(RestartRoundCoroutine());
    }
    
    private IEnumerator RestartRoundCoroutine()
    {
        // Ẩn UI thắng/thua nếu đang hiển thị
        if (completeObj != null)
            completeObj.SetActive(false);
        if (gameLostObj != null)
            gameLostObj.SetActive(false);
        
        // Step 1: Clear placed tiles from map (keep empty MapTile slots)
        if (simpleMapGenerator != null)
        {
            simpleMapGenerator.ClearPlacedTiles();
        }

        // Wait one frame to ensure Destroy operations complete
        yield return null;
        
        // Step 2: Respawn draggable tiles in their original container
        if (mapManager != null)
        {
            mapManager.RespawnTiles();
        }
    }
    
    /// <summary>
    /// Public method to restart game from code (not just button)
    /// </summary>
    public void RestartGame()
    {
        RestartRound();
    }
    
    /// <summary>
    /// Show game complete UI
    /// </summary>
    public void ShowGameComplete()
    {
        if (completeObj != null)
            completeObj.SetActive(true);
        Debug.Log("[GameUIManager] Game Complete!");
    }
    
    /// <summary>
    /// Show game lost UI
    /// </summary>
    public void ShowGameLost()
    {
        if (gameLostObj != null)
            gameLostObj.SetActive(true);
        Debug.Log("[GameUIManager] Game Lost!");
    }
}
