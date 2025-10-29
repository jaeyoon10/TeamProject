using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("손님 타입 목록 (거지/기본/부유층 등)")]
    public CustomerType[] customerTypes;

    [Header("생성 위치")]
    public Transform spawnPoint;
    

    private CraftingUI craftingUI;
    private Transform exitPoint;
    public Transform targetDoor;

    public bool IsReady { get; private set; } = false;

    IEnumerator Start()
    {
        // CraftingUI가 로드될 때까지 대기
        yield return new WaitUntil(() => FindObjectOfType<CraftingUI>() != null);
        craftingUI = FindObjectOfType<CraftingUI>();

        // ExitPoint도 씬에서 찾아서
        var go = GameObject.Find("CustomerExitPoint");
        if (go != null) exitPoint = go.transform;
        else Debug.LogError("CustomerExitPoint가 씬에 없습니다!");

        // ExitPoint도 씬에서 찾아서
        var doorgo = GameObject.Find("CustomerDoorObject");
        if (doorgo != null) targetDoor = doorgo.transform;
        else Debug.LogError("CustomerDoorObject 씬에 없습니다!");


        bool hasValidType = false;

        if (customerTypes != null)
        {
            for (int i = 0; i < customerTypes.Length; i++)
            {
                var t = customerTypes[i];
                Debug.Log($"[TypeChk] idx={i}, name={(t ? t.displayName : "NULL")}, " +
                          $"weight={(t ? t.spawnWeight : -1)}, " +
                          $"prefab={(t && t.customerPrefab ? t.customerPrefab.name : "NULL")}");
            }
        }

        if (customerTypes != null)
        {
            foreach (var t in customerTypes)
            {
                if (t == null) continue;
                if (t.spawnWeight > 0 && t.customerPrefab != null)
                {
                    hasValidType = true;
                    break;
                }
            }
        }
        else
        {
            Debug.LogError("customerTypes 배열이 비어있습니다.");
        }

        IsReady = (spawnPoint && craftingUI && exitPoint && targetDoor && hasValidType);
        Debug.Log(
                $"[Spawner Check] spawnPoint={(spawnPoint ? "OK" : "NULL")}, " +
                $"craftingUI={(craftingUI ? "OK" : "NULL")}, " +
                $"exitPoint={(exitPoint ? "OK" : "NULL")}, " +
                $"targetDoor={(targetDoor ? "OK" : "NULL")}, " +
                $"hasValidType={(customerTypes != null && customerTypes.Length > 0 ? "OK" : "FALSE")}"
                 );
        if (!IsReady)
        {
            Debug.LogError("CustomerSpawner 준비 실패: 필드/타입 설정을 확인하세요.");
        }
    }

    public IEnumerator SpawnWhenReady()
    {
        yield return new WaitUntil(() => IsReady);
        SpawnCustomer();
    }

    public void SpawnCustomer()
    {
        if (!IsReady)
        {
            Debug.LogWarning("SpawnCustomer 호출됐지만 IsReady=false");
            return;
        }

        if (!IsReady)
        {
            Debug.LogWarning("SpawnCustomer 호출됐지만 IsReady=false");
            return;
        }

        // 1) 타입 가중치 랜덤 선택
        var chosenType = PickRandomType();
        if (chosenType == null)
        {
            Debug.LogError("선택된 CustomerType이 없습니다. 설정을 확인하세요.");
            return;
        }

        // 2) 해당 타입의 프리팹 인스턴스화
        var prefab = chosenType.customerPrefab;
        if (prefab == null)
        {
            Debug.LogError($"선택된 타입({chosenType.displayName})에 프리팹이 없습니다.");
            return;
        }

        var go = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        // 3) 컨트롤러 세팅
        var ctrl = go.GetComponent<CustomerController>();
        if (ctrl == null)
        {
            Debug.LogError("스폰된 프리팹에 CustomerController가 없습니다.");
            return;
        }

        ctrl.targetDoor = targetDoor;
        ctrl.exitPoint = exitPoint;

        // ★ 2단계에서 타입 전달만 해둠 (지불 보정 적용은 3단계에서 CalculatePayment에 반영)
        //    (이미 CustomerController에 `public CustomerType type;` 필드를 추가해두었다는 전제)
        ctrl.GetType().GetField("type")?.SetValue(ctrl, chosenType);
        // ↑ 만약 CustomerController에 public CustomerType type; 을 이미 넣었다면,
        //    위의 리플렉션 대신 아래 한 줄로 교체하세요:
        // ctrl.type = chosenType;

        // 4) 요청 레시피 하나 골라서 상호작용 시작
        ctrl.BeginInteraction(
            craftingUI.allRecipes[
                Random.Range(0, craftingUI.allRecipes.Length)
            ]
        );
    }

    private CustomerType PickRandomType()
    {
        if (customerTypes == null || customerTypes.Length == 0) return null;

        int totalWeight = 0;
        foreach (var t in customerTypes)
        {
            if (t == null || t.customerPrefab == null) continue;
            int w = Mathf.Max(0, t.spawnWeight);
            totalWeight += w;
        }
        if (totalWeight <= 0) return null;

        int r = Random.Range(0, totalWeight);
        foreach (var t in customerTypes)
        {
            if (t == null || t.customerPrefab == null) continue;
            int w = Mathf.Max(0, t.spawnWeight);
            if (r < w) return t;
            r -= w;
        }
        // fallback
        foreach (var t in customerTypes)
        {
            if (t != null && t.customerPrefab != null) return t;
        }
        return null;
    }
}