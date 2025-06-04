using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterInfoManager : MonoBehaviour
{
    [Header("=== 캐릭터 초상화 ===")]
    public Image characterPortrait;       // PortraitFrame > CharacterPortrait
    public bool preserveAspect = true;    // Inspector에서 켜둘 수도 있음

    [Header("=== 레벨 & 경험치 ===")]
    public TMP_Text levelText;            // "LV.1" 표시
    public Slider xpBar;                  // Slider (Min과 Max는 코드에서 세팅)
    public int currentLevel = 1;          // 초기 레벨 (LV.1)
    private int currentXP = 0;            // 현재 경험치 (게이지)
    private int xpPerLevel;               // 현재 레벨에서 다음 레벨까지 필요한 경험치

    [Header("Level Up Effect")]
    public CanvasGroup levelUpCanvasGroup; // LevelUpEffect > LevelUpText의 CanvasGroup
    public float levelUpFadeDuration = 0.5f; // 페이드 인/아웃 시간

    [Header("=== 스트레스 바 ===")]
    public List<Image> stressSegments = new List<Image>(); // Segment_0 ~ Segment_N
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

    private void Awake()
    {
        // 1) 초기 레벨용 필요 경험치 계산
        xpPerLevel = CalculateXPForNextLevel(currentLevel);

        // 2) XP Bar 세팅
        xpBar.minValue = 0;
        xpBar.maxValue = xpPerLevel;
        xpBar.value = currentXP;

        // 3) 레벨 텍스트 초기화
        UpdateLevelText();

        // 4) 레벨업 이펙트 숨김
        if (levelUpCanvasGroup != null)
            levelUpCanvasGroup.alpha = 0;

        // 5) 스트레스 세그먼트 색깔 초기화
        maxStress = stressSegments.Count;
        for (int i = 0; i < maxStress; i++)
        {
            stressSegments[i].color = stressEmptyColor;
        }

        // 6) 표정 초기화
        if (faceImage != null && faceNormal != null)
            faceImage.sprite = faceNormal;

        // 7) 날짜 초기화
        UpdateDayText();
    }

    #region ======== 캐릭터 초상화 설정 ========
    /// <summary>
    /// 외부에서 선택된 캐릭터 스프라이트를 전달하면, 흰색 프레임에 꽉 차도록 교체
    /// </summary>
    /// <param name="newPortrait">새로 표시할 스프라이트</param>
    public void SetCharacterPortrait(Sprite newPortrait)
    {
        if (characterPortrait == null || newPortrait == null) return;

        characterPortrait.sprite = newPortrait;
        characterPortrait.preserveAspect = preserveAspect;
    }
    #endregion

    #region ======== 경험치(EXP) & 레벨 ========
    /// <summary>
    /// 경험치를 추가하고, 레벨업 가능 시 레벨업 처리 (게이지 초기화)
    /// </summary>
    /// <param name="amount">추가할 경험치</param>
    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        currentXP += amount;

        // 레벨업 조건: 누적된 currentXP가 필요 경험치 이상이면 곧바로 레벨업
        if (currentXP >= xpPerLevel)
        {
            // 1) 게이지를 초기화 (나머지 경험치 부여하지 않음)
            currentXP = 0;

            // 2) 레벨업 처리 (currentLevel 증가 + 텍스트/이펙트)
            LevelUpRoutine();

            // 3) 다음 레벨 필요 경험치 다시 계산
            xpPerLevel = CalculateXPForNextLevel(currentLevel);
        }

        // 4) Slider 업데이트 (Max가 변경되었을 수도 있으므로 반드시 재할당)
        xpBar.maxValue = xpPerLevel;
        xpBar.value = currentXP;
    }

    /// <summary>
    /// 레벨업 처리: 레벨 증가, 텍스트 갱신, 레벨업 UI 이펙트 실행
    /// </summary>
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

        // 페이드 인
        float t = 0f;
        while (t < levelUpFadeDuration)
        {
            t += Time.deltaTime;
            levelUpCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / levelUpFadeDuration);
            yield return null;
        }

        // 잠시 대기 (1초)
        yield return new WaitForSeconds(1f);

        // 페이드 아웃
        t = 0f;
        while (t < levelUpFadeDuration)
        {
            t += Time.deltaTime;
            levelUpCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / levelUpFadeDuration);
            yield return null;
        }
        levelUpCanvasGroup.alpha = 0f;
    }

    /// <summary>
    /// 현재 레벨에서 다음 레벨로 올라가기 위해 필요한 경험치를 계산
    ///  - LV1→2: 100
    ///  - LV2→3: 130 (+30)
    ///  - LV3→4: 150 (+20)
    ///  - LV4→5: 180 (+30)
    ///  - LV5→6: 200 (+20)
    ///  ...
    /// 홀수 레벨에서 짝수 레벨로 갈 때 +30, 짝수 레벨에서 홀수 레벨로 갈 때 +20을 적용합니다.
    /// </summary>
    private int CalculateXPForNextLevel(int level)
    {
        // level = 현재 레벨 (예: 1), 반환값 = “level → level+1” 에 필요한 XP
        int baseXP = 100;

        // level이 1일 때 그대로 100 반환
        if (level == 1) return baseXP;

        // level > 1인 경우, 1→2 까지는 100을 기준으로 했으니
        // 2→3: +30, 3→4: +20, 4→5: +30, ... 을 반복해서 더함
        // 반복문을 level-1 번 수행해서 최종 누적값을 구함
        int xpNeeded = baseXP;
        for (int lv = 1; lv < level; lv++)
        {
            // lv이 홀수면 다음 레벨로 넘어갈 때 +30
            if (lv % 2 == 1)
                xpNeeded += 30;
            else
                xpNeeded += 20;
        }

        return xpNeeded;
    }
    #endregion

    #region ======== 스트레스 게이지 & 날짜 ========
    /// <summary>
    /// 스트레스 포인트를 1만큼 쌓는다. 
    /// 스트레스가 max에 도달하면 날짜 변경 처리.
    /// </summary>
    public void AddStressPoint()
    {
        if (currentStress >= maxStress)
            return;

        // 1포인트 추가
        stressSegments[currentStress].color = stressFillColor;
        currentStress++;

        UpdateFaceSprite();

        // 만약 가득 찼다면
        if (currentStress >= maxStress)
        {
            // 하루가 지나가도록 처리
            StartCoroutine(OnStressMaxRoutine());
        }
    }

    /// <summary>
    /// 표정 이미지를 스트레스 정도에 따라 변경
    /// </summary>
    private void UpdateFaceSprite()
    {
        if (faceImage == null) return;

        if (currentStress == 0)
        {
            faceImage.sprite = faceNormal;
        }
        else if (currentStress < maxStress / 2f)
        {
            faceImage.sprite = faceHalf;
        }
        else if (currentStress < maxStress)
        {
            faceImage.sprite = faceHalf;
        }
        else
        {
            faceImage.sprite = faceMax;
        }
    }

    /// <summary>
    /// 스트레스 바가 가득 찼을 때 호출되는 코루틴
    /// → 하루가 지나고 스트레스 초기화
    /// </summary>
    private IEnumerator OnStressMaxRoutine()
    {
        // 예: “하루가 지났습니다!” 같은 메시지나 이펙트를 넣고 싶으면 이곳에 추가
        yield return new WaitForSeconds(1f);

        // 날짜 증가
        currentDay++;
        UpdateDayText();

        // 스트레스 초기화(색 복원)
        for (int i = 0; i < maxStress; i++)
        {
            stressSegments[i].color = stressEmptyColor;
        }
        currentStress = 0;
        UpdateFaceSprite();

        yield break;
    }

    private void UpdateDayText()
    {
        if (dayText != null)
            dayText.text = $"Day {currentDay}";
    }
    #endregion

    #region ======== 테스트용 입력 예시 ========
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
    #endregion
}