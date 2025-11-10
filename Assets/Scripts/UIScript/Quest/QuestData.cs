using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestData
{
    [Header("표시용")]
    public Sprite characterSprite;
    public string questName;
    [TextArea] public string description;

    [Header("진행도")]
    public int targetCount = 1;         // 요구 횟수/수량
    public int currentCount = 0;        // 진행 중 수량

    [Header("보상")]
    public int rewardExp = 0;
    public int rewardGold = 0;

    [Header("상태")]
    public bool isClaimed = false;      // 보상 수령 여부

    public string progressKey;
    public bool IsCompleted => currentCount >= targetCount;
    public float Progress01 => targetCount > 0 ? Mathf.Clamp01((float)currentCount / targetCount) : 1f;

    public void AddProgress(int amount = 1)
    {
        currentCount = Mathf.Clamp(currentCount + amount, 0, targetCount);
    }
}   