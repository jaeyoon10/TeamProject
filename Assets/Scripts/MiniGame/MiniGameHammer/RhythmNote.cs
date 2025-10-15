using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RhythmNote : MonoBehaviour
{
    public RectTransform rect;
    public Image img;
    public TextMeshProUGUI textTMP; // 변경: 이름 분리
    public Text textUGUI;

    [HideInInspector] public Lane lane;
    [HideInInspector] public float speed;
    [HideInInspector] public float judgeX;
    [HideInInspector] public bool hasJudged;

    private RhythmGameManager mgr;

    public void Init(RhythmGameManager manager, Lane laneType, float spd, float judgeCenterX)
    {
        mgr = manager;
        lane = laneType;
        speed = spd;
        judgeX = judgeCenterX;

        if (!rect) rect = GetComponent<RectTransform>();
        if (!img) img = GetComponent<Image>();

        // 텍스트 참조 확보 (TMP 우선, 없으면 UGUI Text 찾기)
        if (!textTMP) textTMP = GetComponentInChildren<TextMeshProUGUI>(true);
        if (!textTMP && !textUGUI) textUGUI = GetComponentInChildren<Text>(true);

        // 레인별 색/문자 강제 세팅
        if (lane == Lane.A)
        {
            if (img) img.color = new Color(0.5f, 1f, 0.5f); // 연두
            if (textTMP) textTMP.text = "A";
            if (textUGUI) textUGUI.text = "A";
        }
        else
        {
            if (img) img.color = new Color(0.6f, 0.2f, 1f); // 보라
            if (textTMP) textTMP.text = "D";
            if (textUGUI) textUGUI.text = "D";
        }

        hasJudged = false;
    }

    void Update()
    {
        if (hasJudged) return;

        rect.anchoredPosition += Vector2.left * speed * Time.deltaTime;

        if (rect.anchoredPosition.x < judgeX - mgr.missWindowPx)
        {
            hasJudged = true;
            mgr.OnJudge(this, Judgement.Miss, true);
            Destroy(gameObject);
        }
    }
}