using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Quest", fileName = "Quest_")]
public class QuestDataSO : ScriptableObject
{
    [Header("표시")]
    public Sprite characterSprite;
    public string questName;
    [TextArea] public string description;

    [Header("목표/보상")]
    public int targetCount = 1;
    public int rewardExp = 0;
    public int rewardGold = 0;

    [Header("순서")]
    public int order = 0;   // 순차 진행용
}
