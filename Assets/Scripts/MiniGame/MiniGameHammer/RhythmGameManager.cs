using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum Judgement { Perfect, Great, Good, Miss }
public enum Lane { A, D }

public class RhythmGameManager : MonoBehaviour
{
    [Header("UI 구조")]
    public RectTransform playArea;
    public RectTransform judgeZoneRect;
    public RectTransform spawnPoint;
    public RhythmNote notePrefab;

    [Header("표시 텍스트들(선택)")]
    public Text judgeText;

    [Header("속도/간격")]
    public float scrollSpeed = 350f;
    public float spawnInterval = 0.6f;

    [Header("판정 창 (pixels)")]
    public float perfectWindowPx = 14f;
    public float greatWindowPx = 28f;
    public float goodWindowPx = 46f;
    public float missWindowPx = 56f;

    [Header("점수")]
    public int scorePerPerfect = 1000;
    public int scorePerGreat = 700;
    public int scorePerGood = 400;

    [Header("초기 레인 패턴(테스트용)")]
    public bool autoSpawn = true;

    [Header("게임 길이(초)")]
    public float songLength = 10f;

    [HideInInspector] public float judgeX;

    private readonly List<RhythmNote> activeNotes = new();
    private Coroutine spawnCo;
    private bool playing = false;

    private int failsCount = 0;
    private int perfectCount = 0;
    private bool finished = false;
    private float songStartTime = 0f;


    int _cntPerfect, _cntGreat, _cntGood, _cntMiss;

    void Awake()
    {
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

        HammerResultData.Clear();
        songStartTime = Time.time;

        if (spawnCo != null) StopCoroutine(spawnCo);
        spawnCo = StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        System.Random rng = new System.Random();
        while (playing)
        {
            if (Time.time - songStartTime >= songLength)
            {
                playing = false;
                spawnCo = null;
                break;
            }

            Lane lane = (rng.NextDouble() < 0.5) ? Lane.A : Lane.D;
            SpawnNote(lane);

            float wait = Random.Range(0.2f, 0.5f);
            yield return new WaitForSeconds(wait);
        }
        yield return new WaitUntil(() => activeNotes.Count == 0);
        FinishGame();
    }

    void SpawnNote(Lane lane)
    {
        var n = Instantiate(notePrefab, playArea);
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
        if (!playing && finished) return;

        if (!finished)
        {
            if (Input.GetKeyDown(KeyCode.A)) TryHit(Lane.A);
            if (Input.GetKeyDown(KeyCode.D)) TryHit(Lane.D);
        }

        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            var n = activeNotes[i];
            if (n == null || n.hasJudged) { activeNotes.RemoveAt(i); continue; }

            if (n.rect.anchoredPosition.x < judgeX - missWindowPx)
            {
                n.hasJudged = true;
                OnJudge(n, Judgement.Miss, true);
                Destroy(n.gameObject);
                activeNotes.RemoveAt(i);
            }
        }
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
            t.text = "3"; 
            yield return new WaitForSeconds(1f); 
            t.text = "2"; 
            yield return new WaitForSeconds(1f); 
            t.text = "1"; 
            yield return new WaitForSeconds(1f); 
            t.gameObject.SetActive(false);
        } 
        StartGame(); 
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
            if (dist < bestDist) { bestDist = dist; target = n; }
        }
        if (target == null) return;
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
                _cntPerfect++; 
                ShowJudge("PERFECT", new Color32(255, 240, 120, 255)); 
                break;

            case Judgement.Great:
                _cntGreat++;
                ShowJudge("GREAT", new Color32(160, 255, 160, 255)); 
                break;
            case Judgement.Good:
                _cntGood++;
                ShowJudge("GOOD", new Color32(160, 200, 255, 255)); 
                break;
            case Judgement.Miss: 
                _cntMiss++; 
                ShowJudge("MISS", new Color32(255, 120, 120, 255)); 
                break;
        }
        activeNotes.Remove(note);

        // TODO: hammer 애니/스파크 트리거 (판정별 강도 차이)
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

        Debug.Log($"[Hammer Finish] P:{_cntPerfect} G:{_cntGreat} D:{_cntGood} M:{_cntMiss}");

        HammerResultData.Save(_cntPerfect, _cntGreat, _cntGood, _cntMiss);
        //  완료 신호만 보내기 (복귀/전환은 상위 매니저가 처리)
        MiniGameState.HammerDone = true;
    }
}
