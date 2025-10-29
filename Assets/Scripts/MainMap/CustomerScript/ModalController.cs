using System;
using UnityEngine;

public static class ModalController
{
    private static int openCount = 0;
    public static bool IsOpen => openCount > 0;

    public static event Action<bool> OnChanged; 

    public static void Show()
    {
        openCount++;
        Debug.Log($"[ModalController] Show ¡æ openCount = {openCount}");
        OnChanged?.Invoke(true);
    }

    public static void Hide()
    {
        openCount = Mathf.Max(0, openCount - 1);
        Debug.Log($"[ModalController] Hide ¡æ openCount = {openCount}");
        if (openCount == 0)
            OnChanged?.Invoke(false);
    }
}
