using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItem
{
    public Sprite icon;             // 아이템 아이콘
    public int enhancementLevel;    // 강화 수치
    public Rarity rarity;           // 등급 (Common ~ Legendary)
    public ItemCategory category;   // 분류 (무기, 재료 등)
}

public enum Rarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

public enum ItemCategory
{
    Weapon,         // 무기 재료
    Enhancement,    // 강화 재료
    General,        // 일상 재료
    Quest           // 퀘스트
}