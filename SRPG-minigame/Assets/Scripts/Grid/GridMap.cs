using UnityEngine;
using System.Collections.Generic;
using System;

public enum UnitType
{
    None = 0,
    Character = 1,
    Enemy = 2
}

[Serializable]
public class UnitPlacement
{
    public UnitType unitType;
    public JobData jobData; // Added JobData reference
    public int gridX;
    public int gridZ;
    [HideInInspector] public GameObject instance;

    public UnitPlacement(UnitType type, int x, int z, JobData job = null)
    {
        unitType = type;
        gridX = x;
        gridZ = z;
        jobData = job;
        instance = null;
    }
}

public class GridMap : MonoBehaviour
{
    [Header("Grid Settings")]
    public GameObject tilePrefab;
    [Min(1)] public int gridWidth = 6;
    [Min(1)] public int gridHeight = 8;

    [Header("Scale Settings")]
    public float heightScale = 0.5f;
    public float tileSpacingX = 1f;
    public float tileSpacingZ = 1f;

    [Header("Unit Prefabs")]
    public GameObject characterPrefab;
    public GameObject enemyPrefab;
    public float unitYOffset = 0.5f;

    [Header("Height Map")]
    public int[] heightMap;

    [Header("Unit Placements")]
    public List<UnitPlacement> unitPlacements = new List<UnitPlacement>();

    [HideInInspector]
    public List<GameObject> tileInstances = new List<GameObject>();

    // ========== Height Map ==========
    public void InitializeHeightMap()
    {
        int newSize = gridWidth * gridHeight;
        if (heightMap == null || heightMap.Length == 0)
        {
            heightMap = new int[newSize];
            return;
        }
        if (heightMap.Length == newSize) return;
        int[] oldMap = heightMap;
        heightMap = new int[newSize];
        int copyLen = Mathf.Min(oldMap.Length, newSize);
        Array.Copy(oldMap, heightMap, copyLen);
    }

    public int GetHeight(int x, int z)
    {
        if (x < 0 || x >= gridWidth || z < 0 || z >= gridHeight) return 0;
        int index = z * gridWidth + x;
        if (heightMap == null || index >= heightMap.Length) return 0;
        return heightMap[index];
    }

    public void SetHeight(int x, int z, int height)
    {
        if (x < 0 || x >= gridWidth || z < 0 || z >= gridHeight) return;
        int index = z * gridWidth + x;
        if (heightMap == null || index >= heightMap.Length) return;
        heightMap[index] = height;
    }

    public Vector3 GridToWorld(int x, int z, int h)
    {
        return new Vector3(x * tileSpacingX, h * heightScale, z * tileSpacingZ);
    }

    public Vector3 GetUnitWorldPos(int x, int z)
    {
        int h = GetHeight(x, z);
        return GridToWorld(x, z, h) + new Vector3(0, unitYOffset, 0);
    }

    public bool WorldToGrid(Vector3 worldPos, out int gx, out int gz)
    {
        gx = Mathf.RoundToInt(worldPos.x / tileSpacingX);
        gz = Mathf.RoundToInt(worldPos.z / tileSpacingZ);
        return gx >= 0 && gx < gridWidth && gz >= 0 && gz < gridHeight;
    }

