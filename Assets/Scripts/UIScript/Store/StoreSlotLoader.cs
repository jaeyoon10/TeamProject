using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public class StoreSlotLoader : MonoBehaviour
{
    [Header("프리팹")]
    public GameObject slotNormalPrefab;   // 일반 슬롯 (StoreItemSlotMTE)
    public GameObject slotSpecialPrefab;  // 특가 슬롯 (StoreItemSlot-2)

    [Header("부모 Transform")]
    public Transform specialOfferParent;  // 특가 상품 배치 (MaterialStorePanel > SpecialOfferPanel > BackGroundContentImg)
    public Transform materialParent;      // 재료 상점 Content (ScrollView > Viewport > Content)
    public Transform enhanceParent;       // 강화 상점 Content   (ScrollView > Viewport > Content)
    public Transform todayBgParent;       // 오늘의 상품 BackGroundImg

    [Header("템플릿 데이터 (Inspector)")]
    [Tooltip("재료 상점용 기본 아이템 템플릿\n(itemCategory = Material, effectType = None)")]
    public List<StoreItemData> materialTemplates;

    [Tooltip("강화 상점용 기본 아이템 템플릿\n(itemCategory = Enhance, effectType = None)")]
    public List<StoreItemData> enhanceTemplates;

    [Tooltip("오늘의 상품 “효과 버전” 템플릿은 따로 두지 않습니다.\n(재료/강화 템플릿에서 뽑아와서 효과를 입힐 예정)")]
    public List<StoreItemData> blankList;
    // → 빈 리스트를 사용하므로 인스펙터에 데이터 채우지 마세요.

    // 내부 상태 리스트
    private List<StoreItemData> currentMaterialItems = new List<StoreItemData>();
    private List<StoreItemData> currentEnhanceItems = new List<StoreItemData>();
    private List<StoreItemData> currentTodayItems = new List<StoreItemData>();
    private List<StoreItemData> currentSpecialOffers = new List<StoreItemData>();

    [Header("확률 설정")]
    [Range(0f, 1f)]
    [Tooltip("특가 상품 생성 시 오늘의 상품 전용 아이템이 섞일 확률 (예: 0.05 = 5%)")]
    public float chanceToIncludeTodayItemInSpecial = 0.05f;

    // (1) 세이브 시 현재 리스트를 꺼내올 수 있도록 public 프로퍼티 노출
    public List<StoreItemData> CurrentSpecialOffers => currentSpecialOffers;
    public List<StoreItemData> CurrentMaterialItems => currentMaterialItems;
    public List<StoreItemData> CurrentEnhanceItems => currentEnhanceItems;
    public List<StoreItemData> CurrentTodayItems => currentTodayItems;

    void Start()
    {
        // 1) 최초 실행 시, 재료/강화 상점 템플릿 복사
        CloneTemplates(materialTemplates, currentMaterialItems, false);
        CloneTemplates(enhanceTemplates, currentEnhanceItems, false);
        // TodayItems는 GenerateTodayItems()에서 매번 랜덤 생성

        // 2) 첫 슬롯 로드
        RefreshAll();
    }

    /// <summary>
    /// 저장된 데이터에서 복원할 때 호출
    /// </summary>
    public void LoadFromSave(
        List<StoreItemRecord> special,
        List<StoreItemRecord> material,
        List<StoreItemRecord> enhance,
        List<StoreItemRecord> today)
    {
        // 1) DTO → StoreItemData 변환
        currentSpecialOffers = special.Select(r => new StoreItemData
        {
            icon = Resources.Load<Sprite>(r.iconName),
            itemName = r.iconName,  // 필요하다면 별도 이름 필드로 바꾸세요
            baseCategory = Enum.Parse<StoreItemCategory>(r.baseCategory),
            itemCategory = Enum.Parse<StoreItemCategory>(r.itemCategory),
            price = r.price,
            minPrice = r.price,
            maxPrice = r.price,
            amount = r.amount,
            effectType = Enum.Parse<EffectType>(r.effectType)
        }).ToList();

        currentMaterialItems = material.Select(r => new StoreItemData
        {
            icon = Resources.Load<Sprite>(r.iconName),
            itemName = r.iconName,
            baseCategory = Enum.Parse<StoreItemCategory>(r.baseCategory),
            itemCategory = Enum.Parse<StoreItemCategory>(r.itemCategory),
            price = r.price,
            minPrice = r.price,
            maxPrice = r.price,
            amount = r.amount,
            effectType = Enum.Parse<EffectType>(r.effectType)
        }).ToList();

        currentEnhanceItems = enhance.Select(r => new StoreItemData
        {
            icon = Resources.Load<Sprite>(r.iconName),
            itemName = r.iconName,
            baseCategory = Enum.Parse<StoreItemCategory>(r.baseCategory),
            itemCategory = Enum.Parse<StoreItemCategory>(r.itemCategory),
            price = r.price,
            minPrice = r.price,
            maxPrice = r.price,
            amount = r.amount,
            effectType = Enum.Parse<EffectType>(r.effectType)
        }).ToList();

        currentTodayItems = today.Select(r => new StoreItemData
        {
            icon = Resources.Load<Sprite>(r.iconName),
            itemName = r.iconName,
            baseCategory = Enum.Parse<StoreItemCategory>(r.baseCategory),
            itemCategory = Enum.Parse<StoreItemCategory>(r.itemCategory),
            price = r.price,
            minPrice = r.price,
            maxPrice = r.price,
            amount = r.amount,
            effectType = Enum.Parse<EffectType>(r.effectType)
        }).ToList();

        // 2) 화면에 그대로 뿌려주기 (랜덤 생성 로직 건너뜀)
        ClearChildren(specialOfferParent);
        LoadList(currentSpecialOffers, specialOfferParent, slotSpecialPrefab);

        ClearChildren(materialParent);
        LoadList(currentMaterialItems, materialParent, slotNormalPrefab);

        ClearChildren(enhanceParent);
        LoadList(currentEnhanceItems, enhanceParent, slotNormalPrefab);

        ClearChildren(todayBgParent);
        LoadList(currentTodayItems, todayBgParent, slotNormalPrefab);
    }

    /// <summary>
    /// 저장 없는 일반 리프레시 (랜덤 생성)
    /// </summary>
    public void RefreshAll()
    {
        // 1) 재료/강화 상점 리스트 완전 초기화
        currentMaterialItems.Clear();
        currentEnhanceItems.Clear();
        CloneTemplates(materialTemplates, currentMaterialItems, false);
        CloneTemplates(enhanceTemplates, currentEnhanceItems, false);

        // 2) 특가 및 오늘의 상품, 가격 새로 생성
        GenerateSpecialOffers();
        UpdatePriceList(materialTemplates, currentMaterialItems);
        UpdatePriceList(enhanceTemplates, currentEnhanceItems);
        GenerateTodayItems();

        // 3) 화면에 새로 뿌리기
        ClearChildren(specialOfferParent);
        LoadList(currentSpecialOffers, specialOfferParent, slotSpecialPrefab);

        ClearChildren(materialParent);
        LoadList(currentMaterialItems, materialParent, slotNormalPrefab);

        ClearChildren(enhanceParent);
        LoadList(currentEnhanceItems, enhanceParent, slotNormalPrefab);

        ClearChildren(todayBgParent);
        LoadList(currentTodayItems, todayBgParent, slotNormalPrefab);
    }

    /// <summary>
    /// 템플릿 리스트를 복사해서 내부 사용 리스트 초기화
    /// - randomizeAmount=false : 템플릿에 설정된 amount(0=무제한) 그대로 복사
    /// </summary>
    void CloneTemplates(List<StoreItemData> templates, List<StoreItemData> target, bool randomizeAmount)
    {
        foreach (var t in templates)
        {
            int amt = randomizeAmount ? Random.Range(1, 6) : t.amount;
            target.Add(new StoreItemData
            {
                icon = t.icon,
                itemName = t.itemName,
                itemCategory = t.itemCategory,
                price = t.price,
                minPrice = t.minPrice,
                maxPrice = t.maxPrice,
                amount = amt,
                effectType = t.effectType // 기본 템플릿 effectType은 None
            });
        }
    }

    /// <summary>
    /// 특가 상품 3개 생성
    /// - materialTemplates 중에서 랜덤 3개 선택
    /// - 확률(chanceToIncludeTodayItemInSpecial)로 오늘의 상품 전용 아이템 1개 섞음
    ///   → Today 전용 아이템은 materialTemplates + enhanceTemplates 전체에서 뽑아서
    ///      itemCategory = StoreItemCategory.TodaySpecial, amount=1로 고정
    /// </summary>
    void GenerateSpecialOffers()
    {
        currentSpecialOffers.Clear();
        var usedIdx = new HashSet<int>();
        int totalTemplates = materialTemplates.Count;
        int pickCount = Mathf.Min(3, totalTemplates);

        // TodaySpecial 섞을 확률 판단
        bool includeToday = Random.value < chanceToIncludeTodayItemInSpecial;
        if (includeToday)
        {
            List<StoreItemData> combined = new List<StoreItemData>();
            combined.AddRange(materialTemplates);
            combined.AddRange(enhanceTemplates);

            if (combined.Count > 0)
            {
                int randIdx = Random.Range(0, combined.Count);
                var baseData = combined[randIdx];

                EffectType randomEffect = (Random.value < 0.5f)
                    ? EffectType.EasyMiniGame
                    : EffectType.NoBelowBQuality;

                int newPrice = Mathf.RoundToInt(baseData.price * Random.Range(0.2f, 0.9f));
                newPrice = Mathf.Clamp(newPrice, baseData.minPrice, baseData.maxPrice);

                // 재고 1개
                currentSpecialOffers.Add(new StoreItemData
                {
                    icon = baseData.icon,
                    itemName = baseData.itemName,

                    // 원래 카테고리 유지
                    baseCategory = baseData.baseCategory,

                    // 오늘의 상품으로 바꿈
                    itemCategory = StoreItemCategory.TodaySpecial,

                    price = newPrice,
                    minPrice = baseData.minPrice,
                    maxPrice = baseData.maxPrice,
                    amount = 1,
                    effectType = randomEffect
                });

                pickCount--;
            }
        }

        // 나머지 pickCount만큼 일반 재료 템플릿에서 랜덤 선택
        while (usedIdx.Count < pickCount)
        {
            usedIdx.Add(Random.Range(0, totalTemplates));
        }
        foreach (int idx in usedIdx)
        {
            var t = materialTemplates[idx];
            int amt = Random.Range(1, 6);

            float factor = (Random.value < 0.5f)
                ? Random.Range(0.2f, 0.5f)
                : Random.Range(0.5f, 0.9f);
            int newPrice = Mathf.RoundToInt(t.price * factor);
            newPrice = Mathf.Clamp(newPrice, t.minPrice, t.maxPrice);

            currentSpecialOffers.Add(new StoreItemData
            {
                icon = t.icon,
                itemName = t.itemName,

                baseCategory = t.baseCategory,         // ← 여기!
                itemCategory = t.itemCategory,         // (=Material)

                price = newPrice,
                minPrice = t.minPrice,
                maxPrice = t.maxPrice,
                amount = amt,
                effectType = t.effectType
            });
        }
    }

    /// <summary>
    /// 오늘의 상품 6개 랜덤 생성
    /// - 재료 + 강화 템플릿 합쳐서 랜덤 6개 뽑기
    /// - itemCategory = StoreItemCategory.TodaySpecial, 랜덤 effectType 부여
    /// - amount = 랜덤(1~5), 가격 = base*1.2~2.0 clamp(min/max)
    /// </summary>
    void GenerateTodayItems()
    {
        currentTodayItems.Clear();
        List<StoreItemData> combined = new List<StoreItemData>();
        combined.AddRange(materialTemplates);
        combined.AddRange(enhanceTemplates);

        int count = Mathf.Min(6, combined.Count);
        var usedIdx = new HashSet<int>();
        while (usedIdx.Count < count)
            usedIdx.Add(Random.Range(0, combined.Count));

        foreach (int idx in usedIdx)
        {
            var baseData = combined[idx];
            int amt = Random.Range(1, 2); // 재고 1개만

            float mul = Random.Range(1.2f, 2.0f);
            int newPrice = Mathf.RoundToInt(baseData.price * mul);
            newPrice = Mathf.Clamp(newPrice, baseData.minPrice, baseData.maxPrice);

            EffectType randomEffect = (Random.value < 0.5f)
                ? EffectType.EasyMiniGame
                : EffectType.NoBelowBQuality;

            currentTodayItems.Add(new StoreItemData
            {
                icon = baseData.icon,
                itemName = baseData.itemName,

                baseCategory = baseData.baseCategory,       // ← 원래 카테고리 복사
                itemCategory = StoreItemCategory.TodaySpecial,

                price = newPrice,
                minPrice = baseData.minPrice,
                maxPrice = baseData.maxPrice,
                amount = amt,
                effectType = randomEffect
            });
        }
    }

    /// <summary>
    /// 재료/강화 상점 가격 변동 (주식처럼 ±10%), clamp(minPrice, maxPrice)
    /// </summary>
    void UpdatePriceList(List<StoreItemData> templates, List<StoreItemData> currentList)
    {
        for (int i = 0; i < templates.Count; i++)
        {
            var t = templates[i];
            var c = currentList[i];

            float change = Random.Range(-c.price * 0.1f, c.price * 0.1f);
            int newPrice = Mathf.RoundToInt(c.price + change);
            newPrice = Mathf.Clamp(newPrice, t.minPrice, t.maxPrice);

            c.price = newPrice;
            // amount(재고)는 템플릿에 설정된 값 유지 (0이면 무제한)
            currentList[i] = c;
        }
    }

    /// <summary>
    /// 슬롯 생성 및 초기화 호출
    /// </summary>
    void LoadList(List<StoreItemData> list, Transform parent, GameObject prefab)
    {
        ClearChildren(parent);
        foreach (var item in list)
        {
            var go = Instantiate(prefab, parent);
            go.GetComponent<StoreItemSlotController>().Init(item);
        }
    }

    /// <summary>
    /// 부모 Transform의 자식 모두 삭제
    /// </summary>
    void ClearChildren(Transform parent)
    { 
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }
}