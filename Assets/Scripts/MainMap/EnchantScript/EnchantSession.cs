using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnchantSession
{
    public static bool IsActive { get; private set; }
    public static RecipeData Recipe { get; private set; }
    public static int QualityScore { get; private set; }

    // ▼ 강화 레벨 및 가격 배율
    public static int enchantLevel = 0;   // 0 = 무강, 1 = 1강 ...
    public static float perLevelRate = 0.10f; // 레벨당 +10% (원하면 조정)
    public static InventoryItem matA;         // 네 인벤토리 아이템 타입으로 교체
    public static InventoryItem matB;


    public static void Start(RecipeData r, int q)
    {
        Recipe = r;
        QualityScore = q;
        enchantLevel = 0;
        perLevelRate = 0.10f;
        matA = null; matB = null;
        IsActive = true;
    }

    public static void Clear() => IsActive = false;

    public static float GetPayMultiplierFor(int level)
    {
        if (level <= 0) return 1f;
        return 1f + level * perLevelRate;
    }

    public static float GetPayMultiplier() => GetPayMultiplierFor(enchantLevel); 
}