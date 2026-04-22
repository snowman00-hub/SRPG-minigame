using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// 에디터 작업 모드
public enum EditMode { Height, Enemy, PlayerSpawn }

public class MapEditorWindow : EditorWindow
{
    private EditMode currentMode = EditMode.Height;
    private int paintHeight = 1;
    private int selectedEnemyIndex = 0; // 선택된 적군 인덱스
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

        if (targetMap.Tiles == null)
        {
            targetMap.RebuildTilesArray();
        }

        // 설정 영역
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
            if (targetMap.unitPrefabs != null && targetMap.unitPrefabs.Count > 0)
            {
                string[] options = new string[targetMap.unitPrefabs.Count];
                for (int i = 0; i < targetMap.unitPrefabs.Count; i++)
                {
                    options[i] = targetMap.unitPrefabs[i] != null ? targetMap.unitPrefabs[i].name : "Empty Slot";
                }
                
                selectedEnemyIndex = EditorGUILayout.Popup("Select Enemy", selectedEnemyIndex, options);
                
                if (selectedEnemyIndex >= 0 && selectedEnemyIndex < options.Length)
                {
                    enemyID = options[selectedEnemyIndex];
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Current Prefab:", GUILayout.Width(100));
                    EditorGUILayout.ObjectField(targetMap.unitPrefabs[selectedEnemyIndex], typeof(GameObject), false);
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Map 컴포넌트의 'Unit Prefabs' 리스트에 프리팹을 먼저 등록해주세요.", MessageType.Warning);
            }
        }

        EditorGUILayout.HelpBox("좌클릭: 배치/변경 | 우클릭: 삭제/초기화", MessageType.Info);
        
        EditorGUILayout.Space(5);
        // Generate Map 버튼을 더 잘 보이게 추가
        GUI.backgroundColor = new Color(0.7f, 1f, 0.7f); // 연한 녹색
        if (GUILayout.Button("Generate Map (Rebuild Scene)", GUILayout.Height(30)))
        {
            targetMap.GenerateMap();
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // 그리드 그리기 영역
        DrawGrid();
        
        EditorGUILayout.Space(10);
        // 하단에도 유지 (스크롤이 길어질 경우 대비)
        if (GUILayout.Button("Save Data & Refresh"))
        {
            targetMap.SaveToData();
            targetMap.GenerateMap();
        }
    }

    private void DrawGrid()
    {
        int w = targetMap.horizontalSize;
        int h = targetMap.verticalSize;

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(30);
        for (int x = 0; x < w; x++)
        {
            EditorGUILayout.LabelField(x.ToString(), EditorStyles.centeredGreyMiniLabel, GUILayout.Width(35));
        }
        EditorGUILayout.EndHorizontal();

        for (int z = h - 1; z >= 0; z--)
        {
            EditorGUILayout.BeginHorizontal();
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

                string label = tile != null ? currentH.ToString() : "N/A";
                if (targetMap.mapData != null)
                {
                    if (targetMap.mapData.playerSpawnPoints.Contains(new Vector2Int(x, z)))
                        label = "P\n" + label;
                    if (targetMap.mapData.enemies.Exists(e => e.x == x && e.z == z))
                        label = "E\n" + label;
                }

                GUI.backgroundColor = cellColor;

                Rect rect = GUILayoutUtility.GetRect(new GUIContent(label), GUI.skin.button, GUILayout.Width(45), GUILayout.Height(45));
                
                Event e = Event.current;
                if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && rect.Contains(e.mousePosition))
                {
                    if (e.button == 0) // 좌클릭
                    {
                        HandleCellClick(x, z, tile, 0);
                        e.Use();
                    }
                    else if (e.button == 1) // 우클릭
                    {
                        HandleCellClick(x, z, tile, 1);
                        e.Use();
                    }
                }

                GUI.Box(rect, label, GUI.skin.button);
            }
            
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private void HandleCellClick(int x, int z, Tile tile, int mouseButton)
    {
        if (targetMap.mapData == null) return;

        Undo.RecordObject(targetMap.mapData, "Map Editor Change");

        if (mouseButton == 1)
        {
            targetMap.mapData.enemies.RemoveAll(e => e.x == x && e.z == z);
            targetMap.mapData.playerSpawnPoints.Remove(new Vector2Int(x, z));
            if (tile != null)
            {
                Undo.RecordObject(tile, "Reset Height");
                tile.SetHeightValue(0);
                targetMap.mapData.SetHeight(x, z, 0);
            }
        }
        else
        {
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
                    targetMap.mapData.enemies.Add(new EnemyPlacement { x = x, z = z, enemyID = enemyID });
                    break;

                case EditMode.PlayerSpawn:
                    Vector2Int pos = new Vector2Int(x, z);
                    if (!targetMap.mapData.playerSpawnPoints.Contains(pos))
                        targetMap.mapData.playerSpawnPoints.Add(pos);
                    break;
            }
        }

        EditorUtility.SetDirty(targetMap.mapData);
        if (tile != null) 
        {
            EditorUtility.SetDirty(tile);
            SceneView.RepaintAll();
        }
    }
}
