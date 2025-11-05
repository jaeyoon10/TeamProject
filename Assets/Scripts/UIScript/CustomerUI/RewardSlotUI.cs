using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardSlotUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text amountText;

    /// <param name="prefixX">돈/경험치처럼 'x'를 붙일지 여부 (기본 true)</param>
    public void Set(Sprite sprite, int value, bool prefixX = true, Color? textColor = null)
    {
        icon.sprite = sprite;

        if (prefixX) amountText.text = $"x{value}";
        else amountText.text = value >= 0 ? $"+{value}" : $"{value}";

        if (textColor.HasValue)
            amountText.color = textColor.Value;
    }
}
