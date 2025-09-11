using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItem
{
    public Sprite icon;             // 아이템 아이콘 (상점에서 넘겨받은 Sprite)
    public ItemCategory category;   // 분류 (Weapon, Enhancement 등)
    public EffectType effectType;   // 능력 타입 (None / EasyMiniGame / NoBelowBQuality)
    public int quantity;            // 인벤토리에 쌓인 수량 
    public int acquireIndex;          // 획득 순서(클수록 나중에 획득했다는 의미)
}

public enum ItemCategory
{
    Weapon,         // 무기 재료
    Enhancement,    // 강화 재료
    General,        // 일상 재료
    Quest           // 퀘스트
}