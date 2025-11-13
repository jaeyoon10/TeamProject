using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnchantUIManager : MonoBehaviour
{
    public static EnchantUIManager Instance { get; private set; }

    [Header("Root Panels")]
    public GameObject panel;          // EnchantPanel (비활성 시작)
    public GameObject selectPanel;    // 오른쪽 오버레이 선택 패널 (비활성 시작)
    public Button btnCloseSelect;     // X 버튼

    [Header("Left Area Widgets")]
    public Image weaponIcon;          // 무기 아이콘(맨 위 원)
    public Button matSlotA; public Image matSlotAIcon;   // 왼쪽 아래 원
    public Button matSlotB; public Image matSlotBIcon;   // 오른쪽 아래 원
    public TMP_Text levelText;        // "+1 ⇒ +2" 좌측 라벨
    public TMP_Text chanceText;       // 성공 확률 표시
    public TMP_Text goldBeforeText;   // "G 1000"
    public TMP_Text goldAfterText;    // "⇒ G 1700"

    [Header("Bottom Buttons")]
    public Button btnEnhance;         // 파란 "강화 하기"
    public Button btnSell;            // 빨간 "판매 하기" (원화면 버튼, 패널 뒤에 깔림)
    public Button btnShop;            // 좌상단 "상점 가기"

    [Header("Select Panel (Right)")]
    public Transform gridParent;          // GridLayoutGroup
    public ItemPickButton itemButtonPrefab;

    [Header("Chance Params")]
    [Range(0, 100)] public int baseChance = 45;     // 재료 없이 기본 성공률
    [Range(0, 100)] public int minChance = 5;
    [Range(0, 100)] public int maxChance = 95;
    [Range(0, 50)] public int levelPenalty = 7;     // 레벨당 -확률
    [Range(0, 50)] public int perMaterialBonus = 5;// 재료 1개당 +확률 (모든 재료 동일 취급)

    [SerializeField] private StoreOpener storeOpener;
    private enum PickTarget { None, A, B }
    private PickTarget pickTarget = PickTarget.None;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (panel) panel.SetActive(false);
        if (selectPanel) selectPanel.SetActive(false);

        if (matSlotA) matSlotA.onClick.AddListener(() => StartPick(PickTarget.A));
        if (matSlotB) matSlotB.onClick.AddListener(() => StartPick(PickTarget.B));

        if (btnCloseSelect) btnCloseSelect.onClick.AddListener(CloseSelect);

        if (btnEnhance) btnEnhance.onClick.AddListener(OnClickEnhance);
        if (btnSell) btnSell.onClick.AddListener(OnClickSellToCustomer);
        if (btnShop) btnShop.onClick.AddListener(OpenShop);
    }

    void StartPick(PickTarget target)
    {
        pickTarget = target;

        var inv = FindObjectOfType<InventoryUI>(true);
        var opener = FindObjectOfType<InvnetoryOpen>(true);
        if (inv == null || opener == null) return;

        // 인벤토리 열기
        opener.ShowInventoryPanel();

        // 강화 재료만 고를 수 있도록 필터 + 콜백 등록
        inv.SetPickMode(
            OnPickMaterialFromInventory,
            it => it.category == ItemCategory.Enhancement && it.quantity > 0
        );
    }

    // 추가: 인벤토리에서 아이템을 클릭했을 때 슬롯에 장착
    void OnPickMaterialFromInventory(InventoryItem item)
    {
        if (pickTarget == PickTarget.A) EnchantSession.matA = item;
        else if (pickTarget == PickTarget.B) EnchantSession.matB = item;

        // 아이콘/확률 갱신
        RefreshAll();

        // 인벤토리 닫기
        var opener = FindObjectOfType<InvnetoryOpen>(true);
        opener?.CloseInventoryPanel();

        pickTarget = PickTarget.None;
    }

    public void Show()
    {
        panel.SetActive(true);
        ModalController.Show();

        // 무기 아이콘 표시
        if (EnchantSession.Recipe && weaponIcon)
            weaponIcon.sprite = EnchantSession.Recipe.icon;

        RefreshAll();
    }

    public void Hide()
    {
        panel.SetActive(false);
        if (selectPanel) selectPanel.SetActive(false);
        ModalController.Hide();
    }

    void RefreshAll()
    {
        int cur = EnchantSession.enchantLevel;
        if (levelText)
            levelText.text = $"+{cur} => +{cur + 1}";

        int bonus = 0;
        if (EnchantSession.matA != null) bonus += perMaterialBonus;
        if (EnchantSession.matB != null) bonus += perMaterialBonus;

        int baseC = baseChance - levelPenalty * cur;
        int unclamped = baseC + bonus;
        int shown = Mathf.Clamp(unclamped, minChance, maxChance);

        if (chanceText)
        {
            if (bonus > 0)
                chanceText.text = $"성공 확률 : {shown}% (+{bonus}%)";
            else
                chanceText.text = $"성공 확률 : {shown}%";
        }
        // 슬롯 아이콘
        if (matSlotAIcon)
        {
            if (EnchantSession.matA != null)
            {
                matSlotAIcon.sprite = EnchantSession.matA.icon;
                matSlotAIcon.color = Color.white;  // 완전 보이게
            }
            else
            {
                matSlotAIcon.sprite = null;
                // 알파 0으로 해서 “안 보이게만” 함 (버튼은 살려둠)
                matSlotAIcon.color = new Color(1f, 1f, 1f, 0f);
            }
        }
        if (matSlotBIcon)
        {
            if (EnchantSession.matB != null)
            {
                matSlotBIcon.sprite = EnchantSession.matB.icon;
                matSlotBIcon.color = Color.white;
            }
            else
            {
                matSlotBIcon.sprite = null;
                matSlotBIcon.color = new Color(1f, 1f, 1f, 0f);
            }
        }

        int baseWithout = CalcBasePriceWithoutEnchant();
        int before = Mathf.RoundToInt(baseWithout * EnchantSession.GetPayMultiplierFor(EnchantSession.enchantLevel));
        int after = Mathf.RoundToInt(baseWithout * EnchantSession.GetPayMultiplierFor(EnchantSession.enchantLevel + 1));

        if (goldBeforeText) goldBeforeText.text = $"G {before}";
        if (goldAfterText) goldAfterText.text = $"=>   G {after}";
    }

    // 품질→별→타입배율까지 반영한 "시작가격"(강화 배율 제외)
    int CalcBasePriceWithoutEnchant()
    {
        var cust = WeaponCraftingManager.Instance.CurrentCustomer;
        if (cust == null || !EnchantSession.Recipe) return 0;

        int star = WeaponCraftingManager.Instance.CalcStar(EnchantSession.QualityScore);

        int basePrice = EnchantSession.Recipe.basePrice;
        int bonus = 0;
        switch (star)
        {
            case 5: bonus = +200; break;
            case 4: bonus = 0; break;
            case 3: bonus = -200; break;
            case 2: bonus = -500; break;
            default: bonus = -900; break;
        }
        int price = basePrice + bonus;

        // 손님 타입 배율
        float typeMult = (cust.type != null) ? cust.type.paymentMultiplier : 1f;
        price = Mathf.RoundToInt(price * typeMult);

        return Mathf.Max(100, price);
    }

    // ----------------- 확률 계산 -----------------
    int CalcSuccessChance()
    {
        int chance = baseChance - levelPenalty * EnchantSession.enchantLevel;
        if (EnchantSession.matA != null) chance += perMaterialBonus;
        if (EnchantSession.matB != null) chance += perMaterialBonus;
        return Mathf.Clamp(chance, minChance, maxChance);
    }

    // ----------------- 선택 패널 -----------------
    void OpenSelect(PickTarget target)
    {
        pickTarget = target;
        if (!selectPanel) return;

        selectPanel.SetActive(true);
        // 기존 버튼 정리
        foreach (Transform c in gridParent) Destroy(c.gameObject);

        // 인벤토리에서 "강화 재료"만 가져오기
        var mats = GetEnhanceMaterials();
        foreach (var it in mats)
        {
            var btn = Instantiate(itemButtonPrefab, gridParent);
            btn.Bind(it, OnPickMaterial);
        }
    }

    void CloseSelect()
    {
        pickTarget = PickTarget.None;
        if (selectPanel) selectPanel.SetActive(false);
    }

    void OnPickMaterial(InventoryItem item)
    {
        if (pickTarget == PickTarget.A) EnchantSession.matA = item;
        else if (pickTarget == PickTarget.B) EnchantSession.matB = item;

        CloseSelect();
        RefreshAll();
    }

    // ----------------- 버튼: 강화하기 -----------------
    void OnClickEnhance()
    {
        ConsumeIfAny(EnchantSession.matA);
        ConsumeIfAny(EnchantSession.matB);

        int chance = CalcSuccessChance();
        int roll = Random.Range(0, 100);

        if (roll < chance)
        {
            EnchantSession.enchantLevel++;
            // TODO: 성공 연출
        }
        else
        {
            // TODO: 실패 연출
        }

        // 한 번 사용한 재료는 슬롯에서 제거
        EnchantSession.matA = null;
        EnchantSession.matB = null;

        matSlotA.interactable = true;
        matSlotB.interactable = true;

        RefreshAll();
    }

    // ----------------- 버튼: 판매하기 -----------------
    public void OnClickSellToCustomer()
    {
        Hide();
        WeaponCraftingManager.Instance.RequestSellAfterEnchant(); // 문으로 이동 → 판매
    }

    // ----------------- 버튼: 상점 가기 -----------------
    void OpenShop()
    {
        if (storeOpener == null)
            storeOpener = FindObjectOfType<StoreOpener>(true); 

        if (storeOpener != null)
        {
            storeOpener.ShowStorePanel(); // 상점 패널 열기
        }
        else
        {
            Debug.LogWarning("[Enchant] StoreOpener를 찾지 못했습니다.");
        }
    }

    // ----------------- 인벤토리 연동 -----------------
    List<InventoryItem> GetEnhanceMaterials()
    {
        var inv = FindObjectOfType<InventoryUI>();
        if (inv == null || inv.allItems == null) return new List<InventoryItem>();

        // 카테고리 Enhancement + 수량 > 0
        var list = new List<InventoryItem>();
        foreach (var it in inv.allItems)
        {
            if (it.category == ItemCategory.Enhancement && it.quantity > 0)
                list.Add(it);
        }
        return list;
    }

    void ConsumeIfAny(InventoryItem pickedItem)
    {
        if(pickedItem == null)
        {
            Debug.Log($"[Enchant] ConumeIfAny: pickedItem == null");
            return;
        }

        Debug.Log($"[Enchant] BEFORE consume:{pickedItem.icon?.name}, qty={pickedItem.quantity}");

        pickedItem.quantity = Mathf.Max(0, pickedItem.quantity - 1);

        Debug.Log($"[Enchant] AFTER consume: {pickedItem.icon?.name}, qty={pickedItem.quantity}");

        var inv = FindObjectOfType<InventoryUI>(true);
        if(inv != null)
        {
            Debug.Log($"[Enchant] Refresh Inventory, allItems.Count = {inv.allItems?.Count}");
            inv.Refresh();
        }
        else
        {
            Debug.LogWarning("[Enchant] InventoryUI를 찾지 못했습니다");
        }
    }

}