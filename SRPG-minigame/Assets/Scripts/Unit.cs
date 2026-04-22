using UnityEngine;

public enum Team { Player, Enemy }

public class Unit : MonoBehaviour
{
    [Header("Unit Info")]
    public string unitName;
    public Team team;
    
    [Header("Stats")]
    public int moveRange = 3;
    
    [Header("Grid Position")]
    public int x;
    public int z;

    // 유닛을 그리드 좌표에 즉시 배치합니다.
    public void SetPosition(int targetX, int targetZ, Map map)
    {
        x = targetX;
        z = targetZ;

        Tile tile = map.GetTile(x, z);
        if (tile != null)
        {
            // 타일의 높이(y)를 기준으로 유닛을 살짝 위에 띄웁니다.
            float yPos = tile.transform.position.y + 0.5f; 
            transform.position = new Vector3(x * map.spacing, yPos, z * map.spacing);
        }
    }
    
    // 유닛의 시각적 위치를 현재 그리드 좌표와 맵 상태에 맞춰 업데이트합니다.
    public void UpdateVisualPosition(Map map)
    {
        SetPosition(x, z, map);
    }
}
