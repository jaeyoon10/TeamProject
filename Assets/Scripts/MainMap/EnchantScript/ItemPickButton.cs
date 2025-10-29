using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemPickButton : MonoBehaviour
{
    public Image icon;
    public TMP_Text countText;

    private InventoryItem bound;
    private System.Action<InventoryItem> onPick;

    public void Bind(InventoryItem item, System.Action<InventoryItem> callback)
    {
        bound = item;
        onPick = callback;

        if (icon) icon.sprite = item.icon;
        if (countText) countText.text = item.quantity.ToString();
    }

    public void OnClick()
    {
        onPick?.Invoke(bound);
    }
}