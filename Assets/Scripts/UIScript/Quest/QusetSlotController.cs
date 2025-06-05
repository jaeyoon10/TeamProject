using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QusetSlotController : MonoBehaviour
{
    [Header("UI 참조 (프리팹 내부에서 드래그 연결)")]
    public Image questCharacterImg;   // 의뢰인(기사) 이미지
    public TMP_Text GuestText;        // 의뢰인 이름
    public TMP_Text descriptiontext;  // 의뢰 설명
    public TMP_Text rewordtext;       // 보상 텍스트

    /// <summary>
    /// 외부에서 QuestData를 전달받아, 슬롯 내부 UI를 채워 주는 메서드
    /// </summary>
    public void SetData(QuestData data)
    {
        if (data == null) return;

        // 1) 의뢰인 이미지
        if (questCharacterImg != null && data.characterSprite != null)
        {
            questCharacterImg.sprite = data.characterSprite;
            questCharacterImg.preserveAspect = true;
        }

        // 2) 의뢰인(기사) 이름
        if (GuestText != null)
        {
            GuestText.text = data.questName;
        }

        // 3) 의뢰 설명
        if (descriptiontext != null)
        {
            descriptiontext.text = data.description;
        }

        // 4) 보상 텍스트
        if (rewordtext != null)
        {
            rewordtext.text = data.rewardText;
        }
    }
}
