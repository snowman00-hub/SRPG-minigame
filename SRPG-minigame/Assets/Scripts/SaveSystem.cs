using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class UnitSaveData
{
    public string unitName;
    public string unitDataID; // ScriptableObject를 찾기 위한 고유 ID
    public int level;
    public int currentEXP;
}

[System.Serializable]
public class GameSaveData
{
    public List<UnitSaveData> unitList = new List<UnitSaveData>();
}

public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "savegame.json");

    public static void Save(List<Unit> units)
    {
        GameSaveData saveData = new GameSaveData();

        foreach (Unit unit in units)
        {
            UnitSaveData usd = new UnitSaveData
            {
                unitName = unit.unitName,
                unitDataID = unit.unitData != null ? unit.unitData.UnitID : "",
                level = unit.stats.level,
                currentEXP = unit.stats.currentEXP
            };
            saveData.unitList.Add(usd);
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"Game Saved to: {SavePath}");
    }

    public static GameSaveData Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("Save file not found. Generating default starting data.");
            return GetDefaultSaveData();
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
            
            // 파일은 있는데 데이터가 비어있는 특수한 경우도 체크
            if (data == null || data.unitList == null || data.unitList.Count == 0)
            {
                return GetDefaultSaveData();
            }

            Debug.Log("Game Loaded");
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load save: {e.Message}");
            return GetDefaultSaveData();
        }
    }

    private static GameSaveData GetDefaultSaveData()
    {
        GameSaveData defaultData = new GameSaveData();
        
        var global = GlobalStatSettings.Instance;
        if (global != null && global.allUnitTemplates != null)
        {
            foreach (var template in global.allUnitTemplates)
            {
                if (template == null) continue;
                defaultData.unitList.Add(new UnitSaveData
                {
                    unitName = template.unitName,
                    unitDataID = template.UnitID,
                    level = 1,
                    currentEXP = 0
                });
            }
        }

        return defaultData;
    }

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }
    }
}
