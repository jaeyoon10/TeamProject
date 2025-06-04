using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("프리팹 & 콘텐츠")]
    public GameObject itemSlotPrefab;    // ItemSlot Prefab
    public Transform content;            // ScrollView > Content

    [Header("탭 & 정렬 버튼")]
    public Button filterWeaponButton;        // “무기 재료” 버튼
    public Button filterEnhancementButton;   // “강화 재료” 버튼
    public Button filterGeneralButton;       //  "일상 재료"  버튼
    public Button filterQuestButton;         //  "퀘스트 재료" 버튼
    // 필요한 만큼 버튼이 있다면 추가 (예: 일반 재료, 퀘스트 등)

    [Header("정렬 & 획득 순 버튼")]
    public Button sortButton;   // “정렬” 버튼: 능력 있는 재료 우선 ↔ 일반 재료 우선
    public Button orderButton;  // “획득 순” 버튼: 최신 ↔ 최초

    [Header("아이템 데이터 (런타임)")]
    public List<InventoryItem> allItems;    // 인벤토리에 담길 모든 아이템

    // 내부 상태 변수
    private bool filterActive = false;            // 카테고리 필터링이 활성화되었는지
    private ItemCategory currentCategory = ItemCategory.Weapon; // 필터링 대상 카테고리
    private enum SortMode { None, Effect, Acquisition }
    private SortMode sortMode = SortMode.None;
    private bool effectFirst = true;      // 정렬 → true = “능력 있는 재료 먼저” / false = “일반 재료 먼저”
    private bool newestFirst = true;      // 획득 순 → true = 최신(나중에 획득한 아이템) 먼저 / false = 최초(가장 먼저 획득)

    private void Awake()
    {
        allItems = new List<InventoryItem>();
    }

    private void Start()
    {
        // 1) 필터 버튼 연결
        filterWeaponButton.onClick.AddListener(() => OnFilterCategory(ItemCategory.Weapon));
        filterEnhancementButton.onClick.AddListener(() => OnFilterCategory(ItemCategory.Enhancement));
        filterGeneralButton.onClick.AddListener(() => OnFilterCategory(ItemCategory.General));
        filterQuestButton.onClick.AddListener(() => OnFilterCategory(ItemCategory.Quest));

        // 2) 정렬 버튼 연결
        sortButton.onClick.AddListener(OnToggleEffectSort);

        // 3) 획득 순 버튼 연결
        orderButton.onClick.AddListener(OnToggleAcquisitionSort);

        // 초기 화면: “전체” (필터 비활성)
        filterActive = false;
        sortMode = SortMode.None;
        Refresh();
    }

    /// <summary>
    /// 카테고리 필터링 버튼 클릭 시 호출
    /// </summary>
    private void OnFilterCategory(ItemCategory category)
    {
        filterActive = true;
        currentCategory = category;
        // 카테고리가 바뀌면 정렬 모드 초기화
        sortMode = SortMode.None;
        Refresh();
    }

    /// <summary>
    /// 정렬 버튼 (능력 있는 재료 먼저 ↔ 일반 재료 먼저) 토글
    /// </summary>
    private void OnToggleEffectSort()
    {
        // 정렬 모드를 “Effect” 로 설정하거나, 이미 Effect 모드라면 순서 반전
        if (sortMode != SortMode.Effect)
        {
            sortMode = SortMode.Effect;
            effectFirst = true;
        }
        else
        {
            // 이미 Effect 정렬 모드일 때, 순서만 바꿈
            effectFirst = !effectFirst;
        }
        Refresh();
    }

    /// <summary>
    /// 획득 순 버튼 (최신 먼저 ↔ 최초 먼저) 토글
    /// </summary>
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

    /// <summary>
    /// UI를 다시 그리는 메인 함수
    /// </summary>
    private void Refresh()
    {
        // 1) 기존 슬롯 전부 제거
        foreach (Transform child in content)
            Destroy(child.gameObject);

        // 2) 아이템 필터링
        List<InventoryItem> filtered;
        if (!filterActive)
        {
            // 필터가 활성화되지 않았다면, 전체 아이템
            filtered = new List<InventoryItem>(allItems);
        }
        else
        {
            // 필터가 활성화되었다면, 카테고리별로
            filtered = allItems.FindAll(x => x.category == currentCategory);
        }

        // 3) 정렬 모드에 따라 순서 조정
        switch (sortMode)
        {
            case SortMode.Effect:
                // effectType != None인 아이템을 먼저 또는 나중으로 배치
                // stable partition: 같은 순서를 유지하면서 두 그룹으로 나누기
                List<InventoryItem> groupWith = new List<InventoryItem>();
                List<InventoryItem> groupWithout = new List<InventoryItem>();
                foreach (var it in filtered)
                {
                    if (it.effectType == EffectType.None)
                        groupWithout.Add(it);
                    else
                        groupWith.Add(it);
                }

                if (effectFirst)
                {
                    // 능력 있는(re: effectType != None) → 일반 순서
                    filtered.Clear();
                    filtered.AddRange(groupWith);
                    filtered.AddRange(groupWithout);
                }
                else
                {
                    // 일반(re: effectType == None) → 능력 있는 순서
                    filtered.Clear();
                    filtered.AddRange(groupWithout);
                    filtered.AddRange(groupWith);
                }
                break;

            case SortMode.Acquisition:
                // “획득 순” = allItems 리스트의 삽입 순서(= 획득 순)을 사용
                // filtered는 FindAll 순서대로 (즉, “획득 순” → “나중에 들어온 순”이 뒤쪽에 위치)
                // newestFirst == true → “나중에 획득한 아이템”을 앞으로(인덱스0) 옮겨야 함 → 역순
                if (newestFirst)
                {
                    filtered.Reverse();
                }
                // newestFirst == false → “가장 먼저 획득한”이 앞 → 그냥 그대로
                break;

            case SortMode.None:
            default:
                // 정렬 없이, 원래 리스트 상태(획득 순)가 유지됨
                break;
        }

        // 4) 슬롯 생성 및 Init
        foreach (var item in filtered)
        {
            var slotGO = Instantiate(itemSlotPrefab, content);
            slotGO.GetComponent<ItemSlot>().Init(item);
        }
    }

    /// <summary>
    /// 상점 등에서 아이템을 구매할 때 호출
    /// </summary>
    /// <param name="icon">아이템 아이콘(Sprite)</param>
    /// <param name="category">ItemCategory (Weapon, Enhancement ...)</param>
    /// <param name="effectType">EffectType (None, EasyMiniGame, NoBelowBQuality)</param>
    public void AddOrIncreaseItem(Sprite icon, ItemCategory category, EffectType effectType)
    {
        // 같은 종류(아이콘+카테고리+effectType)가 있는지 찾는다
        InventoryItem existing = allItems.Find(x =>
            x.icon == icon &&
            x.category == category &&
            x.effectType == effectType
        );

        if (existing != null)
        {
            // 이미 있으면 수량만 증가
            existing.quantity += 1;
        }
        else
        {
            // 새로 생성해서 리스트에 추가
            InventoryItem newItem = new InventoryItem
            {
                icon = icon,
                category = category,
                effectType = effectType,
                quantity = 1
            };
            allItems.Add(newItem);
        }

        // 항목이 추가되면 ⇒ 기본적으로는 “획득 순(최초=앞쪽)”로 allItems에 들어감
        // Refresh() 호출 시, sortMode가 None이면 insertion order(획득 순, oldest-first)가 유지됨
        Refresh();
    }
}