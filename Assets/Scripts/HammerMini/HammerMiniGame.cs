using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HammerMiniGame : MonoBehaviour
{
    /* ---------- 인스펙터 ---------- */
    [Header("UI References")]
    public RectTransform pointerRoot;
    public RectTransform pointer;
    public RectTransform zoneContainer;
    public TextMeshProUGUI feedbackText;

    [Header("Prefabs & Settings")]
    public UIArc arcPrefab;
    public int minZones = 1, maxZones = 1;
    public float rotationSpeed = 180f;

    [Header("Success UI (3개 원)")]
    public Image[] successIndicators;
    public Color offColor = Color.gray;
    public Color perfectColor = new Color(0.3f, 0.9f, 1f);
    public Color greatColor = Color.green;
    public Color goodColor = Color.yellow;
    public Color missColor = Color.red;

    [Header("Game Settings")]
    public int requiredSuccesses = 3;
    public float perfectZoneSize = 10f;
    public float greatZoneSize = 30f;
    public float goodZoneSize = 60f;
    public float hitArcSize = 60f;

    /* ---------- 품질 계산 공개 카운터 ---------- */
    public int perfectCount { get; private set; }   // 퍼펙트 횟수
    public int failCount { get; private set; }   // Miss 횟수
    public System.Action onMiniGameSuccess;         // 성공 콜백

    /* ---------- 내부 ---------- */
    private readonly List<UIArc> arcs = new();
    private readonly List<float> zoneAngles = new();
    private float currentAngle;
    private bool isChecking;
    private int successCount;   // Good·Great·Perfect 누적
    private int attemptCount;   // 총 시도(=Space) 횟수

    /* ---------- 초기화 ---------- */
    void Start()
    {
        feedbackText.alpha = 0f;
        foreach (var img in successIndicators) img.color = offColor;
        SetupPointer();
        SpawnZones();
    }

    /* ---------- 매 프레임 ---------- */
    void Update()
    {
        currentAngle = (currentAngle + rotationSpeed * Time.deltaTime) % 360f;
        pointerRoot.localEulerAngles = new Vector3(0, 0, -currentAngle);

        if (!isChecking && Input.GetKeyDown(KeyCode.Space))
            StartCoroutine(CheckTiming());

        pointerRoot.SetAsLastSibling();   // 항상 맨 위 렌더
    }

    /* ---------- 핵심 판정 ---------- */
    IEnumerator CheckTiming()
    {
        isChecking = true;

        /* 1) 가장 가까운 존 찾기 */
        float pointerAng = currentAngle;
        float bestDiff = 180f;
        for (int i = 0; i < zoneAngles.Count; i++)
            bestDiff = Mathf.Min(bestDiff,
                       Mathf.Abs(Mathf.DeltaAngle(pointerAng, zoneAngles[i])));

        /* 2) 결과 결정 */
        string res; Color col;
        if (bestDiff <= perfectZoneSize * 0.5f) { res = "Perfect"; col = perfectColor; perfectCount++; }
        else if (bestDiff <= greatZoneSize * 0.5f) { res = "Great"; col = greatColor; }
        else if (bestDiff <= goodZoneSize * 0.5f) { res = "Good"; col = goodColor; }
        else { res = "Miss"; col = missColor; failCount++; }

        /* 3) UI 표시 */
        feedbackText.text = res;
        feedbackText.color = col; feedbackText.alpha = 1f;
        if (attemptCount < successIndicators.Length)
            successIndicators[attemptCount].color = col;

        /* 4) 누적 카운트 */
        if (res != "Miss") successCount++;
        attemptCount++;

        /* 5) 종료 조건 */
        if (successCount >= requiredSuccesses || attemptCount >= successIndicators.Length)
        {
            OnGameSuccess();                             // 콜백 + 딜레이 언로드
            yield break;
        }

        /* 6) 다음 판 대비 */
        yield return new WaitForSeconds(0.6f);
        feedbackText.alpha = 0f;
        isChecking = false;
        SpawnZones();
    }

    /* ---------- 보조 메서드들 (기존 로직과 동일) ---------- */
    void SetupPointer()
    {
        float outerR = zoneContainer.rect.height * 0.5f;
        float halfPtr = pointer.sizeDelta.y * 0.5f;
        float orbitR = outerR - halfPtr;

        pointerRoot.pivot = new Vector2(0.5f, 0.5f);
        pointerRoot.anchorMin = pointerRoot.anchorMax =
            new Vector2(0.5f, 0.5f);
        pointerRoot.anchoredPosition = Vector2.zero;

        pointer.pivot = new Vector2(0.5f, 0.5f);
        pointer.anchorMin = pointer.anchorMax =
            new Vector2(0.5f, 0.5f);
        pointer.anchoredPosition = new Vector2(0, orbitR);
    }
    void SpawnZones()
    {
        // 기존 삭제
        foreach (var a in arcs) if (a) Destroy(a.gameObject);
        arcs.Clear();
        zoneAngles.Clear();

        int count = Random.Range(minZones, maxZones + 1);
        for (int i = 0; i < count; i++)
        {
            // 1) UIArc 인스턴스 생성

            var a = Instantiate(arcPrefab, zoneContainer);
            var rt = a.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;


            // 2) 랜덤 중심 각도
            float center = Random.Range(0f, 360f);
            zoneAngles.Add(center);

            // 3) Perfect / Great / Good / Miss 4단계 표시
            //    Perfect
            a.startAngle = center - hitArcSize * 0.5f;
            a.endAngle = center + hitArcSize * 0.5f;
            //a.color = Color.white;
            //a.radius = zoneContainer.rect.height * 0.5f;
            //a.thickness = pointer.sizeDelta.y;

            arcs.Add(a);
        }
    }
    void OnGameSuccess()
    {
        perfectCount = successIndicators.Count(img => img.color == perfectColor);
        failCount = successIndicators.Count(img => img.color == missColor);

        onMiniGameSuccess?.Invoke();
        // 바로 언로드하지 않고 잠깐 대기
        StartCoroutine(DelayedFinish());
    }

    private IEnumerator DelayedFinish()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.UnloadSceneAsync("MinigameHammerHit");
    }
}


