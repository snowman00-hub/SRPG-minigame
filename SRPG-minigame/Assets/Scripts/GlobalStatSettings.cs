using UnityEngine;

[CreateAssetMenu(fileName = "GlobalStatSettings", menuName = "SRPG/Global Stat Settings")]
public class GlobalStatSettings : ScriptableObject
{
    [Header("Standard Stats (Level 1)")]
    public int stdMaxHP = 100;
    public int stdAttack = 10;
    public int stdDefense = 5;
    public int stdSpeed = 10;
    public int stdMagicAttack = 10;
    public int stdMagicDefense = 5;
    
    [Header("Standard Growth (Per Level)")]
    public int stdHPGrowth = 10;
    public int stdAttackGrowth = 2;
    public int stdDefenseGrowth = 1;
    public int stdSpeedGrowth = 1;
    public int stdMagicAttackGrowth = 2;
    public int stdMagicDefenseGrowth = 1;

    [Header("Shared Defaults")]
    public int defaultMaxEnergy = 3;
    public int defaultJumpHeight = 1;

    [Header("Unit Management")]
    public System.Collections.Generic.List<UnitData> allUnitTemplates;

    private System.Collections.Generic.Dictionary<string, UnitData> unitDataCache;

    public UnitData GetUnitData(string unitID)
    {
        if (unitDataCache == null)
        {
            unitDataCache = new System.Collections.Generic.Dictionary<string, UnitData>();
            if (allUnitTemplates != null)
            {
                foreach (var template in allUnitTemplates)
                {
                    if (template != null) unitDataCache[template.UnitID] = template;
                }
            }
        }
        
        if (unitDataCache.TryGetValue(unitID, out UnitData data)) return data;
        return null;
    }

    // 싱글톤처럼 쉽게 접근하기 위한 유틸리티 (Resources 폴더 사용 권장)
    private static GlobalStatSettings instance;
    public static GlobalStatSettings Instance
    {
        get
        {
            if (instance == null) instance = Resources.Load<GlobalStatSettings>("GlobalStatSettings");
            return instance;
        }
    }
}
