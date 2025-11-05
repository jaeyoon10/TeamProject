using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
public class CustomerController : MonoBehaviour
{

    [Header("손님 타입 데이터")]
    public CustomerType type;

    [Header("손님 애니메이터 & 위치")]
    public Animator animator;
    public Transform targetDoor;    // 도착할 위치
    public Transform exitPoint;     // 퇴장할 위치 (Inspector에 할당)

    private RecipeData demandRecipe;
    private bool isServed = false;

    private bool isPaused;
    public bool IsServed => isServed;

    /// <summary>
    /// 현재 이 손님이 요구 중인 레시피
    /// </summary>
    public RecipeData DemandRecipe => demandRecipe;

    /// <summary>
    /// 상호작용 시작 (스폰 직후)
    /// </summary>
    public void BeginInteraction(RecipeData recipe)
    {
        demandRecipe = recipe;

        // 걷기 애니메이션 + 이동
        animator.SetBool("isMoving", true);
        Debug.Log($"Animator connected? {animator != null}");
        StartCoroutine(WalkToDoor());
    }

    IEnumerator WalkToDoor()
    {

        if (ModalController.IsOpen)
            yield return new WaitUntil(() => !ModalController.IsOpen);

        transform.LookAt(targetDoor.position);

        while (Vector3.Distance(transform.position, targetDoor.position) > 0.1f)
        {
            //모달 열리면 즉시 멈추고, 닫힐 때까지 대기 후 재개
            if (ModalController.IsOpen)
            {
                animator.SetBool("isMoving", false);
                yield return new WaitUntil(() => !ModalController.IsOpen);
                animator.SetBool("isMoving", true);
            }

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

        yield return new WaitForSeconds(1f);
        ShowRequestDialog();
    }
    void ShowRequestDialog()
    {
        int playerLevel = CharacterInfoManager.Instance.CurrentLevel;

        List<RecipeData> available = new List<RecipeData>();
        foreach (var recipe in CraftingUI.Instance.allRecipes)
        {
            if (recipe.requiredLevel <= playerLevel)
                available.Add(recipe);
        }
        if (available.Count == 0)
        {
            Debug.LogWarning("[Customer] 사용할 수 있는 레시피 없음");
            return;
        }

        demandRecipe = available[Random.Range(0, available.Count)];

        // 여기서 타입 전달
        DialogUIManager.Instance.ShowDialog(demandRecipe, type);
        DialogQuestUI.Instance.SetQuestText($"{demandRecipe.weaponName}을(를) 만들어 주세요");
    }


    /// <summary>
    /// 플레이어가 무기 전달했을 때 호출
    /// </summary>
    public void ServeWeapon(RecipeData craftedRecipe, int qualityScore)
    {
        if (isServed) return;
        isServed = true;

        DialogUIManager.Instance.HideDialog();
        DialogQuestUI.Instance.ClearQuest();

        if (craftedRecipe != demandRecipe)
        {
            //실패 처리
            var moneyMgr = FindObjectOfType<MoneyManager>();
            if (moneyMgr != null)
                moneyMgr.AddGold(-200);
            CharacterInfoManager.Instance.AddProductionProfit(-200);

            CharacterInfoManager.Instance.AddStressPoint();
            CharacterInfoManager.Instance.AddStressPoint();

            DialogUIManager.Instance.SetAfterClose(() => { StartCoroutine(Depart()); });

            // 보상 다이얼로그 띄우기
            DialogUIManager.Instance.ShowRewardDialog(
                recipe: demandRecipe,     // 손님이 요구했던 무기
                customerType: type,
                gold: -200,
                exp: 0,
                stressDelta: +2
            );
            return;
        }
        // 별 개수 계산
        int star = WeaponCraftingManager.Instance.CalcStar(qualityScore);

        // 1) 지불액 계산 & 추가
        int payment = CalculatePayment(demandRecipe.basePrice, star);
        var mgr = FindObjectOfType<MoneyManager>();
        if (mgr != null)
            mgr.AddGold(payment);
        CharacterInfoManager.Instance.AddProductionProfit(payment); ;

        // 3) 경험치 계산
        int xpGain = CalculateXP(demandRecipe.xpReward, star);
        CharacterInfoManager.Instance.AddXP(xpGain);

        Debug.Log($"[Customer] Payment={payment}, XP Gained={xpGain}");


        int stressGain = Random.Range(1, 3);
        for (int i = 0; i < stressGain; i++)
            CharacterInfoManager.Instance.AddStressPoint();

        /// 닫히면 퇴장하도록 예약
        DialogUIManager.Instance.SetAfterClose(() => { StartCoroutine(Depart()); });

        // 보상 다이얼로그 띄우기
        DialogUIManager.Instance.ShowRewardDialog(
            recipe: demandRecipe,
            customerType: type,
            gold: payment,
            exp: xpGain,
            stressDelta: +stressGain
        );
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
        // 1) 품질(별점)까지 반영한 기본 금액
        int price = basePrice + bonus;

        // 2) 손님 타입 보정 (없으면 기본 1.0 / +0 으로 처리)
        float mult = (type != null) ? type.paymentMultiplier : 1f;

        price = Mathf.RoundToInt(price * mult);

        if (EnchantSession.IsActive)
            price = Mathf.RoundToInt(price * EnchantSession.GetPayMultiplier());

        // 3) 최소 100 보장
        price = Mathf.Max(100, price);

        Debug.Log($"[Customer] Pay Calc → base={basePrice}, star={star}, afterStar={basePrice + bonus}, " +
                  $"type={(type ? type.displayName : "None")}, mult={mult}, final={price}");

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

    private void OnEnable()
    {
        ModalController.OnChanged += HandleModal;
    }

    private void OnDisable()
    {
        ModalController.OnChanged -= HandleModal;
    }

    private void HandleModal(bool open)
    {
        isPaused = open;
        animator.SetBool("isMoving", !isPaused);
    }

    IEnumerator Depart()
    {
        if (ModalController.IsOpen)
            yield return new WaitUntil(() => !ModalController.IsOpen);

        // 잠시 대기
        yield return new WaitForSeconds(1f);

        transform.LookAt(exitPoint.position);

        // 퇴장 애니 + 이동
        animator.SetBool("isMoving", true);
        while (Vector3.Distance(transform.position, exitPoint.position) > 0.1f)
        {
            // 팝업이 열려있으면 그동안 멈춤
            if (isPaused)
            {
                yield return null; // 다음 프레임까지 대기
                continue;
            }

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