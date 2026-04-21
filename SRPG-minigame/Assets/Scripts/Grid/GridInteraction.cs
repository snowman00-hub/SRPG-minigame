using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(GridMap))]
public class GridInteraction : MonoBehaviour
{
    [Header("Highlight Settings")]
    public Color moveRangeColor = new Color(0.2f, 0.5f, 1f, 0.45f);
    public float highlightYOffset = 0.51f;

    private GridMap gridMap;
    private List<GameObject> highlights = new List<GameObject>();
    private Material highlightMat;
    private UnitStats selectedUnit;

    void Awake()
    {
        gridMap = GetComponent<GridMap>();
        CreateHighlightMaterial();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
        if (Input.GetMouseButtonDown(1))
        {
            ClearHighlights();
            selectedUnit = null;
        }
    }

    void HandleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, 100f)) return;

        // Check if clicked a unit
        var stats = hit.collider.GetComponent<UnitStats>();
        if (stats == null) stats = hit.collider.GetComponentInParent<UnitStats>();

        if (stats != null)
        {
            SelectUnit(stats);
            return;
        }

        // Clicked on tile or empty - move or deselect
        if (selectedUnit != null)
        {
            int gx, gz;
            if (gridMap.WorldToGrid(hit.point, out gx, out gz))
            {
                // Check if clicked tile is in movement range
                var range = gridMap.CalculateMovementRange(
                    selectedUnit.gridX, selectedUnit.gridZ,
                    selectedUnit.moveRange, selectedUnit.maxHeightDiff);

                bool inRange = false;
                foreach (var r in range)
                    if (r.x == gx && r.y == gz) { inRange = true; break; }

                if (inRange && gridMap.GetUnitTypeAt(gx, gz) == UnitType.None)
                {
                    MoveUnit(selectedUnit, gx, gz);
                }
            }
            ClearHighlights();
            selectedUnit = null;
        }
    }

    void SelectUnit(UnitStats stats)
    {
        ClearHighlights();
        selectedUnit = stats;

        var range = gridMap.CalculateMovementRange(
            stats.gridX, stats.gridZ,
            stats.moveRange, stats.maxHeightDiff);

        ShowHighlights(range);
    }

    void MoveUnit(UnitStats unit, int newX, int newZ)
    {
        // Update placement data
        var placement = gridMap.GetUnitPlacementAt(unit.gridX, unit.gridZ);
        if (placement != null)
        {
            placement.gridX = newX;
            placement.gridZ = newZ;
        }

        // Update stats
        unit.gridX = newX;
        unit.gridZ = newZ;

        // Move the actual GameObject
        unit.transform.position = gridMap.GetUnitWorldPos(newX, newZ);
        unit.gameObject.name = unit.unitType + "_" + newX + "_" + newZ;
    }

    void ShowHighlights(List<Vector2Int> tiles)
    {
        foreach (var t in tiles)
        {
            int h = gridMap.GetHeight(t.x, t.y);
            Vector3 pos = gridMap.GridToWorld(t.x, t.y, h);
            pos.y += highlightYOffset;

            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "Highlight";
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(90, 0, 0);
            go.transform.localScale = new Vector3(gridMap.tileSpacingX * 0.95f, gridMap.tileSpacingZ * 0.95f, 1f);
            go.transform.SetParent(transform);

            var rend = go.GetComponent<Renderer>();
            rend.material = highlightMat;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend.receiveShadows = false;

            // Remove collider so highlights don't block raycasts
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);

            highlights.Add(go);
        }
    }

    public void ClearHighlights()
    {
        foreach (var h in highlights)
            if (h != null) Destroy(h);
        highlights.Clear();
    }

    void CreateHighlightMaterial()
    {
        highlightMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        highlightMat.SetFloat("_Surface", 1); // Transparent
        highlightMat.SetFloat("_Blend", 0);   // Alpha
        highlightMat.SetFloat("_AlphaClip", 0);
        highlightMat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        highlightMat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        highlightMat.SetFloat("_ZWrite", 0);
        highlightMat.SetInt("_RenderQueue", 3000);
        highlightMat.renderQueue = 3000;
        highlightMat.color = moveRangeColor;
        highlightMat.SetOverrideTag("RenderType", "Transparent");
        highlightMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
    }
}
