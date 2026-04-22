using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Map : MonoBehaviour
{
    [Header("Map Settings")]
    public GameObject tilePrefab;
    public int horizontalSize = 6;
    public int verticalSize = 10;

    [Header("Spacing")]
    public float spacing = 1.0f;

    [Header("Data Asset")]
    public MapData mapData;

    public Tile[,] Tiles { get; private set; }

    private void Awake()
    {
        // 게임 시작 시 자동으로 배열을 다시 채웁니다.
        RebuildTilesArray();
    }

    // 현재 씬의 자식 오브젝트들을 조사하여 Tiles 배열을 다시 구성합니다.
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

        // 데이터 에셋이 있다면 크기를 동기화합니다.
        if (mapData != null)
        {
            horizontalSize = mapData.width;
            verticalSize = mapData.height;
        }

        Tiles = new Tile[horizontalSize, verticalSize];

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
                    
                    // 데이터 에셋에 저장된 높이가 있다면 적용합니다.
                    if (mapData != null)
                    {
                        tile.SetHeightValue(mapData.GetHeight(x, z));
                    }
                    
                    Tiles[x, z] = tile;
                }
            }
        }
    }

    // 현재 씬에 배치된 타일들의 정보를 MapData 에셋에 저장합니다.
    [ContextMenu("Save Scene To MapData")]
    public void SaveToData()
    {
#if UNITY_EDITOR
        if (mapData == null)
        {
            Debug.LogError("저장할 MapData 에셋이 할당되지 않았습니다!");
            return;
        }

        mapData.Initialize(horizontalSize, verticalSize);

        Tile[] allTiles = GetComponentsInChildren<Tile>();
        foreach (var t in allTiles)
        {
            mapData.SetHeight(t.x, t.z, t.height);
        }

        EditorUtility.SetDirty(mapData);
        AssetDatabase.SaveAssets();
        Debug.Log("현재 맵 정보를 MapData 에셋에 저장했습니다.");
#endif
    }

    private void ClearMap()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        Tiles = null;
    }

    // 특정 좌표의 타일을 반환합니다.
    public Tile GetTile(int x, int z)
    {
        if (Tiles == null || x < 0 || x >= horizontalSize || z < 0 || z >= verticalSize) return null;
        return Tiles[x, z];
    }

    // 에디터와 런타임에서 안전하게 오브젝트를 생성합니다.
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
