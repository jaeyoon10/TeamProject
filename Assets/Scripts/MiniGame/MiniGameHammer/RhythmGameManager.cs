using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum Judgement { Perfect, Great, Good, Miss }
public enum Lane { A, D }

public class RhythmGameManager : MonoBehaviour
{
    [Header("UI 구조")]
    public RectTransform playArea;      // 갈색 판 부모
    public RectTransform judgeZoneRect; // 회색 원
    public RectTransform spawnPoint;    // 오른쪽 끝 스폰 지점
    public RhythmNote notePrefab;       // 노트 프리팹 (Image)

    [Header("표시 텍스트들(선택)")]
    public Text judgeText;

    [Header("속도/간격")]
    public float scrollSpeed = 350f;    // px/sec
    public float spawnInterval = 0.6f;  // 초

    [Header("판정 창 (pixels)")]
    public float perfectWindowPx = 14f;
    public float greatWindowPx = 28f;
    public float goodWindowPx = 46f;
    public float missWindowPx = 56f; // 지나침 허용치

    [Header("점수")]
    public int scorePerPerfect = 1000;
    public int scorePerGreat = 700;
    public int scorePerGood = 400;

    [Header("초기 레인 패턴(테스트용)")]
    public bool autoSpawn = true;       // 데모 패턴 자동생성
    public bool alternateAD = true;     // A-D-A-D…

    // 내부 상태
    [HideInInspector] public float judgeX;

    private readonly List<RhythmNote> activeNotes = new();
    private Coroutine spawnCo;
    private bool playing = false;

    private Lane nextLane = Lane.A;

    // === 해머 결과 집계 ===
    private int failsCount = 0;
    private int perfectCount = 0;
    private bool finished = false;

    // === 자동 종료용 ===
    [Header("게임 길이(초)")]
    public float songLength = 10f;   // 원하는 길이로 조절
    private float songStartTime = 0f;

    void Awake()
    {
        // HitLine의 로컬 X를 자동으로 사용
        if (judgeZoneRect != null) judgeX = judgeZoneRect.anchoredPosition.x;
    }

    void Start()
    {
        if (autoSpawn) StartGame();
    }
    public void StartGame()
    {
        if (playing) return;
        playing = true;

        failsCount = 0;
        perfectCount = 0;
        finished = false;

        // 추가: 이전 결과 초기화
        HammerResultData.Clear();

        songStartTime = Time.time;

        if (spawnCo != null) StopCoroutine(spawnCo);
        spawnCo = StartCoroutine(SpawnRoutine());
    }


    public void StopGame()
    {
        playing = false;
        if (spawnCo != null) StopCoroutine(spawnCo);
    }

    public void BeginWithCountdown(TextMeshProUGUI countdownText)
    {
        StartCoroutine(CoBegin(countdownText));
    }

    private IEnumerator CoBegin(TextMeshProUGUI t)
    {
        if (t)
        {
            t.gameObject.SetActive(true);
            t.text = "3"; yield return new WaitForSeconds(1f);
            t.text = "2"; yield return new WaitForSeconds(1f);
            t.text = "1"; yield return new WaitForSeconds(1f);
            t.gameObject.SetActive(false);
        }
        StartGame();
    }

    IEnumerator SpawnRoutine()
    {
        System.Random rng = new System.Random();
        while (playing)
        {
            // 곡 길이 초과 시 스폰 중단
            if (Time.time - songStartTime >= songLength)
            {
                playing = false;        // 스폰 종료
                spawnCo = null;
                break;
            }

            Lane lane = (rng.NextDouble() < 0.5) ? Lane.A : Lane.D;  // 50/50
            SpawnNote(lane);

            float wait = Random.Range(0.2f, 0.5f);
            yield return new WaitForSeconds(wait);
        }
        // 스폰이 멈춘 뒤, 모든 노트가 판정/소멸될 때까지 기다렸다 종료
        yield return new WaitUntil(() => activeNotes.Count == 0);
        FinishGame();  // ← 결과 저장 & 씬 복귀
    }

