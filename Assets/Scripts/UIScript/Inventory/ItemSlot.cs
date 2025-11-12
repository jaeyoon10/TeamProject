using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlot : MonoBehaviour
{
    [Header("슬롯 구성 요소")]
    public Image backgroundImage;   // ← 배경 이미지 (회색/보라/파랑)
    public Image icon;              // ← 아이템 아이콘
    public TMP_Text quantityText;   // ← 하단 수량 표시

    private InventoryItem bound;
    private System.Action<InventoryItem> clickCb;  

    public void Init(InventoryItem item, System.Action<InventoryItem> onClick = null)
    {

        bound = item;
        clickCb = onClick;

        icon.sprite = item.icon;
        icon.preserveAspect = true;

        switch (item.effectType)
        {
            case EffectType.EasyMiniGame:
                backgroundImage.color = new Color(0.7f, 0.5f, 0.8f);
                break;
            case EffectType.NoBelowBQuality:
                backgroundImage.color = new Color(0.4f, 0.6f, 1.0f);
                break;
            default:
                backgroundImage.color = new Color(0.9f, 0.9f, 0.9f);
                break;
        }

        quantityText.text = item.quantity.ToString();
    }

    //  Button.onClick에 연결
    public void OnClick()
    {
        clickCb?.Invoke(bound);
    }
}
