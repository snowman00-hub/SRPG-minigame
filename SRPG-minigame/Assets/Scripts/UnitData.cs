using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "SRPG/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("Basic Information")]
    public string unitName;
    public string description;
    public GameObject unitPrefab;

    [Header("Stat Multipliers (1.0 = 100%)")]
    public float hpRatio = 1.0f;
    public float atkRatio = 1.0f;
    public float defRatio = 1.0f;
    public float spdRatio = 1.0f;
    public float matkRatio = 1.0f;
    public float mdefRatio = 1.0f;
    
    [Header("Growth Multipliers (1.0 = 100%)")]
    public float hpGrowthRatio = 1.0f;
    public float atkGrowthRatio = 1.0f;
    public float defGrowthRatio = 1.0f;
    public float spdGrowthRatio = 1.0f;
    public float matkGrowthRatio = 1.0f;
    public float mdefGrowthRatio = 1.0f;

    [Header("Fixed Stats")]
    public int moveRange = 3;
    public int jumpHeight = 1;
    public int atkRange = 1;
    public int maxEnergy = 3;
    
    [Header("Utility Stats")]
    [Range(0, 100)] public float baseCritRate = 5f;
    [Range(0, 100)] public float baseAccuracy = 90f;
    [Range(0, 100)] public float baseEvasion = 5f;
}
