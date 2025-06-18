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

    public CharacterInfoManager characterInfoManager; 
    private void Awake()
    {
        inventoryUI = FindObjectOfType<InventoryUI>();
        characterInfoManager = FindObjectOfType<CharacterInfoManager>();
        if (characterInfoManager == null)
        {
            Debug.LogWarning("[StoreItemSlot] 씬에 CharacterInfoManager가 없습니다.");
        }

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Init(StoreItemData itemData)
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
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
        /* ===== 0) 레퍼런스 확보 ===== */
        if (inventoryUI == null)
            inventoryUI = FindObjectOfType<InventoryUI>(true);           // 비활성도 탐색
        if (characterInfoManager == null)
            characterInfoManager = FindObjectOfType<CharacterInfoManager>(true);

        if (inventoryUI == null)
        {
            Debug.LogError("[StoreSlot] InventoryUI를 찾지 못했습니다. 구매 중단");
            return;
        }

        /* ===== 1) 결제(골드 차감) ===== */
        if (!MoneyManager.Instance.SpendGold(data.price))
        {
            Debug.Log("[Store] 잔액 부족: 구매 불가");
            return;
        }

        /* ===== 2) 재고 처리 ===== */
        if (data.amount > 0)          // 한정 상품
        {
            data.amount--;
            if (data.amount > 0)
            {
                amountText.text = $"수량: {data.amount}";
            }
            else                       // 품절
            {
                amountText.gameObject.SetActive(false);
                DisableAsSoldOut();
            }
        }
        /* amount == 0 → 무제한 상품은 재고 감소 없음 */

        /* ===== 3) 통계 반영 ===== */
        characterInfoManager?.AddStoreCost(data.price);

        /* ===== 4) 인벤토리 카테고리 결정 ===== */
        ItemCategory inventoryCat = ItemCategory.Weapon;    // 기본값(Material)
        if (data.itemCategory == StoreItemCategory.Enhance)
            inventoryCat = ItemCategory.Enhancement;
        else if (data.itemCategory == StoreItemCategory.TodaySpecial)
            inventoryCat = (data.baseCategory == StoreItemCategory.Material)
                           ? ItemCategory.Weapon
                           : ItemCategory.Enhancement;

        /* ===== 5) 인벤토리에 추가 ===== */
        inventoryUI.AddOrIncreaseItem(data.icon, inventoryCat, data.effectType);
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