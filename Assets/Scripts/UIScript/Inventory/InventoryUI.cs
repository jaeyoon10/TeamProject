using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameObject itemSlotPrefab;
    public Transform content;
    public Button sortButton;
    public Button orderButton;
    public Button[] tabButtons;

    public List<InventoryItem> allItems;

    private ItemCategory currentCategory = ItemCategory.Weapon;
    private bool descending = false;

    private void Awake()
    {
        allItems = new List<InventoryItem>();

        allItems.Add(new InventoryItem { icon = null, enhancementLevel = 3, rarity = Rarity.Common, category = ItemCategory.Weapon });
        allItems.Add(new InventoryItem { icon = null, enhancementLevel = 100, rarity = Rarity.Legendary, category = ItemCategory.Weapon });
        allItems.Add(new InventoryItem { icon = null, enhancementLevel = 2, rarity = Rarity.Epic, category = ItemCategory.Enhancement });
        allItems.Add(new InventoryItem { icon = null, enhancementLevel = 7, rarity = Rarity.Legendary, category = ItemCategory.General });
        allItems.Add(new InventoryItem { icon = null, enhancementLevel = 1, rarity = Rarity.Common, category = ItemCategory.Quest });
    }

    private void Start()
    {
        sortButton.onClick.AddListener(ToggleSortOrder);
        orderButton.onClick.AddListener(ToggleSortOrder);

        for (int i = 0; i < tabButtons.Length; i++)
        {
            int index = i;
            tabButtons[i].onClick.AddListener(() => ChangeCategory((ItemCategory)index));
        }

        Refresh();
    }

    private void ToggleSortOrder()
    {
        descending = !descending;
        Refresh();
    }

    private void ChangeCategory(ItemCategory category)
    {
        currentCategory = category;
        Refresh();
    }

    private void Refresh()
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);

        List<InventoryItem> filtered = allItems.FindAll(x => x.category == currentCategory);

        if (descending)
            filtered.Sort((a, b) => b.enhancementLevel.CompareTo(a.enhancementLevel));
        else
            filtered.Sort((a, b) => a.enhancementLevel.CompareTo(b.enhancementLevel));

        foreach (var item in filtered)
        {
            var slot = Instantiate(itemSlotPrefab, content);
            slot.GetComponent<ItemSlot>().Init(item);
        }
    }
}