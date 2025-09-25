using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
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
        transform.LookAt(targetDoor.position);

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

        Debug.Log($"[Customer] DemandRecipe={demandRecipe.weaponName}, BasePrice={demandRecipe.basePrice}");
        // 말풍선 숨기기
        worldCanvas.gameObject.SetActive(false);

        // 별 개수 계산
        int star = WeaponCraftingManager.Instance.CalcStar(qualityScore);

        // 1) 지불액 계산 & 추가
        int payment = CalculatePayment(demandRecipe.basePrice, star);
        var moneyMgr = FindObjectOfType<MoneyManager>();
        if (moneyMgr != null)
            moneyMgr.AddGold(payment);
        CharacterInfoManager.Instance.AddProductionProfit(payment); ;

        // 3) 경험치 계산
        int xpGain = CalculateXP(demandRecipe.xpReward, star);
        CharacterInfoManager.Instance.AddXP(xpGain);

        Debug.Log($"[Customer] Payment={payment}, XP Gained={xpGain}");


        int stressGain = Random.Range(1, 3);
        for (int i = 0; i < stressGain; i++)
            CharacterInfoManager.Instance.AddStressPoint();

        // 3) 퇴장
        StartCoroutine(Depart());
    }

    int CalculatePayment(int basePrice, int star)
    {
        int bonus = 0;
        switch (star)
        {
            case 5: bonus = +200; break;
            case 4: bonus = 0; break;
            case 3: bonus = -200; break;
            case 2: bonus = -500; break;
            default: bonus = -900; break;
        }
        int price = Mathf.Max(100, basePrice + bonus);
        Debug.Log($"[Customer] XP Calc → base={basePrice}, star={star}, price={price}");
        return price;
    }

    private int CalculateXP(int xpReward, int star)
    {
        int bonus = 0;
        switch (star)
        {
            case 5: bonus = +10; break;   // 별 5 → 보너스
            case 4: bonus = 0; break;     // 별 4 → 기본 그대로
            case 3: bonus = -5; break;    // 별 3 → 약간 감소
            case 2: bonus = -10; break;   // 별 2 → 큰 감소
            default: bonus = -20; break;  // 별 1 이하 → 심각한 감소
        }

        int result = Mathf.Max(10, xpReward + bonus); // 최소 10 보장
        Debug.Log($"[Customer] XP Calc → base={xpReward}, star={star}, result={result}");
        return result;
    }

    IEnumerator Depart()
    {
        // 잠시 대기
        yield return new WaitForSeconds(1f);

        transform.LookAt(exitPoint.position);

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