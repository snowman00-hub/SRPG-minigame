using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;

    private Tile[,] tiles;

    [ContextMenu("Generate Map")]
    public void GenerateMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile tile = Instantiate(tilePrefab, new Vector3(x, 0, y), Quaternion.identity, transform);
                tile.Init(new Vector2Int(x, y), 0);
            }
        }
    }
}