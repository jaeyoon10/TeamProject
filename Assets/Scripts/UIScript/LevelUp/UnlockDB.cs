using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Unlock DB", fileName = "UnlockDB")]
public class UnlockDB : ScriptableObject
{
    public List<UnlockItemSO> unlockItems;

    public List<UnlockItemSO> GetItemsForLevel(int level)
    {
        return unlockItems.Where(i => i.unlockLevel == level).ToList();
    }
}