    void SpawnNote(Lane lane)
    {
        var n = Instantiate(notePrefab, playArea);

        // 프리팹 찌그러짐 방지: 중앙 앵커/피벗/스케일 고정
        var r = n.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.pivot = new Vector2(0.5f, 0.5f);
        r.localScale = Vector3.one;
        r.anchoredPosition = spawnPoint.anchoredPosition;

        n.Init(this, lane, scrollSpeed, judgeX);
        activeNotes.Add(n);
    }


    void Update()
    {
        if (!playing && !finished) // 스폰은 멈췄지만 아직 게임이 끝난 건 아닐 수 있음
        {
            // 모든 노트가 처리되면 FinishGame은 SpawnRoutine에서 호출됨
        }

        if (!playing && finished) return;
        if (playing)
        {
            if (Input.GetKeyDown(KeyCode.A)) TryHit(Lane.A);
            if (Input.GetKeyDown(KeyCode.D)) TryHit(Lane.D);
        }

        // === (추가) Miss 판정: 판정선 지나 더 왼쪽으로 벗어나면 Miss ===
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            var n = activeNotes[i];
            if (n == null || n.hasJudged) { activeNotes.RemoveAt(i); continue; }

            // 오른쪽→왼쪽으로 흐른다고 가정: hitLine보다 missWindowPx만큼 더 왼쪽이면 Miss
            if (n.rect.anchoredPosition.x < judgeX - missWindowPx)
            {
                n.hasJudged = true;
                OnJudge(n, Judgement.Miss, true);
                Destroy(n.gameObject);
                activeNotes.RemoveAt(i);
            }
        }
    }

    void TryHit(Lane lane)
    {
        RhythmNote target = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < activeNotes.Count; i++)
        {
            var n = activeNotes[i];
            if (n == null || n.hasJudged || n.lane != lane) continue;
            float dist = Mathf.Abs(n.rect.anchoredPosition.x - judgeX);
            if (dist < bestDist)
            {
                bestDist = dist;
                target = n;
            }
        }

        if (target == null) return;

        // 히트 창 밖이면 입력 무시 (삭제 안함)
        if (bestDist > goodWindowPx) return;

        Judgement j =
            (bestDist <= perfectWindowPx) ? Judgement.Perfect :
            (bestDist <= greatWindowPx) ? Judgement.Great :
                                            Judgement.Good;

        target.hasJudged = true;
        OnJudge(target, j, false);
        Destroy(target.gameObject);
    }

    public void OnJudge(RhythmNote note, Judgement j, bool auto)
    {
        switch (j)
        {
            case Judgement.Perfect:
                perfectCount++;
                ShowJudge("PERFECT", new Color32(255, 240, 120, 255));
                break;
            case Judgement.Great:
                ShowJudge("GREAT", new Color32(160, 255, 160, 255));
                break;
            case Judgement.Good:
                ShowJudge("GOOD", new Color32(160, 200, 255, 255));
                break;
            case Judgement.Miss:
                failsCount++;
                ShowJudge("MISS", new Color32(255, 120, 120, 255));
                break;
        }
        activeNotes.Remove(note);

        // TODO: hammerAnimator.SetTrigger("Hit");
        // TODO: ingotController.Progress(j);
    }

    void ShowJudge(string s, Color c)
    {
        if (!judgeText) return;
        judgeText.text = s;
        judgeText.color = c;
        StopCoroutine(nameof(FadeJudge));
        StartCoroutine(nameof(FadeJudge));
    }

    IEnumerator FadeJudge()
    {
        var col = judgeText.color; col.a = 1f; judgeText.color = col;
        float t = 0f, dur = 0.4f;
        while (t < dur) { t += Time.deltaTime; col.a = Mathf.Lerp(1f, 0f, t / dur); judgeText.color = col; yield return null; }
    }

    void FinishGame()
    {
        if (finished) return;
        finished = true;

        HammerResultData.Save(failsCount, perfectCount);

        // 2) 블로워와 동일하게 씬 복귀
        StartCoroutine(EndMiniGameAfterDelay(1f));
    }

    IEnumerator EndMiniGameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        var camTrans = Camera.main != null ? Camera.main.GetComponent<CameraSceneTransition>() : null;
        if (camTrans != null)
            camTrans.StartZoomOut("Ingame_main");  // 메인으로 축소 복귀
        else
            SceneManager.LoadScene("Ingame_main"); // fallback
    }

}