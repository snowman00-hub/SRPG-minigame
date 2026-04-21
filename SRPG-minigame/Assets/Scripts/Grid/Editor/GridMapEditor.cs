using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(GridMap))]
public class GridMapEditor : Editor
{
    private bool showHeightGrid = true;
    private bool showUnitGrid = true;
    private int brushHeight = 1;
    private UnitType brushUnit = UnitType.Character;
    private JobData brushJob; // Selected Job for the brush

    private Vector2 heightScrollPos;
    private Vector2 unitScrollPos;

    // Range preview
    private int previewX = -1;
    private int previewZ = -1;
    private List<Vector2Int> previewRange = null;

    private static readonly Color CharColor = new Color(0.3f, 0.5f, 1f, 1f);
    private static readonly Color EnemyColor = new Color(1f, 0.35f, 0.35f, 1f);
    private static readonly Color EmptyColor = new Color(0.75f, 0.75f, 0.75f, 1f);

    public override void OnInspectorGUI()
    {
        GridMap gridMap = (GridMap)target;

        // ===== Grid Settings =====
        EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);
        gridMap.tilePrefab = (GameObject)EditorGUILayout.ObjectField("Tile Prefab", gridMap.tilePrefab, typeof(GameObject), false);
        gridMap.gridWidth = EditorGUILayout.IntField("Grid Width (X)", Mathf.Max(1, gridMap.gridWidth));
        gridMap.gridHeight = EditorGUILayout.IntField("Grid Height (Z)", Mathf.Max(1, gridMap.gridHeight));

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Scale Settings", EditorStyles.boldLabel);
        gridMap.heightScale = EditorGUILayout.FloatField("Height Scale (Y per 1)", gridMap.heightScale);
        gridMap.tileSpacingX = EditorGUILayout.FloatField("Tile Spacing X", gridMap.tileSpacingX);
        gridMap.tileSpacingZ = EditorGUILayout.FloatField("Tile Spacing Z", gridMap.tileSpacingZ);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Unit Prefabs", EditorStyles.boldLabel);
        gridMap.characterPrefab = (GameObject)EditorGUILayout.ObjectField("Character Prefab", gridMap.characterPrefab, typeof(GameObject), false);
        gridMap.enemyPrefab = (GameObject)EditorGUILayout.ObjectField("Enemy Prefab", gridMap.enemyPrefab, typeof(GameObject), false);
        gridMap.unitYOffset = EditorGUILayout.FloatField("Unit Y Offset", gridMap.unitYOffset);

        EditorGUILayout.Space(10);

