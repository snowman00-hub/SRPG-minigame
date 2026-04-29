using UnityEngine;

public class Tile : MonoBehaviour
{
    [Header("Grid Info")]
    public int x;
    public int z;
    public int height;

    [Header("Status")]
    public bool isWalkable = true;
    public bool isSpawnPoint = false;
    public Unit unitOnTile; // 현재 타일 위에 있는 유닛

    public void Setup(int x, int z, int height, bool isWalkable = true)
    {
        this.x = x;
        this.z = z;
        this.height = height;
        this.isWalkable = isWalkable;
        UpdateVisualPosition();
    }

    public void SetHeight(int delta)
    {
        height += delta;
        UpdateVisualPosition();
    }

    public void SetHeightValue(int value)
    {
        height = value;
        UpdateVisualPosition();
    }

    private void UpdateVisualPosition()
    {
        // 높이 1단계당 0.5f 씩 위로 이동하도록 설정 (원하는 값으로 수정 가능)
        transform.localPosition = new Vector3(x, height * 0.5f, z);
    }
}
