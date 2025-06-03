using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreItemSlotController : MonoBehaviour
{
    public Image iconImage;     // 아이템 아이콘
    public TMP_Text nameText;   // 아이템 이름
    public TMP_Text priceText;  // 가격 텍스트
    public TMP_Text amountText; // 재고 수량 텍스트

    [Header("Background for Effect")]
    public Image backgroundImage; // 배경 이미지 (아이템 뒤 사각형)

    // 효과별 배경색 예시
    public Color colorNone = Color.white;                             // 기본
    public Color colorEasyMiniGame = new Color(0.7f, 0.5f, 0.8f);  // 보라
    public Color colorNoBelowB = new Color(0.4f, 0.6f, 1.0f);  // 파랑

    public void Init(StoreItemData data)
    {
        // 1) 아이콘/이름/가격/수량 설정
        iconImage.sprite = data.icon;
        nameText.text = data.itemName;
        priceText.text = data.price.ToString();

        if (amountText != null)
        {
            if (data.amount > 0)
            {
                amountText.gameObject.SetActive(true);
                amountText.text = "수량: " + data.amount;
            }
            else
            {
                amountText.gameObject.SetActive(false);
            }
        }

        // 2) 오늘의 상품 전용 효과에 따라 배경색 변경
        switch (data.effectType)
        {
            case EffectType.None:
                backgroundImage.color = colorNone;
                break;
            case EffectType.EasyMiniGame:
                backgroundImage.color = colorEasyMiniGame;
                break;
            case EffectType.NoBelowBQuality:
                backgroundImage.color = colorNoBelowB;
                break;
        }
    }
}


