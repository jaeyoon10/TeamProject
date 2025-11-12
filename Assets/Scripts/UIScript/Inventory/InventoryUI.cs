using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("프리팹 & 콘텐츠")]
    public GameObject itemSlotPrefab;    // ItemSlot Prefab
    public Transform content;            // ScrollView > Content

    [Header("탭 버튼")]
    public Button filterWeaponButton;
    public Button filterEnhancementButton;
    public Button filterGeneralButton;
    public Button filterQuestButton;

    [Header("정렬 & 획득 순 버튼")]
    public Button sortButton;   // 효과 있는 재료 우선 ↔ 일반 재료 우선
    public Button orderButton;  // 최신 ↔ 최초 획득 순

    [Header("아이템 데이터 (런타임)")]
    public List<InventoryItem> allItems;

    // 필터 관련
    private bool filterActive = false;               // 카테고리 필터가 켜져 있는지
    private ItemCategory currentCategory;            // 적용된 카테고리

    // 정렬 모드
    private enum SortMode { None, Effect, Acquisition }
    private SortMode sortMode = SortMode.None;
    private bool effectFirst = true;    // 효과 재료 우선 모드 토글 플래그
    private bool newestFirst = true;    // 획득 순 토글 플래그

    private bool pickMode = false;
    private System.Action<InventoryItem> pickHandler;
    private System.Predicate<InventoryItem> pickFilter;

    private void Awake()
    {
        if (allItems == null)
            allItems = new List<InventoryItem>();
    }

    private void Start()
    {
        // 필터 버튼
        filterWeaponButton.onClick.AddListener(() => OnFilterCategory(ItemCategory.Weapon));
        filterEnhancementButton.onClick.AddListener(() => OnFilterCategory(ItemCategory.Enhancement));
        filterGeneralButton.onClick.AddListener(() => OnFilterCategory(ItemCategory.General));
        filterQuestButton.onClick.AddListener(() => OnFilterCategory(ItemCategory.Quest));

        // 정렬 버튼
        sortButton.onClick.AddListener(OnToggleEffectSort);
        orderButton.onClick.AddListener(OnToggleAcquisitionSort);

        // 최초 열 때는 전체 표시
        filterActive = false;
        Refresh();
    }

    private void OnEnable()
    {
        // 인벤토리 패널을 열 때마다 **카테고리 필터만** 초기화
        filterActive = false;
        currentCategory = ItemCategory.Weapon;
        
        Refresh();
    }

    private void OnFilterCategory(ItemCategory category)
    {
        filterActive = true;
        currentCategory = category;
        Refresh();
    }

    private void OnToggleEffectSort()
    {
        if (sortMode != SortMode.Effect)
        {
            sortMode = SortMode.Effect;
            effectFirst = true;
        }
        else
        {
            effectFirst = !effectFirst;
        }
        Refresh();
    }

    private void OnToggleAcquisitionSort()
    {
        if (sortMode != SortMode.Acquisition)
        {
            sortMode = SortMode.Acquisition;
            newestFirst = true;
        }
        else
        {
            newestFirst = !newestFirst;
        }
        Refresh();
    }

    /// <summary>강화용 “픽 모드” 켜기: 클릭 시 handler로 아이템 전달</summary>
    public void SetPickMode(System.Action<InventoryItem> handler, System.Predicate<InventoryItem> filter = null)
    {
        pickMode = true;
        pickHandler = handler;
        pickFilter = filter;
        Refresh();
    }

    /// <summary>픽 모드 해제</summary>
    public void ClearPickMode()
    {
        pickMode = false;
        pickHandler = null;
        pickFilter = null;
        Refresh();
    }

    private void OnSlotClickedForPick(InventoryItem item)
    {
        if (!pickMode) return;
        if (pickFilter != null && !pickFilter(item)) return;
        pickHandler?.Invoke(item);
        ClearPickMode();
    }

    public void Refresh()
    {
        // 1) 기존 슬롯 전부 제거
        foreach (Transform child in content)
            Destroy(child.gameObject);

        // 2) 필터링 (전체 vs 선택 카테고리) + 재고 0 이하 아이템 제외
        List<InventoryItem> filtered;
        if (filterActive)
            filtered = allItems.FindAll(x => x.category == currentCategory && x.quantity > 0);
        else
            filtered = allItems.FindAll(x => x.quantity > 0);

        // 3) 정렬 적용
        switch (sortMode)
        {
            case SortMode.Effect:
                var with = new List<InventoryItem>();
                var without = new List<InventoryItem>();
                foreach (var it in filtered)
                {
                    if (it.effectType == EffectType.None)
                        without.Add(it);
                    else
                        with.Add(it);
                }
                filtered.Clear();
                if (effectFirst)
                {
                    filtered.AddRange(with);
                    filtered.AddRange(without);
                }
                else
                {
                    filtered.AddRange(without);
                    filtered.AddRange(with);
                }
                break;

            case SortMode.Acquisition:
                if (newestFirst)
                    filtered.Reverse();
                break;

            case SortMode.None:
            default:
                break;
        }

        // 4) 슬롯 생성 & Init 호출
        foreach (var item in filtered)
        {
            var slotGO = Instantiate(itemSlotPrefab, content);
            var slot = slotGO.GetComponent<ItemSlot>();

            //  변경: 픽 모드일 때는 클릭 콜백을 넘겨줌
            if (pickMode)
                slot.Init(item, OnSlotClickedForPick);
            else
                slot.Init(item, null);
        }
    }

    public void AddOrIncreaseItem(Sprite icon, ItemCategory category, EffectType effectType)
    {

        var existing = allItems.Find(x => x.icon == icon && x.category == category && x.effectType == effectType);
        if (existing != null)
            existing.quantity += 1;
        else
            allItems.Add(new InventoryItem
            {
                icon = icon,
                category = category,
                effectType = effectType,
                quantity = 1
            });
        Refresh();
    }
}