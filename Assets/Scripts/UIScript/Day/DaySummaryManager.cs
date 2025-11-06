using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DaySummaryManager : MonoBehaviour
{
    [Header("=== Day Summary 전체 패널 ===")]
    public GameObject daySummaryPanel;   // DaySummaryPanel GameObject (처음엔 비활성화)

    [Header("=== 상단: N일째 ===")]
    public TMP_Text dayText;             // "1일째", "2일째" 등

    [Header("=== 제작 Row ===")]
    public Image 제작_Background;        // Row 배경 이미지 (단색)
    public TMP_Text 제작_CategoryText;   // “제작” (Inspector에서 미리 "제작"으로 설정)
    public TMP_Text 제작_AmountText;     // "+xxxxG" / "-xxxxG"

    [Header("=== 의뢰 Row ===")]
    public Image 의뢰_Background;
    public TMP_Text 의뢰_CategoryText;   // “의뢰”
    public TMP_Text 의뢰_AmountText;

    [Header("=== 상점 Row ===")]
    public Image 상점_Background;
    public TMP_Text 상점_CategoryText;   // “상점”
    public TMP_Text 상점_AmountText;

    [Header("=== 강화 Row ===")]
    public Image 강화_Background;
    public TMP_Text 강화_CategoryText;   // “강화”
    public TMP_Text 강화_AmountText;

    [Header("=== 총합 Row ===")]
    public Image 총합_Background;
    public TMP_Text 총합_CategoryText;   // “총합”
    public TMP_Text 총합_AmountText;

    [Header("=== Confirm 버튼 ===")]
    public Button confirmButton;         // “확인” 버튼

    // 스트레스 관리 및 누적 초기화를 위해 CharacterInfoManager 참조
    public CharacterInfoManager characterInfoManager;

    public StoreSlotLoader storeSlotLoader;

    private void Awake()
    {
        // DaySummaryPanel은 처음에 비활성화
        if (daySummaryPanel != null)
            daySummaryPanel.SetActive(false);

        // Confirm 버튼 이벤트 연결
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
    }

    /// <summary>
    /// 스트레스가 MAX가 되었을 때 CharacterInfoManager에서 호출합니다.
    /// </summary>
    /// <param name="profit제작">제작(판매) 수익 (양수)</param>
    /// <param name="profit의뢰">퀘스트 보상 수익 (양수)</param>
    /// <param name="cost상점">상점 구매 비용 (양수 → 내부 음수 처리)</param>
    /// <param name="cost강화">강화 구매 비용 (양수 → 음수 처리)</param>
    public void ShowDaySummary(int profit제작, int profit의뢰, int cost상점, int cost강화)
    {
        // 1) “N일째” 텍스트 업데이트
        if (dayText != null)
            dayText.text = $"{characterInfoManager.CurrentDay}일째";

        // 2) 각 금액 텍스트 업데이트(양수=금색, 음수=빨간색)
        UpdateAmountText(제작_AmountText, profit제작);
        UpdateAmountText(의뢰_AmountText, profit의뢰);

        // “지출” 항목은 내부적으로 음수로 표시하고 싶으므로
        int minus상점 = -Mathf.Abs(cost상점);
        int minus강화 = -Mathf.Abs(cost강화);
        UpdateAmountText(상점_AmountText, minus상점);
        UpdateAmountText(강화_AmountText, minus강화);

        int total = profit제작 + profit의뢰 + minus상점 + minus강화;
        UpdateAmountText(총합_AmountText, total);

        // 3) DaySummaryPanel을 활성화하여 화면에 보이게 함
        if (daySummaryPanel != null)
            daySummaryPanel.SetActive(true);

        ModalController.Show();
    }

    /// <summary>
    /// 금액(amount)에 따라 텍스트를 “+숫자G” 혹은 “-숫자G”로 설정하고,
    /// 양수는 금색(#FFD700), 음수는 빨간색(#FF0000)으로 텍스트 색을 변경합니다.
    /// </summary>
    private void UpdateAmountText(TMP_Text amountText, int amount)
    {
        if (amountText == null) return;

        string sign = (amount >= 0) ? "+" : "-";
        int absVal = Mathf.Abs(amount);
        amountText.text = $"{sign}{absVal}G";

        if (amount >= 0)
            amountText.color = new Color32(255, 215, 0, 255); // 금색
        else
            amountText.color = new Color32(255, 0, 0, 255);   // 빨간색
    }

    /// <summary>
    /// “확인” 버튼 클릭 시 호출되는 메서드
    /// DaySummaryPanel을 닫고, 날짜를 증가시키며,
    /// QuestUIManager를 통해 “다음 날 의뢰”를 재생성하고,
    /// CharacterInfoManager에서 스트레스를 초기화합니다.
    /// </summary>
    private void OnConfirmButtonClicked()
    {
        // 1) DaySummaryPanel 비활성화
        if (daySummaryPanel != null)
            daySummaryPanel.SetActive(false);

        // 4) 스트레스 초기화 및 캐릭터 정보 초기화
        if (characterInfoManager != null)
        {
            characterInfoManager.ResetForNextDay();

            if (dayText != null)
                dayText.text = $"{characterInfoManager.CurrentDay}일째";
        }
        // 5) 상점 완전 리프레시
        if (storeSlotLoader != null)
        {
            storeSlotLoader.RefreshAll();
        }
        else
        {
            Debug.LogWarning("[DaySummaryManager] storeSlotLoader가 할당되지 않았습니다!");
        };

        ModalController.Hide();
    }
}