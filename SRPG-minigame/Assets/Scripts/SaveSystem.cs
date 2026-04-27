using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class UnitSaveData
{
    public string unitName;
    public string unitDataName; // ScriptableObject를 찾기 위한 이름
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
                unitDataName = unit.unitData != null ? unit.unitData.name : "",
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
            Debug.LogWarning("Save file not found!");
            return null;
        }

        string json = File.ReadAllText(SavePath);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
        Debug.Log("Game Loaded");
        return data;
    }

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }
    }
}
