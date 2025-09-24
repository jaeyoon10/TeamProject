using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Unlock Item", fileName = "NewUnlockItem")]
public class UnlockItemSO : ScriptableObject
{
    public int unlockLevel;   // 해금되는 레벨
    public Sprite icon;       // 아이콘
    [TextArea] public string description; // 설명 텍스트
}