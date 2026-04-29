using UnityEngine;

public enum Team { Player, Enemy }

public class Unit : MonoBehaviour
{
    [Header("Unit Settings")]
    [HideInInspector] public string unitName;
    public Team team;
    public UnitData unitData;
    public int startingLevel = 1;

    [HideInInspector] public GlobalStatSettings globalSettings;
    [HideInInspector] public UnitStats stats;
    [HideInInspector] public bool hasMoved;
    [HideInInspector] public bool hasActed;

    private GameObject currentModel;

    private void Start()
    {
        if (unitData != null && (stats == null || stats.level <= 1))
        {
            Initialize(unitData, startingLevel);
        }
    }

    public void Initialize(UnitData data, int level = 1)
    {
        unitData = data;
        unitName = data.unitName;
        if (stats == null) stats = new UnitStats();
        
        // 비주얼 모델 생성
        UpdateVisual();

        // 만약 globalSettings가 할당되지 않았다면 Resources에서 찾아봄
        if (globalSettings == null) globalSettings = GlobalStatSettings.Instance;
        
        stats.Initialize(data, globalSettings, level);
    }

    public void UpdateVisual()
    {
        // 기존 모델이 있다면 삭제
        if (currentModel != null)
        {
            if (Application.isPlaying) Destroy(currentModel);
            else DestroyImmediate(currentModel);
        }

        // 새로운 모델 소환
        if (unitData != null && unitData.unitPrefab != null)
        {
            currentModel = Instantiate(unitData.unitPrefab, transform);
            currentModel.transform.localPosition = Vector3.zero;
            currentModel.transform.localRotation = Quaternion.identity;
            currentModel.name = "Model_" + unitData.unitName;
        }
    }
    
    [Header("Grid Position")]
    public int x;
    public int z;

    // 유닛을 그리드 좌표에 즉시 배치합니다.
    public void SetPosition(int targetX, int targetZ, Map map)
    {
        // 1. 기존에 서 있던 타일이 있다면 비워줍니다 (Desync 방지)
        Tile currentTile = map.GetTile(x, z);
        if (currentTile != null && currentTile.unitOnTile == this)
        {
            currentTile.unitOnTile = null;
        }

        x = targetX;
        z = targetZ;

        // 2. 새 타일에 자신을 등록합니다.
        Tile newTile = map.GetTile(x, z);
        if (newTile != null)
        {
            newTile.unitOnTile = this;
            
            // 타일의 높이(y)를 기준으로 유닛을 살짝 위에 띄웁니다.
            float yPos = newTile.transform.position.y + 0.5f; 
            transform.position = new Vector3(x * map.spacing, yPos, z * map.spacing);
        }
    }
    
    // 유닛의 시각적 위치를 현재 그리드 좌표와 맵 상태에 맞춰 업데이트합니다.
    public void UpdateVisualPosition(Map map)
    {
        SetPosition(x, z, map);
    }

    public void OnTurnStart()
    {
        hasMoved = false;
        hasActed = false;
        stats.RestoreEnergy();
    }

    // 세이브 데이터를 유닛에 적용합니다.
    public void ApplySaveData(UnitSaveData data, UnitData scriptableObject)
    {
        Initialize(scriptableObject, data.level);
        stats.currentEXP = data.currentEXP;
    }
}
