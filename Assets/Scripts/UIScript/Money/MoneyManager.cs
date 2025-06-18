using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static MoneyManager Instance { get; private set; }

    [Header("초기 설정")]
    [Tooltip("게임 시작 시 플레이어가 가진 초기 골드량")]
    public int startingGold = 100;

    [Header("UI 연결")]
    [Tooltip("돈을 출력할 TextMeshPro Text (예: '0')")]
    public TMP_Text moneyText;

    private int currentGold;  // 실제 보유 골드

    private void Awake()
    {
        // 싱글톤 패턴: 이미 존재하면 Destroy
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
       

        // 초기 골드 설정
        currentGold = startingGold;

        // UI 초기화
        UpdateMoneyUI();
    }

    /// <summary>
    /// 현재 보유 골드를 반환
    /// </summary>
    public int GetGold()
    {
        return currentGold;
    }

    /// <summary>
    /// <para>골드를 추가(획득)한다.</para>
    /// 상점에서 구매한 뒤 반환된 돈이거나, 고객에게 팔아서 얻은 금액 등을 이 메서드로 호출.
    /// </summary>
    /// <param name="amount">추가할 금액 (양수)</param>
    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        currentGold += amount;
        UpdateMoneyUI();
    }

    /// <summary>
    /// <para>골드를 소비(차감)한다.</para>
    /// 상점에서 재료를 살 때 이 메서드 호출.  
    /// 잔액이 부족하면 false를 반환하며, 소비되지 않는다.
    /// </summary>
    /// <param name="amount">차감할 금액 (양수)</param>
    /// <returns>소비 성공 시 true, 부족 시 false</returns>
    public bool SpendGold(int amount)
    {
        if (amount <= 0) return false;
        if (currentGold < amount)
        {
            // 돈 부족
            Debug.Log("MoneyManager: 골드가 부족합니다.");
            return false;
        }

        currentGold -= amount;
        UpdateMoneyUI();
        return true;
    }

    /// <summary>
    /// moneyText를 “Gold: {currentGold}” 형태로 갱신
    /// </summary>
    private void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = $"{currentGold}";
        }
    }

    #region 테스트용 ContextMenu
    [ContextMenu("테스트: 골드 +10000")]
    public void TestAdd10000()
    {
        AddGold(10000);
        Debug.Log($"테스트: 골드 +10000 → 현재 골드 = {currentGold}");
    }

    [ContextMenu("테스트: 골드 -30")]
    public void TestSpend30()
    {
        bool ok = SpendGold(30);
        Debug.Log(ok
            ? $"테스트: 골드 -30 성공 → 현재 골드 = {currentGold}"
            : $"테스트: 골드 -30 실패 (잔액 부족: {currentGold}남음)");
    }
    #endregion
}