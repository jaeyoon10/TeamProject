using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GrindingWipeSimple : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Item")]
    public RectTransform itemArea;
    public Image itemImage;
    public List<Sprite> itemCandidates;

    [Header("Stain")]
    public StainBlob stainPrefab;
    public int minStainCount = 2;
    public int maxStainCount = 5;
    public Vector2 randomScaleRange = new Vector2(0.9f, 1.2f);
    public float padding = 16f;
    public bool avoidOverlap = true;
    public float minDistanceBetweenStains = 80f;
    public float alphaThreshold = 0.1f;

    [Header("Brush")]
    public float brushRadius = 60f;
    public bool showBrushCursor = true;
    public RectTransform brushCursor;

    [Header("UI")]
    public Text progressText;     // 퍼센트 표시
    public Text gradeText;        // Perfect / Great / Good / Miss 표시

    [Header("Game Rules")]
    [Range(0f, 1f)] public float clearThreshold = 0.85f;
    public int roundsToPlay = 2;

    bool _pressing;
    Camera _uiCam;
    List<StainBlob> _stains = new List<StainBlob>();
    int _currentRound = 0;
    bool _finished = false;
    float _roundStartTime;
    float _cleared01;

    void Awake()
    {
        if (brushCursor) brushCursor.gameObject.SetActive(false);
        _uiCam = Camera.main;
    }

    void Start()
    {
        StartRound();
        StartCoroutine(CoCheckProgress());
    }

    // ========== 입력 ==========
    public void OnPointerDown(PointerEventData e) { if (_finished) return; _pressing = true; TryWipe(e); ToggleCursor(true, e); }
    public void OnPointerUp(PointerEventData e) { _pressing = false; ToggleCursor(false, e); }
    public void OnDrag(PointerEventData e) { if (_finished) return; if (_pressing) { TryWipe(e); ToggleCursor(true, e); } }

    void ToggleCursor(bool on, PointerEventData e)
    {
        if (!showBrushCursor || brushCursor == null) return;
        brushCursor.gameObject.SetActive(on);
        if (on && RectTransformUtility.ScreenPointToLocalPointInRectangle(itemArea, e.position, e.pressEventCamera, out var lp))
        {
            brushCursor.anchoredPosition = lp;
            brushCursor.sizeDelta = new Vector2(brushRadius * 2f, brushRadius * 2f);
        }
    }

    void TryWipe(PointerEventData e)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(itemArea, e.position, e.pressEventCamera, out var lp))
            return;

        var rect = itemArea.rect;
        if (!rect.Contains(lp)) return;

        Vector2 worldPos = itemArea.TransformPoint(lp);
        float dt = Time.deltaTime;

        for (int i = 0; i < _stains.Count; i++)
        {
            var s = _stains[i];
            if (!s || !s.gameObject.activeSelf) continue;

            var rt = (RectTransform)s.transform;
            Vector2 stainCenter = rt.TransformPoint(Vector3.zero);
            if (Vector2.Distance(worldPos, stainCenter) <= brushRadius)
                s.WipeTick(dt);
        }
    }

    // ========== 라운드 ==========
    void StartRound()
    {
        if (brushCursor) brushCursor.gameObject.SetActive(false);
        _pressing = false;
        _cleared01 = 0f;
        gradeText.text = "";
        if (progressText) progressText.text = "Clean 0%";

        // 스프라이트 교체
        if (itemImage && itemCandidates.Count > 0)
            itemImage.sprite = itemCandidates[Random.Range(0, itemCandidates.Count)];

        // 기존 얼룩 제거
        foreach (var s in _stains)
            if (s) Destroy(s.gameObject);
        _stains.Clear();

        // 새 얼룩 생성
        SpawnRandomStainsInsideItem();

        // 시간 기록
        _roundStartTime = Time.time;
    }

    IEnumerator CoCheckProgress()
    {
        var wait = new WaitForSeconds(0.2f);
        while (!_finished)
        {
            int total = 0, cleared = 0;
            foreach (var s in _stains)
            {
                if (s == null) continue;
                total++;
                if (s.IsCleared || !s.gameObject.activeSelf) cleared++;
            }
            _cleared01 = (total == 0) ? 1f : (cleared / (float)total);

            if (progressText)
                progressText.text = $"Clean {(int)(_cleared01 * 100f)}%";

            if (_cleared01 >= clearThreshold)
            {
                float elapsed = Time.time - _roundStartTime;
                string grade = CalcGrade(elapsed);
                if (gradeText) gradeText.text = grade;

                _currentRound++;
                if (_currentRound < roundsToPlay)
                {
                    yield return new WaitForSeconds(1f);
                    StartRound();
                }
                else
                {
                    yield return new WaitForSeconds(1f);
                    FinishGame();
                    yield break;
                }
            }

            yield return wait;
        }
    }

    string CalcGrade(float t)
    {
        if (t <= 6f) return "PERFECT!";
        else if (t <= 7f) return "GREAT!";
        else if (t <= 9f) return "GOOD";
        else return "MISS";
    }

    void FinishGame()
    {
        if (_finished) return;
        _finished = true;
        MiniGameState.GrindingDone = true;
        if (progressText) progressText.text = "Complete!";
    }

    // ========== 스폰 (아이템 알파 내부) ==========
    void SpawnRandomStainsInsideItem()
    {
        if (!itemArea || !itemImage || !itemImage.sprite || !stainPrefab) return;

        int count = Random.Range(minStainCount, maxStainCount + 1);
        Rect drawRect = GetSpriteDrawRect(itemArea, itemImage);

        drawRect.xMin += padding;
        drawRect.xMax -= padding;
        drawRect.yMin += padding;
        drawRect.yMax -= padding;

        List<Vector2> placed = new List<Vector2>();
        const int MAX_TRY = 80;

        for (int i = 0; i < count; i++)
        {
            Vector2 localPos = Vector2.zero;
            int tries = 0;
            bool ok = false;

            while (tries++ < MAX_TRY)
            {
                float x = Random.Range(drawRect.xMin, drawRect.xMax);
                float y = Random.Range(drawRect.yMin, drawRect.yMax);
                localPos = new Vector2(x, y);

                if (!IsInsideSpriteAlpha(itemImage, localPos, alphaThreshold))
                    continue;

                if (avoidOverlap && !IsFarEnough(localPos, placed, minDistanceBetweenStains))
                    continue;

                ok = true;
                break;
            }

            if (!ok) continue;

            var inst = Instantiate(stainPrefab, itemArea);
            var rt = (RectTransform)inst.transform;
            rt.anchoredPosition = localPos;
            float s = Random.Range(randomScaleRange.x, randomScaleRange.y);
            rt.localScale = new Vector3(s, s, 1f);
            inst.gameObject.SetActive(true);

            _stains.Add(inst);
            placed.Add(localPos);
        }
    }

    Rect GetSpriteDrawRect(RectTransform target, Image img)
    {
        Rect r = target.rect;
        if (!img || !img.sprite) return r;
        if (!img.preserveAspect) return r;

        var texRect = img.sprite.textureRect;
        float spriteW = texRect.width;
        float spriteH = texRect.height;
        float spriteAspect = spriteW / spriteH;
        float rectAspect = r.width / r.height;

        if (rectAspect > spriteAspect)
        {
            float drawH = r.height;
            float drawW = drawH * spriteAspect;
            float x = r.xMin + (r.width - drawW) * 0.5f;
            return new Rect(x, r.yMin, drawW, drawH);
        }
        else
        {
            float drawW = r.width;
            float drawH = drawW / spriteAspect;
            float y = r.yMin + (r.height - drawH) * 0.5f;
            return new Rect(r.xMin, y, drawW, drawH);
        }
    }

    bool IsInsideSpriteAlpha(Image img, Vector2 localPoint, float threshold)
    {
        if (!img || !img.sprite) return false;
        var tex = img.sprite.texture;
        if (!tex) return false;

        Rect drawRect = GetSpriteDrawRect(itemArea, img);
        if (!drawRect.Contains(localPoint)) return false;

        float u = (localPoint.x - drawRect.xMin) / drawRect.width;
        float v = (localPoint.y - drawRect.yMin) / drawRect.height;

        Rect tr = img.sprite.textureRect;
        float texU = Mathf.Lerp(tr.xMin / tex.width, tr.xMax / tex.width, u);
        float texV = Mathf.Lerp(tr.yMin / tex.height, tr.yMax / tex.height, v);

        Color c = tex.GetPixelBilinear(texU, texV);
        return c.a > threshold;
    }

    bool IsFarEnough(Vector2 p, List<Vector2> others, float minDist)
    {
        foreach (var o in others)
        {
            if (Vector2.Distance(p, o) < minDist) return false;
        }
        return true;
    }
}
