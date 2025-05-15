using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HammerMiniGame : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform pointerRoot;
    public RectTransform pointer;
    public RectTransform zoneContainer;       // QTEContainer
    public TextMeshProUGUI feedbackText;

    [Header("Prefabs & Settings")]
    public UIArc arcPrefab;                   // UIArc 프리팹 참조
    public int minZones = 1, maxZones = 1;    // 아크 개수 (지금은 1)
    public float rotationSpeed = 180f;

    [Header("Success UI")]
    [Tooltip("위에 배치한 3개의 원 이미지들")]
    public Image[] successIndicators;  // 인스펙터에서 크기를 3으로
    public Color offColor = Color.gray;
    public Color perfectColor = Color.blue;
    public Color greatColor = Color.green;
    public Color goodColor = Color.yellow;
    public Color missColor = Color.red;

    [Header("Game Settings")]
    public int requiredSuccesses = 3;  // 통과에 필요한 성공 횟수

    [Tooltip("판정 구간 크기 (각도)")]
    public float perfectZoneSize = 10f;
    public float greatZoneSize = 30f;
    public float goodZoneSize = 60f;

    [Tooltip("히트존 아크 전체 크기(도)")]
    public float hitArcSize = 60f;

    private float currentAngle;
    private List<UIArc> arcs = new List<UIArc>();
    private List<float> zoneAngles = new List<float>();
    private bool isChecking;

    //성공 누적치
    private int successCount = 0;
    //시도 횟수
    private int attemptCount = 0;

    void Start()
    {
        feedbackText.alpha = 0f;
        SetupPointer();

        // 성공 인디케이터 초기화
        for (int i = 0; i < successIndicators.Length; i++)
            successIndicators[i].color = offColor;

        SpawnZones();
    }

    void Update()
    {
        // 포인터 회전
        currentAngle = (currentAngle + rotationSpeed * Time.deltaTime) % 360f;
        pointerRoot.localEulerAngles = new Vector3(0, 0, -currentAngle);

        if (!isChecking && Input.GetKeyDown(KeyCode.Space))
            StartCoroutine(CheckTimingAndFeedback());

        pointerRoot.SetAsLastSibling();
    }

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

    IEnumerator CheckTimingAndFeedback()
    {
        isChecking = true;

        // 포인터 각도 (currentAngle과 동일)
        float pointerAng = currentAngle;

        // 가장 가까운 존 찾기
        float best = 180f;
        int idx = 0;
        for (int i = 0; i < zoneAngles.Count; i++)
        {
            float diff = Mathf.Abs(Mathf.DeltaAngle(pointerAng, zoneAngles[i]));
            if (diff < best)
            {
                best = diff; idx = i;
            }
        }

        float rel = best;
        string res; Color col;
        if (rel <= perfectZoneSize * 0.5f) { res = "Perfect"; col = Color.cyan; }
        else if (rel <= greatZoneSize * 0.5f) { res = "Great"; col = Color.green; }
        else if (rel <= goodZoneSize * 0.5f) { res = "Good"; col = Color.yellow; }
        else { res = "Miss"; col = Color.red; }

        feedbackText.text = res;
        feedbackText.color = col;
        feedbackText.alpha = 1f;

        Debug.Log($"[DEBUG] Pointer:{pointerAng:F1}° HitZone:{zoneAngles[idx]:F1}° Δ{rel:F1}°→{res}");

        if (attemptCount < successIndicators.Length)
            successIndicators[attemptCount].color = col;

        // 2) 성공 카운트 조정
        if (res != "Miss")
            successCount++;
        else
            successCount = Mathf.Max(0, successCount - 1);

        attemptCount++;

        // 3) 조건 체크
        if (successCount >= requiredSuccesses)
        {
            OnGameSuccess();    // 3번 성공으로 클리어
            yield break;
        }
        if (attemptCount >= successIndicators.Length)
        {
            OnGameFail();       // 3번 다 써버리면 실패 처리
            yield break;
        }

        // 짧게 대기 후 페이드 아웃
        yield return new WaitForSeconds(0.6f);
        float t = 0f, fadeDur = 0.3f;
        while (t < fadeDur)
        {
            t += Time.deltaTime;
            feedbackText.alpha = Mathf.Lerp(1f, 0f, t / fadeDur);
            yield return null;
        }

        isChecking = false;
        SpawnZones();
    }


    void OnGameSuccess()
    {
        //통과 처리
        Debug.Log(" MiniGame Cleared!");
        
    }

    void OnGameFail()
    {
        //실패 처리
        Debug.Log("MiniGame Failed!");
    }
}