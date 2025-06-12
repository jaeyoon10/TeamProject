using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterInfoManager : MonoBehaviour
{
    [Header("=== 캐릭터 초상화 ===")]
    public Image characterPortrait;       // PortraitFrame > CharacterPortrait
    public bool preserveAspect = true;

    [Header("=== 레벨 & 경험치 ===")]
    public TMP_Text levelText;            // "LV.1" 표시
    public Slider xpBar;                  // Slider (Min/Max는 코드에서 설정)
    public int currentLevel = 1;          // 초기 레벨 (LV.1)
    private int currentXP = 0;            // 현재 경험치 (게이지)
    private int xpPerLevel;               // 현재 레벨에서 다음 레벨까지 필요한 XP

    [Header("Level Up Effect")]
    public CanvasGroup levelUpCanvasGroup; // LevelUpEffect > LevelUpText의 CanvasGroup
    public float levelUpFadeDuration = 0.5f;

    [Header("=== 스트레스 바 ===")]
    public List<Image> stressSegments = new List<Image>(); // 스트레스 세그먼트 이미지 리스트
    public Color stressEmptyColor = new Color(1f, 0.92f, 0.02f, 1f);  // 노란색
    public Color stressFillColor = Color.red;                         // 빨간색
    private int currentStress = 0;         // 현재 스트레스 포인트
    private int maxStress;                 // stressSegments.Count

    [Header("Face Sprites")]
    public Sprite faceNormal;   // 스트레스 0
    public Sprite faceHalf;     // 절반 찰 때
    public Sprite faceMax;      // 가득 찰 때
    public Image faceImage;     // FaceImage 컴포넌트

    [Header("=== 날짜 관리 ===")]
    public TMP_Text dayText;     // "Day 1" 표시
    private int currentDay = 1;  // 초기 1일

    [Header("=== 일일 수익/지출 누적 변수 ===")]
    [Tooltip("하루 동안 무기 제작(판매)으로 벌어들인 금액(양수)")]
    private int dailyProfit제작 = 0;

    [Tooltip("퀘스트 완료 보상으로 받은 금액(양수)")]
    private int dailyProfit의뢰 = 0;

    [Tooltip("상점에서 재료를 구매하거나 판매할 때 지출된 금액(양수)")]
    private int dailyCost상점 = 0;

    [Tooltip("강화 재료 구매 등에 지출된 금액(양수)")]
    private int dailyCost강화 = 0;

    [Header("=== 의뢰 UI 매니저 참조 ===")]
    [Tooltip("씬에 배치된 QuestUIManager를 드래그 연결하세요.")]
    public QuestUIManager questUIManager;

    [Header("=== DaySummary 매니저 참조 ===")]
    [Tooltip("씬에 배치된 DaySummaryManager를 드래그 연결하세요.")]
    public DaySummaryManager daySummaryManager;

    private void Awake()
    {
        // 1) 경험치 세팅
        xpPerLevel = CalculateXPForNextLevel(currentLevel);
        xpBar.minValue = 0;
        xpBar.maxValue = xpPerLevel;
        xpBar.value = currentXP;

        // 2) 레벨 텍스트 초기화
        UpdateLevelText();

        // 3) 레벨업 이펙트 숨김
        if (levelUpCanvasGroup != null)
            levelUpCanvasGroup.alpha = 0;

        // 4) 스트레스 세그먼트 초기화(모두 empty 색)
        maxStress = stressSegments.Count;
        for (int i = 0; i < maxStress; i++)
        {
            stressSegments[i].color = stressEmptyColor;
        }

        // 5) 표정 초기화
        if (faceImage != null && faceNormal != null)
            faceImage.sprite = faceNormal;

        // 6) 날짜 초기화
        UpdateDayText();
    }

    #region ======== 캐릭터 초상화 ========
    public void SetCharacterPortrait(Sprite newPortrait)
    {
        if (characterPortrait == null || newPortrait == null) return;
        characterPortrait.sprite = newPortrait;
        characterPortrait.preserveAspect = preserveAspect;
    }
    #endregion

    #region ======== 경험치(EXP) & 레벨 ========
    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        currentXP += amount;

        if (currentXP >= xpPerLevel)
        {
            // 레벨업: 남은 XP는 버리고 게이지 초기화
            currentXP = 0;
            LevelUpRoutine();
            xpPerLevel = CalculateXPForNextLevel(currentLevel);
        }

        xpBar.maxValue = xpPerLevel;
        xpBar.value = currentXP;
    }

    private void LevelUpRoutine()
    {
        currentLevel++;
        UpdateLevelText();
        StartCoroutine(PlayLevelUpEffect());
    }

    private void UpdateLevelText()
    {
        if (levelText != null)
            levelText.text = $"LV.{currentLevel}";
    }

    private IEnumerator PlayLevelUpEffect()
    {
        if (levelUpCanvasGroup == null)
            yield break;

        // Fade In
        float t = 0f;
        while (t < levelUpFadeDuration)
        {
            t += Time.deltaTime;
            levelUpCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / levelUpFadeDuration);
            yield return null;
        }

        // 잠시 대기
        yield return new WaitForSeconds(1f);

        // Fade Out
        t = 0f;
        while (t < levelUpFadeDuration)
        {
            t += Time.deltaTime;
            levelUpCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / levelUpFadeDuration);
            yield return null;
        }

        levelUpCanvasGroup.alpha = 0f;
    }

    private int CalculateXPForNextLevel(int level)
    {
        int baseXP = 100;
        if (level == 1)
            return baseXP;

        int xpNeeded = baseXP;
        for (int lv = 1; lv < level; lv++)
        {
            if (lv % 2 == 1)
                xpNeeded += 30;
            else
                xpNeeded += 20;
        }
        return xpNeeded;
    }
    #endregion

    #region ======== 스트레스 & 날짜 ========
    public void AddStressPoint()
    {
        if (currentStress >= maxStress)
            return;

        // 1) 스트레스 1포인트 추가 (fill 색으로 변경)
        stressSegments[currentStress].color = stressFillColor;
        currentStress++;

        // 2) 표정 업데이트
        UpdateFaceSprite();

        // 3) 만약 스트레스가 max에 도달했다면 하루 종료
        if (currentStress >= maxStress)
        {
            StartCoroutine(OnStressMaxRoutine());
        }
    }

    private void UpdateFaceSprite()
    {
        if (faceImage == null) return;

        if (currentStress < maxStress * 0.3f)
        {
            faceImage.sprite = faceNormal;
        }
        else if (currentStress < maxStress * 0.7f)
        {
            faceImage.sprite = faceHalf;
        }
        else
        {
            faceImage.sprite = faceMax;
        }
    }

    private IEnumerator OnStressMaxRoutine()
    {
        // 스트레스 MAX 알림(1초 대기)
        yield return new WaitForSeconds(1f);

        Debug.Log($"[StressMaxRoutine] 호출됨 → 제작={dailyProfit제작}, 의뢰={dailyProfit의뢰}, 상점={dailyCost상점}, 강화={dailyCost강화}");
        // 1) 하루 동안 누적된 수익/지출을 DaySummaryManager에 전달
        if (daySummaryManager != null)
        {
            daySummaryManager.ShowDaySummary(
                profit제작: dailyProfit제작,
                profit의뢰: dailyProfit의뢰,
                cost상점: dailyCost상점,
                cost강화: dailyCost강화
            );
        }
        else
        {
            Debug.LogWarning("[CharacterInfoManager] daySummaryManager가 할당되지 않음");
        }

        // (스트레스는 Confirm 버튼 클릭 시 초기화할 예정)

        yield break;
    }

    private void UpdateDayText()
    {
        if (dayText != null)
            dayText.text = $"Day {currentDay}";
    }

    #endregion

    #region ======== 일일 수익/지출 기록 메서드 ========
    // 아래 메서드들은 “무기 제작을 판매했을 때”, “퀘스트 완료했을 때”,
    // “상점에서 재료 구매했을 때”, “강화에 돈을 썼을 때” 등을 호출하여 누적 값을 더하는 용도입니다.

    /// <summary>
    /// 무기 제작(판매)을 통해 얻은 금액만큼 누적
    /// </summary>
    public void AddProductionProfit(int amount)
    {
        if (amount <= 0) return;
        dailyProfit제작 += amount;
    }

    /// <summary>
    /// 의뢰(퀘스트) 완료로 받은 보상 금액만큼 누적
    /// </summary>
    public void AddQuestProfit(int amount)
    {
        if (amount <= 0) return;
        dailyProfit의뢰 += amount;
    }

    /// <summary>
    /// 상점에서 재료 구매 등에 지출한 금액만큼 누적
    /// </summary>
    public void AddStoreCost(int amount)
    {
        if (amount <= 0) return;
        dailyCost상점 += amount;
    }

    /// <summary>
    /// 강화(재료 구매 등)로 지출한 금액만큼 누적
    /// </summary>
    public void AddEnhanceCost(int amount)
    {
        if (amount <= 0) return;
        dailyCost강화 += amount;
    }
    #endregion

    #region ======== 테스트용 ContextMenu 메서드 ========
    [ContextMenu("테스트: 경험치 +120")]
    public void TestAddXP120()
    {
        AddXP(120);
    }

    [ContextMenu("테스트: 스트레스 1점")]
    public void TestAddStress()
    {
        AddStressPoint();
    }

    [ContextMenu("테스트: 제작 수익 +5000")]
    public void TestAddProdProfit()
    {
        AddProductionProfit(5000);
    }

    [ContextMenu("테스트: 퀘스트 수익 +3000")]
    public void TestAddQuestProfit()
    {
        AddQuestProfit(3000);
    }

    [ContextMenu("테스트: 상점 지출 2000")]
    public void TestAddStoreCost()
    {
        AddStoreCost(2000);
    }

    [ContextMenu("테스트: 강화 지출 1000")]
    public void TestAddEnhanceCost()
    {
        AddEnhanceCost(1000);
    }
    #endregion

    #region ======== 스트레스 초기화 & 다음 날 호출(Confirm 버튼 시) ========
    /// <summary>
    /// 다음 날로 넘어갈 때 스트레스 초기화, 날짜 증가, 수익/지출 초기화 등.
    /// DaySummaryManager의 Confirm 버튼 클릭 시 호출할 메서드.
    /// </summary>
    public void ResetForNextDay()
    {
        // 1) 날짜 증가
        currentDay++;
        UpdateDayText();

        // 2) 스트레스 초기화 (색 복원 및 변수 초기화)
        for (int i = 0; i < maxStress; i++)
            stressSegments[i].color = stressEmptyColor;
        currentStress = 0;
        UpdateFaceSprite();

        // 3) 일일 수익/지출 초기화 (다음 날 데이터를 새로 기록)
        dailyProfit제작 = 0;
        dailyProfit의뢰 = 0;
        dailyCost상점 = 0;
        dailyCost강화 = 0;
    }
    #endregion
}