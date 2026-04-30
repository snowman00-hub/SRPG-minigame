using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Map : MonoBehaviour
{
    [Header("Map Settings")]
    public GameObject tilePrefab;
    public GameObject spawnMarkerPrefab; // 아군 배치 구역 표시 프리팹
    public List<GameObject> unitPrefabs;
    public int horizontalSize = 6;
    public int verticalSize = 10;

    [Header("Spacing")]
    public float spacing = 1.0f;

    [Header("Data Asset")]
    public MapData mapData;

    [Header("Player Spawning")]
    public GameObject playerUnitBasePrefab; // character_base
    public List<Unit> spawnedPlayerUnits = new List<Unit>();
    public List<UnitSaveData> deploymentList = new List<UnitSaveData>(); // 현재 출전 확정된 명단 (임시)

    public Tile[,] Tiles { get; private set; }
    private Dictionary<Vector2Int, SpawnMarker> activeMarkers = new Dictionary<Vector2Int, SpawnMarker>(); // 현재 활성화된 마커들
    private bool isInitialDeploymentDone = false; // 처음에 한 번 채웠는지 체크용

    private void Awake()
    {
        RebuildTilesArray();
    }

    private void Start()
    {
        // 1. 처음 시작할 때 세이브에서 자동으로 최대 인원만큼 명단 채우기
        var save = SaveSystem.Load();
        deploymentList.Clear();
        if (save != null && save.unitList != null && mapData != null)
        {
            int autoCount = Mathf.Min(save.unitList.Count, mapData.maxDeployableUnits);
            for (int i = 0; i < autoCount; i++)
            {
                deploymentList.Add(save.unitList[i]);
            }
        }

        isInitialDeploymentDone = true; // 게임 시작 시 한 번 채웠음을 기록
        SpawnInitialUnits();
    }

    [ContextMenu("Spawn Initial Units")]
    public void SpawnInitialUnits()
    {
        if (playerUnitBasePrefab == null || mapData == null)
        {
            Debug.LogError("Map: 프리팹이나 맵 데이터가 할당되지 않았습니다!");
            return;
        }

        // 3. 기존 소환된 유닛 제거
        foreach (var u in spawnedPlayerUnits) 
        {
            if (u != null)
            {
                Tile tile = GetTile(u.x, u.z);
                if (tile != null && tile.unitOnTile == u) tile.unitOnTile = null;

                if (Application.isPlaying) Destroy(u.gameObject);
                else DestroyImmediate(u.gameObject);
            }
        }
        spawnedPlayerUnits.Clear();

        // 4. 출전 명단 확인 (에디터 전용 또는 완전 초기화용)
        // 이미 한 번 채워졌거나 명단에 내용이 있다면 더 이상 자동으로 채우지 않습니다.
        if (!isInitialDeploymentDone && deploymentList.Count == 0)
        {
            var save = SaveSystem.Load();
            if (save != null && save.unitList != null && mapData != null)
            {
                int autoCount = Mathf.Min(save.unitList.Count, mapData.maxDeployableUnits);
                for (int i = 0; i < autoCount; i++) deploymentList.Add(save.unitList[i]);
            }
        }

        int deployCount = Mathf.Min(deploymentList.Count, mapData.maxDeployableUnits);
        int spawnPointCount = mapData.playerSpawnPoints.Count;

        if (spawnPointCount == 0)
        {
            Debug.LogWarning("Map: 맵 데이터에 설정된 스폰 포인트가 없습니다!");
            return;
        }

        // 5. 배치 시작
        for (int i = 0; i < deployCount && i < spawnPointCount; i++)
        {
            var saveData = deploymentList[i];
            string targetID = saveData.unitDataID;
            UnitData data = GlobalStatSettings.Instance.GetUnitData(targetID);
            
            if (data != null)
            {
                Vector2Int spawnPos = mapData.playerSpawnPoints[i];
                Tile tile = GetTile(spawnPos.x, spawnPos.y); 
                
                if (tile == null)
                {
                    Debug.LogWarning($"Map: ({spawnPos.x}, {spawnPos.y}) 위치에 타일이 없습니다.");
                    continue;
                }

                // 유닛 소환
                GameObject unitGo = Instantiate(playerUnitBasePrefab);
                unitGo.name = $"Player_{data.unitName}";
                Unit unit = unitGo.GetComponent<Unit>();
                
                if (unit != null)
                {
                    unit.team = Team.Player;
                    unit.ApplySaveData(saveData, data);
                    // SetPosition 내부에서 타일 정보 자동 갱신
                    unit.SetPosition(spawnPos.x, spawnPos.y, this);
                    
                    spawnedPlayerUnits.Add(unit);
                }
            }
        }

        Debug.Log($"Map: {spawnedPlayerUnits.Count}명의 유닛을 자동으로 배치했습니다.");
    }

    // 개별 유닛 스폰 (런타임 로스터 UI 조작용 최적화)
    public void SpawnUnit(UnitSaveData saveData)
    {
        if (playerUnitBasePrefab == null || mapData == null) return;
        
        string targetID = saveData.unitDataID;
        UnitData data = GlobalStatSettings.Instance.GetUnitData(targetID);
        if (data == null) return;

        // 이미 스폰되어 있는지 확인
        if (spawnedPlayerUnits.Any(u => u != null && u.unitData == data)) return;

        for (int i = 0; i < mapData.playerSpawnPoints.Count; i++)
        {
            Vector2Int spawnPos = mapData.playerSpawnPoints[i];
            Tile tile = GetTile(spawnPos.x, spawnPos.y);
            // 비어있는 스폰 포인트 찾기
            if (tile != null && tile.unitOnTile == null)
            {
                GameObject unitGo = Instantiate(playerUnitBasePrefab);
                unitGo.name = $"Player_{data.unitName}";
                Unit unit = unitGo.GetComponent<Unit>();
                
                if (unit != null)
                {
                    unit.team = Team.Player;
                    unit.ApplySaveData(saveData, data);
                    unit.SetPosition(spawnPos.x, spawnPos.y, this);
                    spawnedPlayerUnits.Add(unit);
                }
                break; // 한 명 배치 후 종료
            }
        }
    }

    // 개별 유닛 제거 (런타임 로스터 UI 조작용 최적화)
    public void RemoveUnit(UnitSaveData saveData)
    {
        string targetID = saveData.unitDataID;
        UnitData data = GlobalStatSettings.Instance.GetUnitData(targetID);
        if (data == null) return;

        Unit unitToRemove = spawnedPlayerUnits.FirstOrDefault(u => u != null && u.unitData == data);
        if (unitToRemove != null)
        {
            // 논리적 위치 비우기
            Tile tile = GetTile(unitToRemove.x, unitToRemove.z);
            if (tile != null && tile.unitOnTile == unitToRemove) tile.unitOnTile = null;

            spawnedPlayerUnits.Remove(unitToRemove);
            if (Application.isPlaying) Destroy(unitToRemove.gameObject);
            else DestroyImmediate(unitToRemove.gameObject);
        }
    }

    public void RebuildTilesArray()
    {
        Tiles = new Tile[horizontalSize, verticalSize];
        Tile[] foundTiles = GetComponentsInChildren<Tile>();

        foreach (Tile tile in foundTiles)
        {
            if (tile.x >= 0 && tile.x < horizontalSize && tile.z >= 0 && tile.z < verticalSize)
            {
                Tiles[tile.x, tile.z] = tile;
            }
        }
    }

    [ContextMenu("Generate Map")]
    public void GenerateMap()
    {
        ClearMap();

        if (tilePrefab == null)
        {
            Debug.LogError("Tile Prefab이 할당되지 않았습니다!");
            return;
        }

        if (mapData != null)
        {
            horizontalSize = mapData.width;
            verticalSize = mapData.height;
        }

        Tiles = new Tile[horizontalSize, verticalSize];

        // 1. 타일 생성
        for (int z = 0; z < verticalSize; z++)
        {
            for (int x = 0; x < horizontalSize; x++)
            {
                Vector3 position = new Vector3(x * spacing, 0, z * spacing);
                GameObject tileGo = InstantiateObject(tilePrefab, position, Quaternion.identity, transform);
                tileGo.name = $"Tile_{x}_{z}";

                Tile tile = tileGo.GetComponent<Tile>();
                if (tile != null)
                {
                    tile.x = x;
                    tile.z = z;
                    if (mapData != null) tile.SetHeightValue(mapData.GetHeight(x, z));
                    Tiles[x, z] = tile;
                }
            }
        }

        // 2. 적군 생성
        if (mapData != null && unitPrefabs != null)
        {
            foreach (var enemyInfo in mapData.enemies)
            {
                GameObject prefab = unitPrefabs.Find(p => p != null && p.name == enemyInfo.enemyID);
                if (prefab != null)
                {
                    GameObject unitGo = InstantiateObject(prefab, Vector3.zero, Quaternion.identity, transform);
                    unitGo.name = $"Enemy_{enemyInfo.enemyID}_{enemyInfo.x}_{enemyInfo.z}";
                    Unit unit = unitGo.GetComponent<Unit>();
                    if (unit != null) unit.SetPosition(enemyInfo.x, enemyInfo.z, this);
                }
            }
        }

        // 3. 아군 배치 마커 생성
        ShowSpawnMarkers();
    }

    // 마커들을 생성하고 표시합니다.
    public void ShowSpawnMarkers()
    {
        ClearMarkers();
        
        if (mapData == null || spawnMarkerPrefab == null) return;

        foreach (Vector2Int spawnPos in mapData.playerSpawnPoints)
        {
            Tile tile = GetTile(spawnPos.x, spawnPos.y);
            if (tile != null)
            {
                tile.isSpawnPoint = true;
                // 타일 윗면에 배치
                Vector3 pos = tile.transform.position + Vector3.up * 0.53f; 
                GameObject markerObj = InstantiateObject(spawnMarkerPrefab, pos, Quaternion.Euler(90, 0, 0), transform);
                markerObj.name = $"SpawnMarker_{spawnPos.x}_{spawnPos.y}";
                
                SpawnMarker marker = markerObj.GetComponent<SpawnMarker>();
                if (marker != null)
                {
                    activeMarkers.Add(spawnPos, marker);
                    marker.SetHighlight(false); // 초기 상태는 일반
                }
            }
        }
    }

    // 마커들을 모두 제거합니다.
    public void ClearMarkers()
    {
        foreach (var kvp in activeMarkers)
        {
            if (kvp.Value != null) 
            {
                if (Application.isPlaying) Destroy(kvp.Value.gameObject);
                else DestroyImmediate(kvp.Value.gameObject);
            }
        }
        activeMarkers.Clear();

        // 모든 타일의 스폰 포인트 상태 리셋
        if (Tiles != null)
        {
            foreach (var tile in Tiles)
            {
                if (tile != null) tile.isSpawnPoint = false;
            }
        }
        
        // 이름으로도 한 번 더 체크해서 청소 (에디터용)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (transform.GetChild(i).name.StartsWith("SpawnMarker_"))
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
    }

    // 특정 좌표에 있는 스폰 마커의 하이라이트(선택됨 표시)를 설정합니다.
    public void SetMarkerHighlight(int x, int z, bool isSelected)
    {
        Vector2Int pos = new Vector2Int(x, z);
        if (activeMarkers.ContainsKey(pos))
        {
            activeMarkers[pos].SetHighlight(isSelected);
        }
    }

    [ContextMenu("Save Scene To MapData")]
    public void SaveToData()
    {
#if UNITY_EDITOR
        if (mapData == null) return;
        mapData.Initialize(horizontalSize, verticalSize);
        Tile[] allTiles = GetComponentsInChildren<Tile>();
        foreach (var t in allTiles) mapData.SetHeight(t.x, t.z, t.height);
        EditorUtility.SetDirty(mapData);
        AssetDatabase.SaveAssets();
#endif
    }

    private void ClearMap()
    {
        ClearMarkers();
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        Tiles = null;
    }

    public Tile GetTile(int x, int z)
    {
        if (Tiles == null || x < 0 || x >= horizontalSize || z < 0 || z >= verticalSize) return null;
        return Tiles[x, z];
    }

    private GameObject InstantiateObject(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.position = position;
            go.transform.rotation = rotation;
            return go;
        }
#endif
        return Instantiate(prefab, position, rotation, parent);
    }
}
