using UnityEngine;
using System.Collections.Generic;
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

    public Tile[,] Tiles { get; private set; }
    private List<GameObject> activeMarkers = new List<GameObject>(); // 현재 활성화된 마커들

    private void Awake()
    {
        RebuildTilesArray();
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
                // 타일 윗면에 배치
                Vector3 pos = tile.transform.position + Vector3.up * 0.53f; 
                GameObject marker = InstantiateObject(spawnMarkerPrefab, pos, Quaternion.Euler(90, 0, 0), transform);
                marker.name = $"SpawnMarker_{spawnPos.x}_{spawnPos.y}";
                activeMarkers.Add(marker);
            }
        }
    }

    // 마커들을 모두 제거합니다.
    public void ClearMarkers()
    {
        foreach (var marker in activeMarkers)
        {
            if (marker != null) DestroyImmediate(marker);
        }
        activeMarkers.Clear();
        
        // 이름으로도 한 번 더 체크해서 청소 (에디터용)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (transform.GetChild(i).name.StartsWith("SpawnMarker_"))
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
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
