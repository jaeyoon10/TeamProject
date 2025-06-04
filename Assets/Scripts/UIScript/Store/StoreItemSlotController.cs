using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreItemSlotController : MonoBehaviour
{
    [Header("UI 참조 (Inspector에서 반드시 할당)")]
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public TMP_Text amountText;
    public Button purchaseButton;
    public Image backgroundImage;

    [Header("효과별 배경색")]
    public Color colorNone = new Color(0.9f, 0.9f, 0.9f);
    public Color colorEasyMiniGame = new Color(0.7f, 0.5f, 0.8f);
    public Color colorNoBelowB = new Color(0.4f, 0.6f, 1.0f);

    private StoreItemData data;
    private InventoryUI inventoryUI;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        inventoryUI = FindObjectOfType<InventoryUI>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Init(StoreItemData itemData)
    {
        // 1) Init 시작부에서 반드시 CanvasGroup을 확보하거나 추가
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        // 2) 필수 컴포넌트 검증
        if (iconImage == null || nameText == null || priceText == null || amountText == null
            || purchaseButton == null || backgroundImage == null)
        {
            Debug.LogError("[StoreItemSlotController] Init 호출 시 필수 컴포넌트가 할당되지 않음. " +
                $"iconImage={iconImage}, nameText={nameText}, priceText={priceText}, " +
                $"amountText={amountText}, purchaseButton={purchaseButton}, backgroundImage={backgroundImage}");
            return;
        }

        data = itemData;

        // 3) 아이콘/이름/가격 설정
        iconImage.sprite = data.icon;
        iconImage.preserveAspect = true;
        nameText.text = data.itemName;
        priceText.text = data.price.ToString();

        // 4) 재고 표시: amount > 0인 경우에만 “수량” 보여줌
        if (data.amount > 0)
        {
            amountText.gameObject.SetActive(true);
            amountText.text = "수량: " + data.amount;

            // 정상 상태
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            purchaseButton.interactable = true;
        }
        else
        {
            // amount == 0 → 무제한 판매 상태
            amountText.gameObject.SetActive(false);
            // “무제한”이므로 반투명이나 비활성화는 하지 않음
            canvasGroup.alpha = 1f;
            purchaseButton.interactable = true;
        }

        // 5) 효과(effectType)에 따라 배경색 적용
        switch (data.effectType)
        {
            case EffectType.EasyMiniGame:
                backgroundImage.color = colorEasyMiniGame;
                break;
            case EffectType.NoBelowBQuality:
                backgroundImage.color = colorNoBelowB;
                break;
            default:
                backgroundImage.color = colorNone;
                break;
        }

        // 6) 구매 버튼 이벤트 연결
        purchaseButton.onClick.RemoveAllListeners();
        purchaseButton.onClick.AddListener(OnClickPurchase);
    }

    private void OnClickPurchase()
    {
        // 1) 재고 amount > 0인 “한정 상품”만 실제 재고를 빼고, 0이 되면 품절 처리
        if (data.amount > 0)
        {
            // 1-1) 구매 가능 여부(골드 확인)
            bool paid = MoneyManager.Instance.SpendGold(data.price);
            if (!paid)
            {
                Debug.Log("[Store] 잔액 부족: 구매 불가");
                return;
            }

            // 1-2) 재고 차감 후 UI 갱신
            data.amount--;
            Debug.Log($"[Store] '{data.itemName}' 남은 재고: {data.amount}");

            if (data.amount > 0)
            {
                amountText.gameObject.SetActive(true);
                amountText.text = "수량: " + data.amount;
            }
            else
            {
                amountText.gameObject.SetActive(false);
                DisableAsSoldOut(); // 재고 0이므로 품절 처리
            }
        }
        else
        {
            // 2) amount == 0인 “무제한 상품”
            bool paid = MoneyManager.Instance.SpendGold(data.price);
            if (!paid)
            {
                Debug.Log("[Store] 잔액 부족: 구매 불가");
                return;
            }
            // (무제한 상품이므로 재고 감소/품절 처리 없음)
        }

        // 3) 인벤토리 카테고리 매핑
        ItemCategory inventoryCat;
        if (data.itemCategory == StoreItemCategory.Material)
        {
            inventoryCat = ItemCategory.Weapon;
        }
        else if (data.itemCategory == StoreItemCategory.Enhance)
        {
            inventoryCat = ItemCategory.Enhancement;
        }
        else // TodaySpecial
        {
            inventoryCat = (data.baseCategory == StoreItemCategory.Material)
                ? ItemCategory.Weapon
                : ItemCategory.Enhancement;
        }

        // 4) 인벤토리에 추가
        inventoryUI.AddOrIncreaseItem(
            data.icon,
            inventoryCat,
            data.effectType
        );
    }

    private void DisableAsSoldOut()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.5f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        if (purchaseButton != null)
            purchaseButton.interactable = false;
    }
}