using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "SRPG/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("Basic Information")]
    public string unitName;
    public string description;
    public GameObject unitPrefab;

    [Header("Base Stats (Level 1)")]
    public int baseMaxHP = 100;
    public int baseAttack = 10;
    public int baseDefense = 5;
    public int baseMoveRange = 3;
    public int baseJumpHeight = 1;
    public int baseAtkRange = 1;
    public int baseMaxEnergy = 3;
    
    [Range(0, 100)] public float baseCritRate = 5f;
    [Range(0, 100)] public float baseAccuracy = 90f;
    [Range(0, 100)] public float baseEvasion = 5f;

    [Header("Growth Stats (Per Level)")]
    public int hpGrowth = 10;
    public int attackGrowth = 2;
    public int defenseGrowth = 1;
    public float critGrowth = 0.5f;
    public float accuracyGrowth = 1f;
    public float evasionGrowth = 0.5f;
    
    // 이동력이나 사거리는 보통 레벨업으로 안 오르지만, 필요시 확장 가능
    public int moveRangeGrowth = 0;
    public int atkRangeGrowth = 0;
}
