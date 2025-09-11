using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestUIManager : MonoBehaviour
{
    [Header("의뢰 슬롯 프리팹")]
    public GameObject questSlotPrefab;

    [Header("Content Transform")]
    public Transform contentParent;

    [Header("가능한 모든 의뢰 데이터 목록 (코드에서 채워질 예정)")]
    public List<QuestData> possibleQuests = new List<QuestData>();

    [Header("하루에 최대 생성할 의뢰 개수")]
    public int maxQuestsPerDay = 2;

    private List<GameObject> currentSlots = new List<GameObject>();


    void Start()
    {
        // 만약 Inspector에서 possibleQuests를 채우지 않았다면, 코드로 몇 가지 퀘스트를 추가
        if (possibleQuests.Count == 0)
        {
            PopulateQuestsByCode();
        }

        // 의뢰 창을 열 때(혹은 Start 직후) 하루치 퀘스트를 생성
        GenerateDailyQuests();
    }

    private void PopulateQuestsByCode()
    {
        // 1) 첫 번째 퀘스트: "기술자 모집"
        QuestData q1 = new QuestData();
        q1.questName = "기술자 모집";
        q1.description = "마을의 대장장이가 금속을 남쪽 광산에서 공수해 달라고 합니다.";
        q1.rewardText = "경험치 50xp, 금화 100G";
        // Resources/QuestSprites/Smith.png 라는 경로에 스프라이트가 있다고 가정
        q1.characterSprite = Resources.Load<Sprite>("QuestSprite/123");
        possibleQuests.Add(q1);

        // 2) 두 번째 퀘스트: "전장의 부상병 구호"
        QuestData q2 = new QuestData();
        q2.questName = "부상병 구호";
        q2.description = "전쟁터 부상병에게 물약을 전달해 주세요.";
        q2.rewardText = "경험치 80xp, 금화 150G";
        q2.characterSprite = Resources.Load<Sprite>("QuestSprite/432");
        possibleQuests.Add(q2);

        // 3) 세 번째 퀘스트: "음식 배달"
        QuestData q3 = new QuestData();
        q3.questName = "음식 배달";
        q3.description = "마을 남쪽에서 상점까지 빵과 과일을 배달해 주세요.";
        q3.rewardText = "경험치 30xp, 금화 50G";
        q3.characterSprite = Resources.Load<Sprite>("QuestSprite/12323");
        possibleQuests.Add(q3);
    }

    public void GenerateDailyQuests()
    {
        ClearAllSlots();

        int count = Random.Range(0, maxQuestsPerDay + 1);

        List<int> indices = new List<int>();
        for (int i = 0; i < possibleQuests.Count; i++)
        {
            indices.Add(i);
        }
        for (int i = 0; i < indices.Count; i++)
        {
            int rand = Random.Range(i, indices.Count);
            int tmp = indices[i];
            indices[i] = indices[rand];
            indices[rand] = tmp;
        }

        for (int i = 0; i < count && i < indices.Count; i++)
        {
            int questIdx = indices[i];
            QuestData qd = possibleQuests[questIdx];
            CreateQuestSlot(qd);
        }
    }

    private void CreateQuestSlot(QuestData data)
    {
        if (questSlotPrefab == null || contentParent == null || data == null)
        {
            return;
        }
        GameObject go = Instantiate(questSlotPrefab, contentParent);

        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localPosition = Vector3.one;
            rt.anchoredPosition3D = Vector3.zero;
        }
        QusetSlotController slotCtrl = go.GetComponent<QusetSlotController>();
        if (slotCtrl != null)
        {
            slotCtrl.SetData(data);
        }

        // 4) 생성된 슬롯을 리스트에 추가
        currentSlots.Add(go);
    }

    private void ClearAllSlots()
    {
        foreach (var go in currentSlots)
        {
            if (go != null)
            {
                Destroy(go);
            }
        }
        currentSlots.Clear();
    }

}

