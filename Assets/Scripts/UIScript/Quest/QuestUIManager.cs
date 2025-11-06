using System.Collections.Generic;
using UnityEngine;

public class QuestUIManager : MonoBehaviour
{
    [Header("슬롯 프리팹 & 컨테이너")]
    public GameObject questSlotPrefab;
    public Transform contentParent;

    [Header("데이터 로드 옵션")]
    public bool loadFromResources = true;

    public List<QuestData> questChain = new List<QuestData>();

    public int activeCount = 2; // 스샷처럼 2줄도 가능

    List<GameObject> _currentSlots = new();
    int _nextIndexToOpen = 0; // 다음에 열릴 퀘스트 인덱스

    void Start()
    {

        if (loadFromResources)
            LoadQuestsFromResources();

        BuildActiveSlots();
    }

    void LoadQuestsFromResources()
    {
        questChain.Clear();
        var sos = Resources.LoadAll<QuestDataSO>("Quests");
        System.Array.Sort(sos, (a, b) => a.order.CompareTo(b.order));

        foreach (var so in sos)
        {
            var q = new QuestData
            {
                characterSprite = so.characterSprite,
                questName = so.questName,
                description = so.description,
                targetCount = so.targetCount,
                rewardExp = so.rewardExp,
                rewardGold = so.rewardGold
            };
            questChain.Add(q);
        }
    }

        void BuildActiveSlots()
    {
        ClearAllSlots();

        // 아직 열리지 않은 퀘스트에서 activeCount개까지 활성화
        int opened = 0;
        for (int i = _nextIndexToOpen; i < questChain.Count && opened < activeCount; i++)
        {
            var q = questChain[i];
            CreateQuestSlot(q);
            opened++;
        }
    }

    void CreateQuestSlot(QuestData data)
    {
        if (questSlotPrefab == null || contentParent == null || data == null) return;

        var go = Instantiate(questSlotPrefab, contentParent);
        var slot = go.GetComponent<QusetSlotController>();
        slot.SetData(data, OnClaimReward);
        _currentSlots.Add(go);
    }

    void ClearAllSlots()
    {
        foreach (var go in _currentSlots) if (go) Destroy(go);
        _currentSlots.Clear();
    }

    // 외부에서 진행도 업데이트용 (예: 전투 완료 시 호출)
    public void AddProgressTo(string questName, int amount = 1)
    {
        var q = questChain.Find(x => x.questName == questName);
        if (q == null) return;

        q.AddProgress(amount);
        RefreshAll();
    }

    void RefreshAll()
    {
        foreach (var go in _currentSlots)
            if (go && go.TryGetComponent<QusetSlotController>(out var slot))
                slot.Refresh();
    }

    // 보상 수령 시 호출: 다음 퀘스트를 열어준다(순차 진행)
    void OnClaimReward(QuestData claimed)
    {
        // 보상 지급: 실제 게임 매니저(골드/경험치 매니저)에 연결
        MoneyManager.Instance?.AddGold(claimed.rewardGold);
        CharacterInfoManager.Instance?.AddXP(claimed.rewardExp);

        CharacterInfoManager.Instance?.AddQuestProfit(claimed.rewardGold);

        // 체인에서 다음 슬롯 열기 로직
        int claimedIndex = questChain.IndexOf(claimed);
        if (claimedIndex == _nextIndexToOpen)
        {
            // 가장 앞쪽 활성 퀘스트를 수령했다면 개시 인덱스 +1
            _nextIndexToOpen = Mathf.Min(_nextIndexToOpen + 1, questChain.Count);
        }

        // UI 재구성(활성 라인 유지)
        BuildActiveSlots();
    }
}
