using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private int horizontal = 10;
    [SerializeField] private int vertical = 10;

    private Tile[,] tiles;

    [ContextMenu("Generate Map")]
    public void GenerateMap()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        for (int x = 0; x < horizontal; x++)
        {
            for (int y = 0; y < vertical; y++)
            {
                Tile tile = Instantiate(tilePrefab, new Vector3(x, 0, y), Quaternion.identity, transform);
                tile.Init(new Vector2Int(x, y), 0);
            }
        }
    }
}