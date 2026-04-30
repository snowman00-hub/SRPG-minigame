using UnityEngine;
using UnityEngine.UI;

public class BattlePrepManager : MonoBehaviour
{
    [Header("References")]
    public Map map;
    public GridPlacementSystem gridPlacement;

    [Header("UI Buttons")]
    public Button placementButton; // '배치 모드' 버튼
    public Button startButton;     // '전투 시작' 버튼

    private bool isPlacementMode = false;

    private void Start()
    {
        // 버튼 이벤트 연결
        if (placementButton != null)
            placementButton.onClick.AddListener(TogglePlacementMode);

        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);
    }

    public void TogglePlacementMode()
    {
        isPlacementMode = !isPlacementMode;
        Debug.Log($"배치 모드 {(isPlacementMode ? "활성화" : "비활성화")}");

        if (map != null)
        {
            if (isPlacementMode) map.ShowSpawnMarkers();
            else map.ClearMarkers();
        }

        if (gridPlacement != null)
        {
            gridPlacement.SetActive(isPlacementMode);
        }
    }

    private void OnStartClicked()
    {
        Debug.Log("전투 시작! 모든 마커와 커서를 정리합니다.");
        isPlacementMode = false;

        if (map != null) map.ClearMarkers();
        if (gridPlacement != null) gridPlacement.SetActive(false);

        // 이후 실제 전투 시작 로직을 여기에 추가하시면 됩니다.
    }
}