    // ========== Tiles ==========
    public void ClearTiles()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i).gameObject;
            if (child.name.StartsWith("Tile")) // Only clear tiles if needed, or clear all
                DestroyImmediate(child);
        }
        tileInstances.Clear();
    }

    public void GenerateMap()
    {
        if (tilePrefab == null) { Debug.LogError("[GridMap] Tile prefab not assigned!"); return; }

        ClearTiles();
        // ClearUnits(); // We usually want to keep units if regenerating tiles, but for consistency:
        // Actually, let's keep units and just update their positions.
        
        InitializeHeightMap();
        tileInstances = new List<GameObject>(gridWidth * gridHeight);

        for (int z = 0; z < gridHeight; z++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                int h = GetHeight(x, z);
                var tile = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(tilePrefab);
                tile.name = "Tile_" + x + "_" + z + "_H" + h;
                tile.transform.position = GridToWorld(x, z, h);
                tile.transform.SetParent(transform);
                tileInstances.Add(tile);
            }
        }
        
        RespawnAllUnits();
    }

    public void UpdateTileHeight(int x, int z)
    {
        int index = z * gridWidth + x;
        if (index < 0 || index >= tileInstances.Count || tileInstances[index] == null) return;
        int h = GetHeight(x, z);
        tileInstances[index].transform.position = GridToWorld(x, z, h);
        tileInstances[index].name = "Tile_" + x + "_" + z + "_H" + h;

        foreach (var u in unitPlacements)
            if (u.gridX == x && u.gridZ == z && u.instance != null)
                u.instance.transform.position = GetUnitWorldPos(x, z);
    }

    public void RefreshAllTilePositions()
    {
        for (int z = 0; z < gridHeight; z++)
            for (int x = 0; x < gridWidth; x++)
                UpdateTileHeight(x, z);
    }

    // ========== Units ==========
    public UnitType GetUnitTypeAt(int x, int z)
    {
        foreach (var u in unitPlacements)
            if (u.gridX == x && u.gridZ == z) return u.unitType;
        return UnitType.None;
    }

    public UnitPlacement GetUnitPlacementAt(int x, int z)
    {
        foreach (var u in unitPlacements)
            if (u.gridX == x && u.gridZ == z) return u;
        return null;
    }

    public void PlaceUnit(UnitType type, int x, int z, JobData job = null)
    {
        if (x < 0 || x >= gridWidth || z < 0 || z >= gridHeight) return;
        RemoveUnit(x, z);
        if (type == UnitType.None) return;

        var placement = new UnitPlacement(type, x, z, job);
        unitPlacements.Add(placement);
        SpawnUnit(placement);
    }

    public void RemoveUnit(int x, int z)
    {
        for (int i = unitPlacements.Count - 1; i >= 0; i--)
        {
            if (unitPlacements[i].gridX == x && unitPlacements[i].gridZ == z)
            {
                if (unitPlacements[i].instance != null) DestroyImmediate(unitPlacements[i].instance);
                unitPlacements.RemoveAt(i);
            }
        }
    }

    public void ClearUnits()
    {
        foreach (var u in unitPlacements)
            if (u.instance != null) DestroyImmediate(u.instance);
        unitPlacements.Clear();
    }

    private GameObject GetPrefabForType(UnitType type)
    {
        if (type == UnitType.Character) return characterPrefab;
        if (type == UnitType.Enemy) return enemyPrefab;
        return null;
    }

    public void SpawnUnit(UnitPlacement placement)
    {
        var prefab = GetPrefabForType(placement.unitType);
        if (prefab == null) return;
        if (placement.instance != null) DestroyImmediate(placement.instance);

        var go = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
        go.name = (placement.jobData != null ? placement.jobData.jobName : placement.unitType.ToString()) + "_" + placement.gridX + "_" + placement.gridZ;
        go.transform.position = GetUnitWorldPos(placement.gridX, placement.gridZ);
        go.transform.SetParent(transform);
        placement.instance = go;

        // Add UnitStats
        var stats = go.GetComponent<UnitStats>();
        if (stats == null) stats = go.AddComponent<UnitStats>();
        stats.unitType = placement.unitType;
        stats.gridX = placement.gridX;
        stats.gridZ = placement.gridZ;
        stats.jobData = placement.jobData;
    }

    public void SpawnAllUnits()
    {
        foreach (var u in unitPlacements) SpawnUnit(u);
    }

    public void RespawnAllUnits()
    {
        foreach (var u in unitPlacements)
        {
            if (u.instance != null) DestroyImmediate(u.instance);
            u.instance = null;
        }
        SpawnAllUnits();
    }

    // ========== Pathfinding (BFS) ==========
    public List<Vector2Int> CalculateMovementRange(int startX, int startZ, int moveRange, int maxHeightDiff)
    {
        var result = new List<Vector2Int>();
        var best = new Dictionary<long, int>();
        var queue = new Queue<Vector2Int>();

        var start = new Vector2Int(startX, startZ);
        queue.Enqueue(start);
        best[Key(startX, startZ)] = moveRange;

        Vector2Int[] dirs = new Vector2Int[] {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };

        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            long ck = Key(cur.x, cur.y);
            int remaining = best[ck];

            if (!result.Contains(cur)) result.Add(cur);
            if (remaining <= 0) continue;

            foreach (var d in dirs)
            {
                int nx = cur.x + d.x;
                int nz = cur.y + d.y;
                if (nx < 0 || nx >= gridWidth || nz < 0 || nz >= gridHeight) continue;

                int hDiff = Mathf.Abs(GetHeight(cur.x, cur.y) - GetHeight(nx, nz));
                if (hDiff > maxHeightDiff) continue;

                int cost = 1;
                int newRemaining = remaining - cost;
                long nk = Key(nx, nz);

                if (best.ContainsKey(nk) && best[nk] >= newRemaining) continue;

                best[nk] = newRemaining;
                queue.Enqueue(new Vector2Int(nx, nz));
            }
        }
        return result;
    }

    private long Key(int x, int z) { return (long)x * 10000 + z; }
}
