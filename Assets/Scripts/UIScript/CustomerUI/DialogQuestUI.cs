using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogQuestUI : MonoBehaviour
{
    public static DialogQuestUI Instance { get; private set; }

    [Header("DialogQuest UI")]
    public TMP_Text questText;
    public GameObject dialogquestPanel;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        if (dialogquestPanel != null)
            dialogquestPanel.SetActive(false);
    }

    public void SetQuestText(string text)
    {
        if (questText != null)
            questText.text = $"<color=#FF0000>[Quest] </color>\n<color=#FFFFFF>{text}</color>";

        if (dialogquestPanel != null)
            dialogquestPanel.SetActive(true);
    }

    public void ClearQuest()
    {
        if (dialogquestPanel != null)
            dialogquestPanel.SetActive(false);
    }
}
