using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class BattlePrepManager : MonoBehaviour
{
    [Header("References")]
    public UIDocument uiDocument;
    public Map map;

    private VisualElement selectionPanel;
    private Button btnMembers;
    private Button btnPlacement;
    private Button btnStart;

    void OnEnable()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;

        var root = uiDocument.rootVisualElement;
        
        // UI 요소 찾기
        selectionPanel = root.Q<VisualElement>("selection-panel");
        btnMembers = root.Q<Button>("btn-members");
        btnPlacement = root.Q<Button>("btn-placement");
        btnStart = root.Q<Button>("btn-start");

        // 이벤트 연결
        btnMembers.clicked += OnMembersClicked;
        btnPlacement.clicked += OnPlacementClicked;
        btnStart.clicked += OnStartClicked;
    }

    private void OnMembersClicked()
    {
        if (selectionPanel == null) return;
        
        bool isHidden = selectionPanel.ClassListContains("hidden");
        if (isHidden) selectionPanel.RemoveFromClassList("hidden");
        else selectionPanel.AddToClassList("hidden");
        
        Debug.Log("출전 멤버 선택창 토글");
    }

    private void OnPlacementClicked()
    {
        Debug.Log("배치 모드 활성화: 스폰 마커를 표시합니다.");
        if (map != null) map.ShowSpawnMarkers();
    }

    private void OnStartClicked()
    {
        Debug.Log("전투 시작! 모든 마커를 제거하고 게임 루프로 진입합니다.");
        if (map != null) map.ClearMarkers();
        
        // 여기서 실제 전투 시퀀스나 턴 매니저를 실행할 수 있습니다.
        if (selectionPanel != null) selectionPanel.AddToClassList("hidden");
    }
}
