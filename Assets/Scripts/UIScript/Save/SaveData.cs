using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int currentDay;
    public int currentLevel;
    public int currentXP;
    public int currentStress;

    public List<InventoryItemData> inventoryItems;

    public List<StoreItemRecord> specialOffers;
    public List<StoreItemRecord> materialItems;
    public List<StoreItemRecord> enhanceItems;
    public List<StoreItemRecord> todayItems;
}

// 인벤 항목 직렬화용 DTO (기존)
[System.Serializable]
public class InventoryItemData
{
    public string iconName;
    public string category;
    public string effectType;
    public int quantity;
    public int acquireIndex;
}

// 상점 슬롯 직렬화용 DTO
[System.Serializable]
public class StoreItemRecord
{
    public string iconName;       // Resources.Load 로 불러올 스프라이트 이름
    public string baseCategory;   // StoreItemCategory.ToString()
    public string itemCategory;   // StoreItemCategory.ToString()
    public int price;          // 현재 가격
    public int amount;         // 재고 수량
    public string effectType;     // EffectType.ToString()
}