        // ===== Action Buttons =====
        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("Generate Map", GUILayout.Height(30)))
        {
            Undo.RecordObject(gridMap, "Generate GridMap");
            gridMap.GenerateMap();
            EditorUtility.SetDirty(gridMap);
        }
        GUI.backgroundColor = new Color(0.8f, 0.4f, 0.4f);
        if (GUILayout.Button("Clear All", GUILayout.Height(30)))
        {
            Undo.RecordObject(gridMap, "Clear GridMap");
            gridMap.ClearUnits();
            gridMap.ClearTiles();
            EditorUtility.SetDirty(gridMap);
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);
        DrawHeightMapEditor(gridMap);
        EditorGUILayout.Space(10);
        DrawUnitEditor(gridMap);
        EditorGUILayout.Space(10);
        DrawRangePreview(gridMap);

        if (GUI.changed) EditorUtility.SetDirty(gridMap);
    }

    private void DrawHeightMapEditor(GridMap gridMap)
    {
        showHeightGrid = EditorGUILayout.Foldout(showHeightGrid, "Height Map Editor", true, EditorStyles.foldoutHeader);
        if (!showHeightGrid) return;

        gridMap.InitializeHeightMap();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Brush H:", GUILayout.Width(55));
        brushHeight = EditorGUILayout.IntField(brushHeight, GUILayout.Width(35));
        if (GUILayout.Button("Fill All", GUILayout.Width(55)))
        {
            Undo.RecordObject(gridMap, "Fill All");
            for (int i = 0; i < gridMap.heightMap.Length; i++) gridMap.heightMap[i] = brushHeight;
            if (gridMap.tileInstances.Count > 0) gridMap.RefreshAllTilePositions();
            EditorUtility.SetDirty(gridMap);
        }
        if (GUILayout.Button("Reset", GUILayout.Width(50)))
        {
            Undo.RecordObject(gridMap, "Reset Heights");
            for (int i = 0; i < gridMap.heightMap.Length; i++) gridMap.heightMap[i] = 0;
            if (gridMap.tileInstances.Count > 0) gridMap.RefreshAllTilePositions();
            EditorUtility.SetDirty(gridMap);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(3);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(30);
        for (int x = 0; x < gridMap.gridWidth; x++)
            EditorGUILayout.LabelField(x.ToString(), EditorStyles.centeredGreyMiniLabel, GUILayout.Width(32));
        EditorGUILayout.EndHorizontal();

        heightScrollPos = EditorGUILayout.BeginScrollView(heightScrollPos, GUILayout.MaxHeight(300));
        for (int z = gridMap.gridHeight - 1; z >= 0; z--)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Z" + z, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(26));
            for (int x = 0; x < gridMap.gridWidth; x++)
            {
                int h = gridMap.GetHeight(x, z);
                float t = Mathf.Clamp01(h / 10f);
                GUI.backgroundColor = Color.Lerp(new Color(0.6f, 0.85f, 0.6f), new Color(0.9f, 0.5f, 0.2f), t);
                if (h < 0) GUI.backgroundColor = new Color(0.4f, 0.6f, 0.9f);

                if (GUILayout.Button(h.ToString(), GUILayout.Width(32), GUILayout.Height(22)))
                {
                    Undo.RecordObject(gridMap, "Set Height");
                    gridMap.SetHeight(x, z, Event.current.shift ? h - 1 : brushHeight);
                    if (gridMap.tileInstances.Count > 0) gridMap.UpdateTileHeight(x, z);
                    EditorUtility.SetDirty(gridMap);
                }
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawUnitEditor(GridMap gridMap)
    {
        showUnitGrid = EditorGUILayout.Foldout(showUnitGrid, "Unit Placement Editor", true, EditorStyles.foldoutHeader);
        if (!showUnitGrid) return;

        gridMap.InitializeHeightMap();

        // Unit Type Selection
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Side:", GUILayout.Width(45));
        GUI.backgroundColor = brushUnit == UnitType.Character ? CharColor : Color.white;
        if (GUILayout.Button("Character", GUILayout.Height(22))) brushUnit = UnitType.Character;
        GUI.backgroundColor = brushUnit == UnitType.Enemy ? EnemyColor : Color.white;
        if (GUILayout.Button("Enemy", GUILayout.Height(22))) brushUnit = UnitType.Enemy;
        GUI.backgroundColor = brushUnit == UnitType.None ? Color.yellow : Color.white;
        if (GUILayout.Button("Eraser", GUILayout.Height(22))) brushUnit = UnitType.None;
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        // Job Selection
        if (brushUnit != UnitType.None)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Job:", GUILayout.Width(45));
            brushJob = (JobData)EditorGUILayout.ObjectField(brushJob, typeof(JobData), false);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(5);

        // Buttons
        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(0.5f, 0.7f, 1f);
        if (GUILayout.Button("Respawn Units", GUILayout.Height(22)))
        { Undo.RecordObject(gridMap, "Respawn"); gridMap.RespawnAllUnits(); EditorUtility.SetDirty(gridMap); }
        GUI.backgroundColor = new Color(1f, 0.6f, 0.4f);
        if (GUILayout.Button("Clear Units", GUILayout.Height(22)))
        { Undo.RecordObject(gridMap, "Clear Units"); gridMap.ClearUnits(); EditorUtility.SetDirty(gridMap); }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(30);
        for (int x = 0; x < gridMap.gridWidth; x++)
            EditorGUILayout.LabelField(x.ToString(), EditorStyles.centeredGreyMiniLabel, GUILayout.Width(32));
        EditorGUILayout.EndHorizontal();

        unitScrollPos = EditorGUILayout.BeginScrollView(unitScrollPos, GUILayout.MaxHeight(300));
        for (int z = gridMap.gridHeight - 1; z >= 0; z--)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Z" + z, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(26));
            for (int x = 0; x < gridMap.gridWidth; x++)
            {
                var p = gridMap.GetUnitPlacementAt(x, z);
                UnitType ut = p != null ? p.unitType : UnitType.None;
                string label = ".";
                if (ut == UnitType.Character) { GUI.backgroundColor = CharColor; label = p.jobData != null ? p.jobData.jobName.Substring(0, 1) : "C"; }
                else if (ut == UnitType.Enemy) { GUI.backgroundColor = EnemyColor; label = p.jobData != null ? p.jobData.jobName.Substring(0, 1) : "E"; }
                else { GUI.backgroundColor = EmptyColor; }

                if (GUILayout.Button(label, GUILayout.Width(32), GUILayout.Height(22)))
                {
                    Undo.RecordObject(gridMap, "Place Unit");
                    if (brushUnit == UnitType.None || Event.current.shift)
                        gridMap.RemoveUnit(x, z);
                    else
                        gridMap.PlaceUnit(brushUnit, x, z, brushJob);
                    EditorUtility.SetDirty(gridMap);
                }
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawRangePreview(GridMap gridMap)
    {
        EditorGUILayout.LabelField("Movement Range Preview", EditorStyles.foldoutHeader);
        
        unitScrollPos = EditorGUILayout.BeginScrollView(unitScrollPos, GUILayout.MaxHeight(300));
        for (int z = gridMap.gridHeight - 1; z >= 0; z--)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Z" + z, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(26));
            for (int x = 0; x < gridMap.gridWidth; x++)
            {
                var p = gridMap.GetUnitPlacementAt(x, z);
                bool isPreview = (previewX == x && previewZ == z);
                bool isInRange = false;
                if (previewRange != null)
                {
                    foreach (var r in previewRange)
                        if (r.x == x && r.y == z) { isInRange = true; break; }
                }

                if (isPreview) GUI.backgroundColor = Color.yellow;
                else if (isInRange) GUI.backgroundColor = Color.cyan;
                else if (p != null) GUI.backgroundColor = p.unitType == UnitType.Character ? CharColor : EnemyColor;
                else GUI.backgroundColor = EmptyColor;

                string label = p != null ? (p.jobData != null ? p.jobData.jobName.Substring(0,1) : "U") : ".";

                if (GUILayout.Button(label, GUILayout.Width(32), GUILayout.Height(22)))
                {
                    if (p != null)
                    {
                        previewX = x; previewZ = z;
                        int range = 3, hDiff = 2;
                        if (p.jobData != null) { range = p.jobData.moveRange; hDiff = p.jobData.maxHeightDiff; }
                        previewRange = gridMap.CalculateMovementRange(x, z, range, hDiff);
                        SceneView.RepaintAll();
                    }
                    else { previewX = -1; previewZ = -1; previewRange = null; SceneView.RepaintAll(); }
                }
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        if (GUILayout.Button("Clear Preview")) { previewX = -1; previewZ = -1; previewRange = null; SceneView.RepaintAll(); }
    }

    private void OnSceneGUI()
    {
        if (previewRange == null || previewRange.Count == 0) return;
        GridMap gridMap = (GridMap)target;
        foreach (var tile in previewRange)
        {
            int h = gridMap.GetHeight(tile.x, tile.y);
            Vector3 center = gridMap.GridToWorld(tile.x, tile.y, h);
            center.y += 0.52f;
            Handles.color = (tile.x == previewX && tile.y == previewZ) ? new Color(1, 1, 0, 0.5f) : new Color(0, 0.5f, 1, 0.3f);
            Vector3 size = new Vector3(gridMap.tileSpacingX * 0.9f, 0, gridMap.tileSpacingZ * 0.9f);
            Handles.DrawSolidRectangleWithOutline(new Vector3[] {
                center + new Vector3(-size.x/2, 0, -size.z/2),
                center + new Vector3(-size.x/2, 0, size.z/2),
                center + new Vector3(size.x/2, 0, size.z/2),
                center + new Vector3(size.x/2, 0, -size.z/2)
            }, Handles.color, Color.white);
        }
    }
}
