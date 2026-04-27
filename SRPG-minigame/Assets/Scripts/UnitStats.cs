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
    public int moveRange;
    public int jumpHeight;
    public int atkRange;
    
    public int currentEnergy;
    public int maxEnergy;

    public float critRate;
    public float accuracy;
    public float evasion;

    private UnitData data;

    public void Initialize(UnitData unitData, int startLevel = 1)
    {
        data = unitData;
        
        // 레벨이 1인 경우에만 초기화 (새 유닛 생성 시)
        // 만약 기존 레벨을 유지하고 싶다면 레벨 설정 로직을 분리해야 함
        level = startLevel;
        
        // 경험치 통 계산 (레벨에 비례)
        maxEXP = 100;
        for (int i = 1; i < level; i++)
        {
            maxEXP = (int)(maxEXP * 1.2f);
        }

        // 기본 스탯 + (성장치 * (레벨 - 1))
        maxHP = data.baseMaxHP + (data.hpGrowth * (level - 1));
        currentHP = maxHP;
        
        attack = data.baseAttack + (data.attackGrowth * (level - 1));
        defense = data.baseDefense + (data.defenseGrowth * (level - 1));
        
        moveRange = data.baseMoveRange + (data.moveRangeGrowth * (level - 1));
        jumpHeight = data.baseJumpHeight; // 점프 높이는 성장치 없음 (기획 참고)
        atkRange = data.baseAtkRange + (data.atkRangeGrowth * (level - 1));
        
        maxEnergy = data.baseMaxEnergy;
        currentEnergy = maxEnergy;

        critRate = data.baseCritRate + (data.critGrowth * (level - 1));
        accuracy = data.baseAccuracy + (data.accuracyGrowth * (level - 1));
        evasion = data.baseEvasion + (data.evasionGrowth * (level - 1));
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
        maxEXP = (int)(maxEXP * 1.2f); // 레벨업 필요 경험치 증가 예시

        // 성장치 반영
        maxHP += data.hpGrowth;
        currentHP += data.hpGrowth; // 레벨업 시 현재 체력도 증가 (기획에 따라 변경 가능)
        
        attack += data.attackGrowth;
        defense += data.defenseGrowth;
        
        critRate += data.critGrowth;
        accuracy += data.accuracyGrowth;
        evasion += data.evasionGrowth;

        moveRange += data.moveRangeGrowth;
        atkRange += data.atkRangeGrowth;

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
