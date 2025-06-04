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
    public Sprite icon;
    public string itemName;

    // 새로 추가: “원래(Material/Enhance)” 정보를 저장
    public StoreItemCategory baseCategory;

    // 캔버스상에서 카테고리(오늘의 상품으로 바뀌면 TodaySpecial)
    public StoreItemCategory itemCategory;

    public int price;
    public int minPrice;
    public int maxPrice;

    public int amount;
    public EffectType effectType;
}