using UnityEngine;

[System.Serializable]
public class UnitStats
{
    public int level = 1;
    public int currentEXP = 0;
    public int maxEXP = 100;

    public int currentHP;
    public int maxHP;
    
    public int attack;
    public int defense;
    public int speed;
    public int magicAttack;
    public int magicDefense;
    public int moveRange;
    public int jumpHeight;
    public int atkRange;
    
    public int currentEnergy;
    public int maxEnergy;

    public float critRate;
    public float accuracy;
    public float evasion;

    // 계산 전용 정적 함수 (인스턴스 생성 없이 사용 가능)
    public static int GetMaxHP(UnitData data, GlobalStatSettings global, int level)
    {
        if (data == null || global == null) return 0;
        return Mathf.RoundToInt((global.stdMaxHP * data.hpRatio) + (global.stdHPGrowth * data.hpGrowthRatio * (level - 1)));
    }

    private UnitData data;
    private GlobalStatSettings global;

    public void Initialize(UnitData unitData, GlobalStatSettings globalSettings, int startLevel = 1)
    {
        data = unitData;
        global = globalSettings;
        level = startLevel;
        
        // 경험치 통 계산
        maxEXP = 100;
        for (int i = 1; i < level; i++)
        {
            maxEXP = (int)(maxEXP * 1.2f);
        }

        RecalculateStats();
        currentHP = maxHP;
        currentEnergy = maxEnergy;
    }

    public void RecalculateStats()
    {
        if (data == null || global == null) return;

        // 최종 스탯 = (글로벌 표준 * 유닛 계수) + (글로벌 성장 * 유닛 성장 계수 * (레벨 - 1))
        maxHP = Mathf.RoundToInt((global.stdMaxHP * data.hpRatio) + (global.stdHPGrowth * data.hpGrowthRatio * (level - 1)));
        attack = Mathf.RoundToInt((global.stdAttack * data.atkRatio) + (global.stdAttackGrowth * data.atkGrowthRatio * (level - 1)));
        defense = Mathf.RoundToInt((global.stdDefense * data.defRatio) + (global.stdDefenseGrowth * data.defGrowthRatio * (level - 1)));
        speed = Mathf.RoundToInt((global.stdSpeed * data.spdRatio) + (global.stdSpeedGrowth * data.spdGrowthRatio * (level - 1)));
        magicAttack = Mathf.RoundToInt((global.stdMagicAttack * data.matkRatio) + (global.stdMagicAttackGrowth * data.matkGrowthRatio * (level - 1)));
        magicDefense = Mathf.RoundToInt((global.stdMagicDefense * data.mdefRatio) + (global.stdMagicDefenseGrowth * data.mdefGrowthRatio * (level - 1)));

        moveRange = data.moveRange;
        jumpHeight = data.jumpHeight;
        atkRange = data.atkRange;
        maxEnergy = data.maxEnergy;

        critRate = data.baseCritRate;
        accuracy = data.baseAccuracy;
        evasion = data.baseEvasion;
    }

    public void GainExperience(int amount)
    {
        currentEXP += amount;
        while (currentEXP >= maxEXP)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentEXP -= maxEXP;
        level++;
        maxEXP = (int)(maxEXP * 1.2f);
        
        int oldMaxHP = maxHP;
        RecalculateStats();
        
        // 레벨업 시 늘어난 최대 체력만큼 현재 체력도 보정
        currentHP += (maxHP - oldMaxHP);

        Debug.Log($"{data.unitName} Level Up! Now Level {level}");
    }

    public void ConsumeEnergy(int amount)
    {
        currentEnergy -= amount;
        if (currentEnergy < 0) currentEnergy = 0;
    }

    public void RestoreEnergy()
    {
        currentEnergy = maxEnergy;
    }
}
