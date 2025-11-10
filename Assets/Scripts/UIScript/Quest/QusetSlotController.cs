using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class QusetSlotController : MonoBehaviour
{
    [Header("UI 참조")]
    public Image questCharacterImg;
    public TMP_Text titleText;
    public TMP_Text descriptiontext;

    [Header("진행도 UI")]
    public Image progressFill;
    public TMP_Text progressLabel;

    [Header("보상 UI")]
    public Transform rewardContainer; // 여기에 프리팹 넣을 거임
    public GameObject rewardPrefab;   // x1000 골드 같은 UI 프리팹

    [Header("보상 아이콘")]
    public Sprite goldIcon;
    public Sprite expIcon;


    [Header("보상 버튼/상태")]
    public Button claimButton;

    private QuestData _data;
    private System.Action<QuestData> _onClaim;

    public void SetData(QuestData data, System.Action<QuestData> onClaim = null)
    {
        _data = data;
        _onClaim = onClaim;

        Refresh();
        WireEvents(true);
    }

    void OnDestroy() => WireEvents(false);

    void WireEvents(bool add)
    {
        if (claimButton == null) return;
        claimButton.onClick.RemoveAllListeners();
        if (add) claimButton.onClick.AddListener(HandleClaim);
    }

    public void Refresh()
    {
        if (_data == null) return;

        if (titleText) titleText.text = _data.questName;
        if (descriptiontext) descriptiontext.text = _data.description;

        if (progressFill)
        {
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.fillOrigin = 0; // Left
            progressFill.fillAmount = _data.Progress01; // current/target
        }

        if (progressLabel) progressLabel.text = $"{_data.currentCount}/{_data.targetCount}";

        if (claimButton) claimButton.interactable = _data.IsCompleted;

        //보상 아이콘 세팅 부분
        UpdateRewardsUI();
    }

    void UpdateRewardsUI()
    {
        if (rewardContainer == null || rewardPrefab == null) return;

        // 기존 보상 UI 비우기
        for (int i = rewardContainer.childCount - 1; i >= 0; i--)
            Destroy(rewardContainer.GetChild(i).gameObject);

        // 골드 보상
        if (_data.rewardGold > 0)
        {
            GameObject go = Instantiate(rewardPrefab, rewardContainer);
            go.transform.Find("AmountText").GetComponent<TMP_Text>().text = "x" + _data.rewardGold;
            go.transform.Find("Icon").GetComponent<Image>().sprite = goldIcon;
        }

        // 경험치 보상
        if (_data.rewardExp > 0)
        {
            GameObject go = Instantiate(rewardPrefab, rewardContainer);
            go.transform.Find("AmountText").GetComponent<TMP_Text>().text = "x" + _data.rewardExp;
            go.transform.Find("Icon").GetComponent<Image>().sprite = expIcon;
        }
    }


    void HandleClaim()
    {
        if (_data == null || !_data.IsCompleted) return;

        _onClaim?.Invoke(_data);

        // 슬롯 제거 (수령 완료 표시 대신 삭제)
        Destroy(gameObject);
    }
}
