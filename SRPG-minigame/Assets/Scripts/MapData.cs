using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMapData", menuName = "SRPG/Map Data")]
public class MapData : ScriptableObject
{
    public int width;
    public int height;
    
    // 타일별 높이 데이터 (1차원 배열로 저장)
    public int[] heights;
    
    // 타일별 이동 가능 여부
    public bool[] walkables;

    // 데이터를 초기화합니다.
    public void Initialize(int w, int h)
    {
        width = w;
        height = h;
        heights = new int[w * h];
        walkables = new bool[w * h];
        
        for (int i = 0; i < walkables.Length; i++)
            walkables[i] = true;
    }

    // 특정 좌표의 높이를 설정합니다.
    public void SetHeight(int x, int z, int h)
    {
        int index = z * width + x;
        if (index >= 0 && index < heights.Length)
        {
            heights[index] = h;
        }
    }

    // 특정 좌표의 높이를 가져옵니다.
    public int GetHeight(int x, int z)
    {
        int index = z * width + x;
        if (index >= 0 && index < heights.Length)
        {
            return heights[index];
        }
        return 0;
    }

    [System.Serializable]
    public class EnemyPlacement
    {
        public int x, z;
        public string enemyID; // 프리팹 이름이나 ID
    }

    [Header("Spawn Data")]
    public List<EnemyPlacement> enemies = new List<EnemyPlacement>();
    public List<Vector2Int> playerSpawnPoints = new List<Vector2Int>();
}
