using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class CharacterInfoManager : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────
    // 싱글톤 인스턴스
    public static CharacterInfoManager Instance { get; private set; }  // ← 추가

    [Header("=== 캐릭터 초상화 ===")]
    public Image characterPortrait;
    public bool preserveAspect = true;

    [Header("Portrait Sprites")]
    public Sprite edwinPortrait;
    public Sprite isabellaPortrait;
    public Sprite tuskPortrait;

    [Header("=== 골드 ===")]  // ← 추가
    public TMP_Text goldText;       // UI에 표시할 골드 텍스트
    private int gold;               // 내부 골드 관리 변수
    public int CurrentGold => gold;

    [Header("=== 레벨 & 경험치 ===")]
    public TMP_Text levelText;
    public Slider xpBar;
    public int currentLevel = 1;
    private int currentXP = 0;
    private int xpPerLevel;

    [Header("Level Up Effect")]
    public CanvasGroup levelUpCanvasGroup;
    public float levelUpFadeDuration = 0.5f;

    [Header("=== 스트레스 바 ===")]
    public List<Image> stressSegments = new List<Image>();
    public Color stressEmptyColor = new Color(1f, 0.92f, 0.02f, 1f);
    public Color stressFillColor = Color.red;
    private int currentStress = 0;
    private int maxStress;

    [Header("Face Sprites")]
    public Sprite faceNormal;
    public Sprite faceHalf;
    public Sprite faceMax;
    public Image faceImage;

    [Header("=== 날짜 관리 ===")]
    public TMP_Text dayText;
    private int currentDay = 1;

    [Header("=== 일일 수익/지출 누적 변수 ===")]
    private int dailyProfit제작 = 0;
    private int dailyProfit의뢰 = 0;
    private int dailyCost상점 = 0;
    private int dailyCost강화 = 0;

    [Header("=== 의뢰 UI 매니저 참조 ===")]
    public QuestUIManager questUIManager;
    [Header("=== DaySummary 매니저 참조 ===")]
    public DaySummaryManager daySummaryManager;

    [Header("레벨업 UI 연결")]
    public LevelUpUI levelUpUI;

    #region ===== 공개 속성 =====
    public int CurrentDay => currentDay;
    public int CurrentLevel => currentLevel;
    public int CurrentXP => currentXP;
    public int CurrentStress => currentStress;
    #endregion

    void Awake()
    {
        // 싱글톤 세팅
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;  // ← 추가

        ApplySavedPortrait();

        // 경험치 세팅
        xpPerLevel = CalculateXPForNextLevel(currentLevel);
        xpBar.minValue = 0;
        xpBar.maxValue = xpPerLevel;
        xpBar.value = currentXP;
        UpdateLevelText();

        // 레벨업 이펙트
        if (levelUpCanvasGroup != null) levelUpCanvasGroup.alpha = 0;

        // 스트레스 초기화
        maxStress = stressSegments.Count;
        for (int i = 0; i < maxStress; i++) stressSegments[i].color = stressEmptyColor;
        if (faceImage != null && faceNormal != null) faceImage.sprite = faceNormal;

        // 날짜 초기화
        UpdateDayText();

        // 골드 초기 UI
        gold = 0;
        UpdateGoldUI();  // ← 추가
    }

    private void ApplySavedPortrait()
    {
        string selected = PlayerPrefs.GetString("SelectedCharacter", "");
        switch (selected)
        {
            case "Edwin": SetCharacterPortrait(edwinPortrait); break;
            case "Isabella": SetCharacterPortrait(isabellaPortrait); break;
            case "Tusk": SetCharacterPortrait(tuskPortrait); break;
            default: Debug.LogWarning("[CharacterInfoManager] 선택된 캐릭터 없음"); break;
        }
    }

    public void SetLevel(int level, int xp)
    {
        currentLevel = level;
        currentXP = xp;
        xpPerLevel = CalculateXPForNextLevel(currentLevel);
        xpBar.maxValue = xpPerLevel;
        xpBar.value = currentXP;
        UpdateLevelText();
    }

    /// <summary>
    /// 날짜(일차)를 바로 설정합니다.
    /// </summary>
    public void SetDay(int day)
    {
        currentDay = day;
        UpdateDayText();
    }

    /// <summary>
    /// 스트레스 포인트를 바로 설정합니다.
    /// </summary>
    public void SetStress(int stress)
    {
        currentStress = Mathf.Clamp(stress, 0, maxStress);
        for (int i = 0; i < maxStress; i++)
            stressSegments[i].color = (i < currentStress) ? stressFillColor : stressEmptyColor;
        UpdateFaceSprite();
    }

    #region ===== 캐릭터 초상화 =====
    public void SetCharacterPortrait(Sprite newPortrait)
    {
        if (characterPortrait == null || newPortrait == null) return;
        characterPortrait.sprite = newPortrait;
        characterPortrait.preserveAspect = preserveAspect;
    }
    #endregion

    #region ===== 골드 =====  // ← 추가
    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        gold += amount;
        UpdateGoldUI();
    }

    private void UpdateGoldUI()  // ← 추가
    {
        if (goldText != null)
            goldText.text = $"{gold}G";
    }
    #endregion

    #region ===== 경험치(EXP) & 레벨 =====
    public void AddXP(int amount)
    {
        if (amount <= 0) return;
        currentXP += amount;
        if (currentXP >= xpPerLevel)
        {
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

        if (levelUpUI != null)
            levelUpUI.ShowLevelUp(currentLevel);
    }

    private void UpdateLevelText()
    {
        if (levelText != null)
            levelText.text = $"LV.{currentLevel}";
    }

    private IEnumerator PlayLevelUpEffect()
    {
        if (levelUpCanvasGroup == null) yield break;
        float t = 0f;
        while (t < levelUpFadeDuration)
        {
            t += Time.deltaTime;
            levelUpCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / levelUpFadeDuration);
            yield return null;
        }
        yield return new WaitForSeconds(1f);
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
        if (level == 1) return baseXP;
        int xpNeeded = baseXP;
        for (int lv = 1; lv < level; lv++) xpNeeded += (lv % 2 == 1) ? 30 : 20;
        return xpNeeded;
    }
    #endregion

    #region ===== 스트레스 & 날짜 =====
    public void AddStressPoint()
    {
        if (currentStress >= maxStress) return;
        stressSegments[currentStress].color = stressFillColor;
        currentStress++;
        UpdateFaceSprite();
        if (currentStress >= maxStress)
            StartCoroutine(OnStressMaxRoutine());
    }

    private void UpdateFaceSprite()
    {
        if (faceImage == null) return;
        if (currentStress < maxStress * 0.3f) faceImage.sprite = faceNormal;
        else if (currentStress < maxStress * 0.7f) faceImage.sprite = faceHalf;
        else faceImage.sprite = faceMax;
    }

    private IEnumerator OnStressMaxRoutine()
    {
        yield return new WaitForSeconds(1f);
        if (daySummaryManager != null)
            daySummaryManager.ShowDaySummary(dailyProfit제작, dailyProfit의뢰, dailyCost상점, dailyCost강화);
        else
            Debug.LogWarning("[CharacterInfoManager] daySummaryManager 없음");
    }

    private void UpdateDayText()
    {
        if (dayText != null)
            dayText.text = $"Day {currentDay}";
    }

    #endregion

    #region ===== 수익/지출 기록 =====
    public void AddProductionProfit(int amount) { if (amount > 0) dailyProfit제작 += amount; }
    public void AddQuestProfit(int amount) { if (amount > 0) dailyProfit의뢰 += amount; }
    public void AddStoreCost(int amount) { if (amount > 0) dailyCost상점 += amount; }
    public void AddEnhanceCost(int amount) { if (amount > 0) dailyCost강화 += amount; }
    #endregion

    #region ===== 스트레스 초기화 & 다음 날 =====
    public void ResetForNextDay()
    {
        currentDay++;
        UpdateDayText();
        for (int i = 0; i < maxStress; i++) stressSegments[i].color = stressEmptyColor;
        currentStress = 0;
        UpdateFaceSprite();
        dailyProfit제작 = dailyProfit의뢰 = dailyCost상점 = dailyCost강화 = 0;
    }
    #endregion

    public void AddTestXP()
    {
        AddXP(50); // 원하는 값 넣으면 됨
        Debug.Log("[CharacterInfoManager] 테스트용 XP 50 추가!");
    }
}