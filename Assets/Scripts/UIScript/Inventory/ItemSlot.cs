using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlot : MonoBehaviour
{
    public Image icon;
    public Image frame;
    public TMP_Text numberText;

    public void Init(InventoryItem item)
    {
        icon.sprite = item.icon;
        numberText.text = $"{item.enhancementLevel}";
        frame.color = GetColorByRarity(item.rarity);
    }

    private Color GetColorByRarity(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => Color.white,
            Rarity.Rare => Color.blue,
            Rarity.Epic => new Color(0.6f, 0f, 1f),
            Rarity.Legendary => Color.yellow,
            _ => Color.white,
        };
    }
}

