using UnityEngine;

// 나중에 Tile엔 MonoBehaviour 제거하기
public class Tile : MonoBehaviour
{
    private Vector2Int gridPosition;
    [SerializeField] private int height;

    public Vector2Int GridPosition => gridPosition;
    public int Height => height;

    public void Init(Vector2Int gridPosition, int height)
    {
        this.gridPosition = gridPosition;
        this.height = height;
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 1.0f,
            $"{gridPosition}\nH:{height}"
        );
    }

    // 인스펙터 값 변경 시, 씬 저장 시, 컴파일 시 호출
    private void OnValidate()
    {
        transform.position = new Vector3(
            transform.position.x,
            height * 0.5f,
            transform.position.z
        );
    }
#endif
}