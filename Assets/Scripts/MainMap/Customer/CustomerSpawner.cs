using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("손님 프리팹 & 생성 위치")]
    public GameObject customerPrefab;
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

        if (!customerPrefab) Debug.LogError("[CustomerSpawner] customerPrefab 미지정!");
        if (!spawnPoint) Debug.LogError("[CustomerSpawner] spawnPoint 미지정!");
        if (!craftingUI) Debug.LogError("[CustomerSpawner] craftingUI 미지정!");
        if (!exitPoint) Debug.LogError("[CustomerSpawner] exitPoint 미지정!");
        if (!targetDoor) Debug.LogError("[CustomerSpawner] targetDoor 미지정!");

        IsReady = (customerPrefab && spawnPoint && craftingUI && exitPoint && targetDoor);
    }

    public IEnumerator SpawnWhenReady()
    {
        yield return new WaitUntil(() => IsReady);
        SpawnCustomer();
    }

    public void SpawnCustomer()
    {
        

        var go = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
        var ctrl = go.GetComponent<CustomerController>();
        if (ctrl == null) 
        {
            Debug.LogError("CustomerPrefab에 CustomerController가 없습니다.");
            return;
        }


        // **CustomerController에 방금 찾은 targetDoor/exitPoint 넘겨주기**
        ctrl.targetDoor = targetDoor;
        ctrl.exitPoint = exitPoint;

        ctrl.BeginInteraction(
          craftingUI.allRecipes[
            Random.Range(0, craftingUI.allRecipes.Length)
          ]
        );
    }
}