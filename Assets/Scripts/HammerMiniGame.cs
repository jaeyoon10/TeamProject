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
    public UIArc arcPrefab;                   // UIArc ������ ����
    public int minZones = 1, maxZones = 1;    // ��ũ ���� (������ 1)
    public float rotationSpeed = 180f;

    [Tooltip("���� ���� ũ�� (����)")]
    public float perfectZoneSize = 10f;
    public float greatZoneSize = 30f;
    public float goodZoneSize = 60f;

    private float currentAngle;
    private List<UIArc> arcs = new List<UIArc>();
    private List<float> zoneAngles = new List<float>();
    private bool isChecking;

    void Start()
    {
        feedbackText.alpha = 0f;
        SetupPointer();
        SpawnZones();
    }

    void Update()
    {
        // ������ ȸ��
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
        // ���� ����
        foreach (var a in arcs) if (a) Destroy(a.gameObject);
        arcs.Clear();
        zoneAngles.Clear();

        int count = Random.Range(minZones, maxZones + 1);
        for (int i = 0; i < count; i++)
        {
            // 1) UIArc �ν��Ͻ� ����
            var a = Instantiate(arcPrefab, zoneContainer);
            a.color = Color.white;      // �⺻ ����
            a.radius = zoneContainer.rect.height * 0.5f;
            a.thickness = pointer.sizeDelta.y;
            a.segments = 60;

            // 2) ���� �߽� ����
            float center = Random.Range(0f, 360f);
            zoneAngles.Add(center);

            // 3) Perfect / Great / Good / Miss 4�ܰ� ǥ��
            //    Perfect
            a.startAngle = center - perfectZoneSize * 0.5f;
            a.endAngle = center + perfectZoneSize * 0.5f;
            a.color = Color.white;
            arcs.Add(a);
        }
    }

    IEnumerator CheckTimingAndFeedback()
    {
        isChecking = true;

        // ������ ���� (currentAngle�� ����)
        float pointerAng = currentAngle;

        // ���� ����� �� ã��
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
        if (rel <= perfectZoneSize * 0.5f) { res = "Perfect"; col = Color.white; }
        else if (rel <= greatZoneSize * 0.5f) { res = "Great"; col = Color.yellow; }
        else if (rel <= goodZoneSize * 0.5f) { res = "Good"; col = Color.green; }
        else { res = "Miss"; col = Color.red; }

        feedbackText.text = res;
        feedbackText.color = col;
        feedbackText.alpha = 1f;

        Debug.Log($"[DEBUG] Pointer:{pointerAng:F1}�� HitZone:{zoneAngles[idx]:F1}�� ��{rel:F1}�ơ�{res}");

        yield return new WaitForSeconds(0.6f);

        // Fade out
        float t = 0f, f = 0.3f;
        while (t < f)
        {
            t += Time.deltaTime;
            feedbackText.alpha = Mathf.Lerp(1f, 0f, t / f);
            yield return null;
        }

        isChecking = false;
        SpawnZones();
    }
}