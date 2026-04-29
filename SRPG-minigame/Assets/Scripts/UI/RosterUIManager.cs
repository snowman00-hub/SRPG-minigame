using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class RosterUIManager : MonoBehaviour
{
    [Header("Windows & Buttons")]
    [SerializeField] private GameObject rosterWindow;
    [SerializeField] private Button rosterOpenButton;

    [Header("Right Side - Slots")]
    [SerializeField] private Transform slotContainer;
    [SerializeField] private RosterUnitSlot[] existingSlots; // 씬에 미리 배치된 슬롯들

    [Header("Left Side - Info Panel")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI jobText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI lvExpText;
    [SerializeField] private TextMeshProUGUI deployCountText; // 출격 유닛 7/7 표시용

    private List<UnitSaveData> currentUnitSaves = new List<UnitSaveData>();
    private Dictionary<string, UnitData> unitDataCache = new Dictionary<string, UnitData>();

    // --- 캐싱 및 인풋 ---
    private Map map; // Start에서 캐싱하여 직접 사용
    private GameInput inputActions; 
    private UnitSaveData currentViewingSaveData;
    private RosterUnitSlot currentViewingSlot;

    private void Awake()
    {
        inputActions = new GameInput();

        if (rosterWindow != null) rosterWindow.SetActive(false);
        if (rosterOpenButton != null) rosterOpenButton.onClick.AddListener(ToggleWindow);

        if (slotContainer != null && (existingSlots == null || existingSlots.Length == 0))
        {
            existingSlots = slotContainer.GetComponentsInChildren<RosterUnitSlot>(true);
        }
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Start()
    {
        // 씬에서 Map을 찾아 캐싱합니다.
        map = Object.FindFirstObjectByType<Map>();
        LoadAllUnits();
    }

    private void Update()
    {
        if (rosterWindow != null && rosterWindow.activeSelf && currentViewingSaveData != null)
        {
            if (inputActions.Player.Confirm.triggered)
            {
                ToggleDeployment(currentViewingSaveData, currentViewingSlot);
            }
        }
    }

    public void LoadAllUnits()
    {
        GameSaveData saveData = SaveSystem.Load();
        currentUnitSaves = saveData.unitList;
        RefreshRoster();
    }

    public void ToggleWindow()
    {
        if (rosterWindow == null) return;
        bool isActive = !rosterWindow.activeSelf;
        rosterWindow.SetActive(isActive);

        if (isActive)
        {
            LoadAllUnits();
        }
    }

    private void RefreshRoster()
    {
        if (unitDataCache.Count == 0)
        {
            var global = GlobalStatSettings.Instance;
            if (global != null && global.allUnitTemplates != null)
            {
                foreach (var t in global.allUnitTemplates) unitDataCache[t.name] = t;
            }
        }

        for (int i = 0; i < existingSlots.Length; i++)
        {
            if (i < currentUnitSaves.Count)
            {
                UnitSaveData saveData = currentUnitSaves[i];
                unitDataCache.TryGetValue(saveData.unitDataName, out UnitData data);
                
                RosterUnitSlot slot = existingSlots[i];
                slot.Setup(data, saveData, (clickedData) => SelectUnit(clickedData, saveData, slot));
                slot.gameObject.SetActive(true);

                if (map != null)
                {
                    bool isDeployed = map.deploymentList.Any(d => d.unitDataName == saveData.unitDataName);
                    slot.SetHighlight(isDeployed);
                }

                if (i == 0 && data != null)
                {
                    SelectUnit(data, saveData, slot);
                }
            }
            else
            {
                existingSlots[i].Setup(null, null, null);
                existingSlots[i].gameObject.SetActive(true);
            }
        }

        UpdateDeployCountText();
    }

    private void SelectUnit(UnitData data, UnitSaveData saveData, RosterUnitSlot clickedSlot)
    {
        if (data == null) return;
        currentViewingSaveData = saveData;
        currentViewingSlot = clickedSlot;
        UpdateInfoPanel(data, saveData);
    }

    private void ToggleDeployment(UnitSaveData saveData, RosterUnitSlot slot)
    {
        if (map == null) return;

        var existing = map.deploymentList.FirstOrDefault(d => d.unitDataName == saveData.unitDataName);

        if (existing != null)
        {
            map.deploymentList.Remove(existing);
            slot.SetHighlight(false);
        }
        else
        {
            if (map.deploymentList.Count < map.mapData.maxDeployableUnits)
            {
                map.deploymentList.Add(saveData);
                slot.SetHighlight(true);
            }
            else
            {
                Debug.Log("출전 인원이 가득 찼습니다!");
            }
        }
        
        map.SpawnInitialUnits();
        UpdateDeployCountText();
    }

    private void UpdateDeployCountText()
    {
        if (deployCountText == null || map == null || map.mapData == null) return;
        // 요청하신 대로 넓은 공백을 유지합니다. (\t는 탭 문자입니다)
        deployCountText.text = $"출격 유닛\t\t\t{map.deploymentList.Count}/{map.mapData.maxDeployableUnits}";
    }

    private void UpdateInfoPanel(UnitData data, UnitSaveData saveData)
    {
        if (portraitImage != null) portraitImage.sprite = data.unitPortrait != null ? data.unitPortrait : data.unitIcon;
        if (nameText != null) nameText.text = data.unitName;
        if (jobText != null) jobText.text = data.description;
        
        if (lvExpText != null && saveData != null)
        {
            lvExpText.text = $"Lv. {saveData.level} | EXP {saveData.currentEXP}";
        }

        if (statsText != null)
        {
            int maxHP = UnitStats.GetMaxHP(data, GlobalStatSettings.Instance, saveData.level);
            statsText.text = $"HP.{maxHP}/{maxHP}";
        }
    }
}
