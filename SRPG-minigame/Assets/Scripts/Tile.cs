using UnityEngine;

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
    //private void OnDrawGizmos()
    //{
    //    UnityEditor.Handles.Label(
    //        transform.position + Vector3.up * 1.0f,
    //        $"{gridPosition}\nH:{height}"
    //    );
    //}

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