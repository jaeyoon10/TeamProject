using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestOpner : MonoBehaviour
{

    [Header("Panel -> Open,Close 버튼")]
    public Button questOpenButton;
    public Button questCloseButton;
    public GameObject questPanel;
    // Start is called before the first frame update
    void Start()
    {
        // 시작할 때 QuestPanel을 꺼 두고,
        if (questPanel.activeSelf)
            questPanel.SetActive(false);

        // 열기 버튼 누르면 ShowQuestPanel() 호출
        questOpenButton.onClick.AddListener(ShowQuestPanel);

        // 닫기 버튼 누르면 CloseQuestPanel() 호출
        questCloseButton.onClick.AddListener(CloseQuestPanel);
    }

    void ShowQuestPanel()
    {
        questPanel.SetActive(true);
    }

    void CloseQuestPanel()
    {
        questPanel.SetActive(false);
    }
}
