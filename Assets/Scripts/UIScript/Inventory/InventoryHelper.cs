using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryHelper
{
    public static bool HasMaterials(List<InventoryItem> allItems, RecipeData recipe)
    {
        foreach (var ing in recipe.ingredients)
        {
            var match = allItems.Find(x => x.icon == ing.icon);
            if (match == null || match.quantity < ing.amount)
                return false;
        }
        return true;
    }

    public static void ConsumeMaterials(List<InventoryItem> allItems, RecipeData recipe)
    {
        foreach (var ing in recipe.ingredients)
        {
            var match = allItems.Find(x => x.icon == ing.icon);
            if (match != null)
                match.quantity -= ing.amount;
        }
    }
}
