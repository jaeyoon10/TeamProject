using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestBridge
{
    public static void Progress(string key, int amount = 1)
    {
        var ui = Object.FindObjectOfType<QuestUIManager>(true);
        ui?.AddProgressTo(key, amount);
    }
}