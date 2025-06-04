using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreItemSlotController : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public TMP_Text amountText;
    public Button purchaseButton;       // ← 구매 버튼
    public Image backgroundImage;
        
    // 색상 예시
    public Color colorNone = new Color(0.9f, 0.9f, 0.9f);
    public Color colorEasyMiniGame = new Color(0.7f, 0.5f, 0.8f);
    public Color colorNoBelowB = new Color(0.4f, 0.6f, 1.0f);

    private StoreItemData data;
    private InventoryUI inventoryUI;    // 씬에 붙어 있는 InventoryUI 참조

    private void Awake()
    {
        // 씬에 하나만 존재한다고 가정. 
        inventoryUI = FindObjectOfType<InventoryUI>();
    }

    public void Init(StoreItemData itemData)
    {
        data = itemData;

        // 1) 아이콘/이름/가격/재고 표시
        iconImage.sprite = data.icon;
        iconImage.preserveAspect = true;
        nameText.text = data.itemName;
        priceText.text = data.price.ToString();

        if (data.amount > 0)
        {
            amountText.gameObject.SetActive(true);
            amountText.text = "수량: " + data.amount;
        }
        else
        {
            amountText.gameObject.SetActive(false);
        }

        // 2) 효과(effectType)에 따라 배경색
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

        // 3) 구매 버튼에 이벤트 연결
        purchaseButton.onClick.RemoveAllListeners();
        purchaseButton.onClick.AddListener(OnClickPurchase);
    }

    /// <summary>
    /// 구매 버튼 클릭 시 호출
    /// </summary>
    private void OnClickPurchase()
    {
        bool paid = MoneyManager.Instance.SpendGold(data.price);
        if (!paid)
        {
            Debug.Log("잔액 부족: 구매 불가");
            return;
        }

        // 재고 감소 & 반투명 처리 등(생략)

        // ------------- 여기서 카테고리 매핑 -------------
        ItemCategory inventoryCat;

        if (data.itemCategory == StoreItemCategory.Material)
        {
            inventoryCat = ItemCategory.Weapon;
        }
        else if (data.itemCategory == StoreItemCategory.Enhance)
        {
            inventoryCat = ItemCategory.Enhancement;
        }
        else // data.itemCategory == TodaySpecial
        {
            // TodaySpecial일 때는 baseCategory를 따릅니다
            if (data.baseCategory == StoreItemCategory.Material)
                inventoryCat = ItemCategory.Weapon;
            else // baseCategory == StoreItemCategory.Enhance
                inventoryCat = ItemCategory.Enhancement;
        }

        inventoryUI.AddOrIncreaseItem(
            data.icon,
            inventoryCat,
            data.effectType
        );
    }
}


