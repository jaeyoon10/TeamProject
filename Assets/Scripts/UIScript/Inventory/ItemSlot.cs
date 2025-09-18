using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlot : MonoBehaviour
{
    [Header("슬롯 구성 요소")]
    public Image backgroundImage;   // ← 배경 이미지 (회색/보라/파랑)
    public Image icon;              // ← 아이템 아이콘
    public TMP_Text quantityText;   // ← 하단 수량 표시

    /// <summary>
    /// 인벤토리에 추가될 때 호출됩니다.
    /// </summary>
    public void Init(InventoryItem item)
    {

        // 1) 아이콘 세팅
        icon.sprite = item.icon;
        icon.preserveAspect = true;

        // 2) 배경 색상 세팅 (effectType에 따라)
        switch (item.effectType)
        {
            case EffectType.EasyMiniGame:
                backgroundImage.color = new Color(0.7f, 0.5f, 0.8f);  // 보라 예시
                break;
            case EffectType.NoBelowBQuality:
                backgroundImage.color = new Color(0.4f, 0.6f, 1.0f);  // 파랑 예시
                break;
            default:
                backgroundImage.color = new Color(0.9f, 0.9f, 0.9f);  // 기본 회색
                break;
        }

        // 3) 수량 표시
        quantityText.text = item.quantity.ToString();
    }
}
