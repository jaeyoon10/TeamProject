using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class IngItem : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    public Image iconImg;
    public TMP_Text countText;

    // Init 호출 시 세팅되는 값들
    string itemName;

    /// <summary>
    /// CraftingUI.ShowDetail() 에서 이 메서드를 통해 초기화합니다.
    /// </summary>
    public void Init(string name, Sprite icon, int amount)
    {
        itemName = name;
        iconImg.sprite = icon;
        countText.text = amount.ToString();
    }

    // 마우스 올릴 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        Tooltip.Instance.Show(itemName, transform as RectTransform);
    }

    // 마우스 떼면 숨기기
    public void OnPointerExit(PointerEventData eventData)
    {
        Tooltip.Instance.Hide();
    }
}
