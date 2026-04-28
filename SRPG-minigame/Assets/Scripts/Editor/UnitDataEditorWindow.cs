using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class UnitDataEditorWindow : EditorWindow
{
    private List<UnitData> unitDataList = new List<UnitData>();
    private Vector2 scrollPos;
    private Vector2 tableScrollPos;
    private UnitData selectedData;
    private UnityEditor.Editor cachedEditor;
    private string searchText = "";

    private enum DisplayMode { Individual, Comparison }
    private DisplayMode currentMode = DisplayMode.Individual;

    private GlobalStatSettings globalSettings;

    [MenuItem("Tools/Unit Data Editor")]
    public static void ShowWindow()
    {
        GetWindow<UnitDataEditorWindow>("Unit Data Editor");
    }

    private void OnEnable()
    {
        RefreshList();
        globalSettings = GlobalStatSettings.Instance;
    }

    private void RefreshList()
    {
        unitDataList = AssetDatabase.FindAssets("t:UnitData")
            .Select(guid => AssetDatabase.LoadAssetAtPath<UnitData>(AssetDatabase.GUIDToAssetPath(guid)))
            .OrderBy(d => d.name)
            .ToList();
    }

    private void OnGUI()
    {
        DrawToolbar();

        if (globalSettings == null)
        {
            EditorGUILayout.HelpBox("글로벌 설정을 찾을 수 없습니다! 'Resources' 폴더에 GlobalStatSettings 파일을 생성해주세요.", MessageType.Error);
            if (GUILayout.Button("설정 파일 찾기")) globalSettings = GlobalStatSettings.Instance;
            return;
        }

        if (currentMode == DisplayMode.Individual)
        {
            DrawIndividualMode();
        }
        else
        {
            DrawComparisonMode();
        }
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        currentMode = (DisplayMode)GUILayout.Toolbar((int)currentMode, new string[] { "개별 편집", "계수 비교 (표)" }, EditorStyles.toolbarButton);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("새로고침", EditorStyles.toolbarButton)) RefreshList();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawIndividualMode()
    {
        EditorGUILayout.BeginHorizontal();

        // 좌측 리스트
        EditorGUILayout.BeginVertical(GUILayout.Width(250), GUILayout.ExpandHeight(true));
        searchText = EditorGUILayout.TextField(searchText, EditorStyles.toolbarSearchField);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, "box");
        foreach (var data in unitDataList)
        {
            if (!string.IsNullOrEmpty(searchText) && !data.name.ToLower().Contains(searchText.ToLower())) continue;
            GUI.backgroundColor = selectedData == data ? Color.cyan : Color.white;
            if (GUILayout.Button(data.name, "Button", GUILayout.Height(25))) { selectedData = data; cachedEditor = null; }
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndScrollView();
        if (GUILayout.Button("새 유닛 생성", GUILayout.Height(30))) CreateNewUnit();
        EditorGUILayout.EndVertical();

        // 우측 상세
        EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        if (selectedData != null)
        {
            if (cachedEditor == null) cachedEditor = UnityEditor.Editor.CreateEditor(selectedData);
            
            // Preview Stats
            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.LabelField("최종 스탯 (1레벨 프리뷰)", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"체력: {Mathf.RoundToInt(globalSettings.stdMaxHP * selectedData.hpRatio)} | " +
                                     $"공격: {Mathf.RoundToInt(globalSettings.stdAttack * selectedData.atkRatio)} | " +
                                     $"방어: {Mathf.RoundToInt(globalSettings.stdDefense * selectedData.defRatio)} | " +
                                     $"속도: {Mathf.RoundToInt(globalSettings.stdSpeed * selectedData.spdRatio)} | " +
                                     $"마공: {Mathf.RoundToInt(globalSettings.stdMagicAttack * selectedData.matkRatio)} | " +
                                     $"마방: {Mathf.RoundToInt(globalSettings.stdMagicDefense * selectedData.mdefRatio)}");
            EditorGUILayout.EndVertical();

            cachedEditor.OnInspectorGUI();
        }
        else
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("편집할 유닛을 선택하세요.", new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter });
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawComparisonMode()
    {
        tableScrollPos = EditorGUILayout.BeginScrollView(tableScrollPos);

        EditorGUILayout.BeginHorizontal();
        DrawTableHeader("유닛 이름", 120);
        DrawTableHeader("아이콘", 50);
        DrawTableHeader("체력", 45); DrawTableHeader("체력성장", 55);
        DrawTableHeader("공격", 45); DrawTableHeader("공격성장", 55);
        DrawTableHeader("방어", 45); DrawTableHeader("방어성장", 55);
        DrawTableHeader("속도", 45); DrawTableHeader("속도성장", 55);
        DrawTableHeader("마공", 45); DrawTableHeader("마공성장", 55);
        DrawTableHeader("마방", 45); DrawTableHeader("마방성장", 55);
        DrawTableHeader("명중", 40);
        DrawTableHeader("회피", 40);
        DrawTableHeader("크리", 40);
        DrawTableHeader("이동", 35);
        DrawTableHeader("사거리", 45);
        EditorGUILayout.EndHorizontal();

        foreach (var data in unitDataList)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(data.name, GUILayout.Width(120));
            data.unitIcon = (Sprite)EditorGUILayout.ObjectField(data.unitIcon, typeof(Sprite), false, GUILayout.Width(50));
            
            // HP
            data.hpRatio = EditorGUILayout.FloatField(data.hpRatio, GUILayout.Width(45));
            data.hpGrowthRatio = EditorGUILayout.FloatField(data.hpGrowthRatio, GUILayout.Width(55));
            
            // ATK
            data.atkRatio = EditorGUILayout.FloatField(data.atkRatio, GUILayout.Width(45));
            data.atkGrowthRatio = EditorGUILayout.FloatField(data.atkGrowthRatio, GUILayout.Width(55));
            
            // DEF
            data.defRatio = EditorGUILayout.FloatField(data.defRatio, GUILayout.Width(45));
            data.defGrowthRatio = EditorGUILayout.FloatField(data.defGrowthRatio, GUILayout.Width(55));
            
            // SPD
            data.spdRatio = EditorGUILayout.FloatField(data.spdRatio, GUILayout.Width(45));
            data.spdGrowthRatio = EditorGUILayout.FloatField(data.spdGrowthRatio, GUILayout.Width(55));

            // Magic
            data.matkRatio = EditorGUILayout.FloatField(data.matkRatio, GUILayout.Width(45));
            data.matkGrowthRatio = EditorGUILayout.FloatField(data.matkGrowthRatio, GUILayout.Width(55));
            data.mdefRatio = EditorGUILayout.FloatField(data.mdefRatio, GUILayout.Width(45));
            data.mdefGrowthRatio = EditorGUILayout.FloatField(data.mdefGrowthRatio, GUILayout.Width(55));

            // Utility
            data.baseAccuracy = EditorGUILayout.FloatField(data.baseAccuracy, GUILayout.Width(40));
            data.baseEvasion = EditorGUILayout.FloatField(data.baseEvasion, GUILayout.Width(40));
            data.baseCritRate = EditorGUILayout.FloatField(data.baseCritRate, GUILayout.Width(40));

            // Fixed
            data.moveRange = EditorGUILayout.IntField(data.moveRange, GUILayout.Width(35));
            data.atkRange = EditorGUILayout.IntField(data.atkRange, GUILayout.Width(45));

            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(data);
                AssetDatabase.SaveAssets();
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawTableHeader(string label, float width)
    {
        EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel, GUILayout.Width(width));
    }

    private void CreateNewUnit()
    {
        string path = EditorUtility.SaveFilePanelInProject("새 유닛 데이터 저장", "NewUnitData", "asset", "이름을 입력하세요.");
        if (!string.IsNullOrEmpty(path))
        {
            UnitData newData = CreateInstance<UnitData>();
            
            if (globalSettings != null)
            {
                newData.maxEnergy = globalSettings.defaultMaxEnergy;
                newData.jumpHeight = globalSettings.defaultJumpHeight;
                
                newData.hpRatio = 1.0f;
                newData.atkRatio = 1.0f;
                newData.defRatio = 1.0f;
                newData.spdRatio = 1.0f;
                newData.matkRatio = 1.0f;
                newData.mdefRatio = 1.0f;
                newData.hpGrowthRatio = 1.0f;
                newData.atkGrowthRatio = 1.0f;
                newData.defGrowthRatio = 1.0f;
                newData.spdGrowthRatio = 1.0f;
                newData.matkGrowthRatio = 1.0f;
                newData.mdefGrowthRatio = 1.0f;
                
                newData.baseAccuracy = 90f;
                newData.baseEvasion = 5f;
                newData.baseCritRate = 5f;
                newData.moveRange = 3;
                newData.atkRange = 1;
            }

            AssetDatabase.CreateAsset(newData, path);
            AssetDatabase.SaveAssets();
            RefreshList();
            selectedData = newData;
        }
    }
}
