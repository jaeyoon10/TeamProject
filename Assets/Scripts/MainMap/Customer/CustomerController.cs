using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CustomerController : MonoBehaviour
{
    [Header("손님 애니메이터 & 위치")]
    public Animator animator;
    public Transform targetDoor;    // 도착할 위치
    public Transform exitPoint;     // 퇴장할 위치 (Inspector에 할당)

    [Header("요구 말풍선 UI")]
    public Canvas worldCanvas;
    public Image demandIcon;

    private RecipeData demandRecipe;
    private bool isServed = false;

    public bool IsServed => isServed;
    // ← 여기를 추가
    /// <summary>현재 이 손님이 요구 중인 레시피</summary>
    public RecipeData DemandRecipe => demandRecipe;

    /// <summary>
    /// 상호작용 시작 (스폰 직후)
    /// </summary>
    public void BeginInteraction(RecipeData recipe)
    {
        demandRecipe = recipe;

        // 말풍선 세팅
        demandIcon.sprite = recipe.icon;
        worldCanvas.gameObject.SetActive(true);

        // 걷기 애니메이션 + 이동
        animator.SetBool("isMoving", true);
        StartCoroutine(WalkToDoor());
    }

    IEnumerator WalkToDoor()
    {
        while (Vector3.Distance(transform.position, targetDoor.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetDoor.position,
                Time.deltaTime * 2f
            );
            yield return null;
        }

        animator.SetBool("isMoving", false);
        // 도착 알림
        WeaponCraftingManager.Instance.OnCustomerArrived(this);
    }

    /// <summary>
    /// 플레이어가 무기 전달했을 때 호출
    /// </summary>
    public void ServeWeapon(int qualityScore)
    {
        if (isServed) return;
        isServed = true;

        // 말풍선 숨기기
        worldCanvas.gameObject.SetActive(false);

        // 1) 지불액 계산 & 추가
        int payment = CalculatePayment(qualityScore);
        var moneyMgr = FindObjectOfType<MoneyManager>();
        if (moneyMgr != null)
            moneyMgr.AddGold(payment);
        CharacterInfoManager.Instance.AddProductionProfit(payment);

        // 2) XP 계산 & 추가 (골드 비율과 동일하게)
        int xpGain = CalculateXP(demandRecipe.baseXP, qualityScore);
        CharacterInfoManager.Instance.AddXP(xpGain);

        Debug.Log($"[Customer] XP Gained: {xpGain}");


        int stressGain = Random.Range(6, 8);
        for (int i = 0; i < stressGain; i++)
            CharacterInfoManager.Instance.AddStressPoint();
        // 3) 퇴장
        StartCoroutine(Depart());
    }

    int CalculatePayment(int score)
    {
        if (score >= 100) return 1000;
        else if (score >= 80) return 800;
        else if (score >= 50) return 500;
        else if (score >= 30) return 300;
        else if (score >= 10) return 100;
        else return 50;
    }

    private int CalculateXP(int baseXP, int score)
    {
        float ratio;
        if (score >= 100) ratio = 5.0f;    // 10 경치
        else if (score >= 80) ratio = 3.0f; //  80%
        else if (score >= 50) ratio = 1.5f; //  50%
        else if (score >= 30) ratio = 1f; //  30%
        else if (score >= 10) ratio = 0.5f; //  10%
        else ratio = 0.1f;                //   5%

        return Mathf.RoundToInt(baseXP * ratio);
    }

    int CalculateXP(RecipeData recipe, int score)
    {
        // RecipeData에 추가한 baseXP 필드를 사용
        float ratio = score / 100f;
        return Mathf.RoundToInt(recipe.baseXP * ratio);
    }

    IEnumerator Depart()
    {
        // 잠시 대기
        yield return new WaitForSeconds(1f);

        // 퇴장 애니 + 이동
        animator.SetBool("isMoving", true);
        while (Vector3.Distance(transform.position, exitPoint.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                exitPoint.position,
                Time.deltaTime * 2f
            );
            yield return null;
        }
        WeaponCraftingManager.Instance.customerSpawner.SpawnCustomer();

        Destroy(gameObject);
    }
}