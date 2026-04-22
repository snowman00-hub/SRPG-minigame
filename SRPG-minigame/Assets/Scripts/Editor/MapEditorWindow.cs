using UnityEngine;
using UnityEditor;

public class MapEditorWindow : EditorWindow
{
    private enum EditMode { Height, Enemy, PlayerSpawn }
    private EditMode currentMode = EditMode.Height;

    private int paintHeight = 1;
    private string enemyID = "Enemy_Slime";
    private Vector2 scrollPos;
    private Map targetMap;

    [MenuItem("Tools/Map Editor")]
    public static void ShowWindow()
    {
        GetWindow<MapEditorWindow>("Map Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Map Grid Editor", EditorStyles.boldLabel);

        if (targetMap == null)
        {
            targetMap = FindAnyObjectByType<Map>();
        }

        if (targetMap == null)
        {
            EditorGUILayout.HelpBox("씬에 Map 오브젝트가 없습니다.", MessageType.Warning);
            if (GUILayout.Button("Find Map Again")) targetMap = FindAnyObjectByType<Map>();
            return;
        }

        // 2. 설정 영역
        EditorGUILayout.BeginVertical("box");
        targetMap = (Map)EditorGUILayout.ObjectField("Target Map", targetMap, typeof(Map), true);
        
        EditorGUILayout.Space(5);
        currentMode = (EditMode)EditorGUILayout.EnumPopup("Edit Mode", currentMode);

        if (currentMode == EditMode.Height)
        {
            paintHeight = EditorGUILayout.IntField("Paint Height", paintHeight);
        }
        else if (currentMode == EditMode.Enemy)
        {
            enemyID = EditorGUILayout.TextField("Enemy ID/Prefab Name", enemyID);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // 3. 그리드 그리기 영역
        DrawGrid();
        
        EditorGUILayout.Space(10);
        if (GUILayout.Button("Refresh Map Data"))
        {
            targetMap.GenerateMap(); // 필요시 맵 재생성
        }
    }

    private void DrawGrid()
    {
        int w = targetMap.horizontalSize;
        int h = targetMap.verticalSize;

        // 스크롤 뷰 시작
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        
        // 상단 X 좌표 표시
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(30);
        for (int x = 0; x < w; x++)
        {
            EditorGUILayout.LabelField(x.ToString(), EditorStyles.centeredGreyMiniLabel, GUILayout.Width(35));
        }
        EditorGUILayout.EndHorizontal();

        // 그리드 본체 (Z축은 위에서 아래로)
        for (int z = h - 1; z >= 0; z--)
        {
            EditorGUILayout.BeginHorizontal();
            
            // 좌측 Z 좌표 표시
            EditorGUILayout.LabelField("Z" + z, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(25));

            for (int x = 0; x < w; x++)
            {
                Tile tile = targetMap.GetTile(x, z);
                
                Color cellColor = Color.white;
                int currentH = 0;

                if (tile != null)
                {
                    currentH = tile.height;
                    float t = Mathf.Clamp01(currentH / 10f);
                    cellColor = Color.Lerp(new Color(0.8f, 0.8f, 0.8f), new Color(0.2f, 0.6f, 1f), t);
                    if (currentH < 0) cellColor = new Color(1f, 0.5f, 0.5f);
                }

                // 정보 표시용 라벨 (P: 아군 스폰, E: 적군 고정배치)
                string label = tile != null ? currentH.ToString() : "N/A";
                if (targetMap.mapData != null)
                {
                    if (targetMap.mapData.playerSpawnPoints.Contains(new Vector2Int(x, z)))
                        label = "P\n" + label;
                    if (targetMap.mapData.enemies.Exists(e => e.x == x && e.z == z))
                        label = "E\n" + label;
                }

                GUI.backgroundColor = cellColor;

                if (GUILayout.Button(label, GUILayout.Width(45), GUILayout.Height(45)))
                {
                    HandleCellClick(x, z, tile);
                }
            }
            
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private void HandleCellClick(int x, int z, Tile tile)
    {
        if (targetMap.mapData == null) return;

        Undo.RecordObject(targetMap.mapData, "Map Editor Change");

        switch (currentMode)
        {
            case EditMode.Height:
                if (tile != null)
                {
                    Undo.RecordObject(tile, "Update Height");
                    tile.SetHeightValue(paintHeight);
                    targetMap.mapData.SetHeight(x, z, paintHeight);
                }
                break;

            case EditMode.Enemy:
                targetMap.mapData.enemies.RemoveAll(e => e.x == x && e.z == z);
                targetMap.mapData.enemies.Add(new MapData.EnemyPlacement { x = x, z = z, enemyID = enemyID });
                break;

            case EditMode.PlayerSpawn:
                Vector2Int pos = new Vector2Int(x, z);
                if (targetMap.mapData.playerSpawnPoints.Contains(pos))
                    targetMap.mapData.playerSpawnPoints.Remove(pos);
                else
                    targetMap.mapData.playerSpawnPoints.Add(pos);
                break;
        }

        EditorUtility.SetDirty(targetMap.mapData);
        if (tile != null) EditorUtility.SetDirty(tile);
    }
}
