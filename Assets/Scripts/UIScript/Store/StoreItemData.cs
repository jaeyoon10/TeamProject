using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StoreItemCategory
{
    Material,        // 재료 상점 (일반)
    Enhance,         // 강화 상점 (일반)
    TodaySpecial     // 오늘의 상품 전용
}

// 오늘의 상품에만 적용 가능한 효과 종류
public enum EffectType
{
    None,             // 효과 없음 (일반 상품)
    EasyMiniGame,     // 미니게임 난이도 완화
    NoBelowBQuality   // 품질이 B 이하로 떨어지지 않음
}


[System.Serializable]
public class StoreItemData
{
    public Sprite icon;                     // 아이템 아이콘
    public string itemName;                 // 아이템 이름
    public StoreItemCategory itemCategory;  // 카테고리 (Material/Enhance/TodaySpecial)

    public int price;       // 기준 가격
    public int minPrice;    // 최솟값(이하로 내려가지 않음)
    public int maxPrice;    // 최댓값(이상으로 올라가지 않음)

    public int amount;      // 재고 수량 (0이면 무제한)
    public EffectType effectType; // 효과 타입 (TodaySpecial일 때만 None 이외의 값)
}