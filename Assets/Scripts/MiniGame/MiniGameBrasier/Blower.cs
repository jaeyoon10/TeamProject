using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Blower : MonoBehaviour
{
    [Header("UI")]
    public Slider heatGauge;
    public float decreaseSpeed = 0.1f;
    public float increaseAmount = 0.5f;
    public float cooldownTime = 1f;

    [Header("풀무")]
    public GameObject bellows;
    private Renderer bellowsRenderer;
    private Color originalColor;
    private float lastClickTime;

    [Header("숯 (eggs_basket 오브젝트들)")]
    public GameObject[] coalParents;
    private List<Renderer> coals = new List<Renderer>();

    [Header("주괴 (ingot 2개")]
    public Renderer[] ingots;
    private Gradient ingotGradient;

    private Gradient heatGradient;
    private bool gameCompleted = false;

    void Start()
    {
        if (bellows != null)
        {
            bellowsRenderer = bellows.GetComponent<Renderer>();
            if (bellowsRenderer != null)
                originalColor = bellowsRenderer.material.color;
        }

        if (heatGauge != null)
            heatGauge.value = 0f;

        coals.Clear();
        foreach (var parent in coalParents)
        {
            if (!parent) continue;
            Renderer[] childRenderers = parent.GetComponentsInChildren<Renderer>(true);
            coals.AddRange(childRenderers);
        }

        ingotGradient = new Gradient();
        ingotGradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.gray, 0f),
                new GradientColorKey(new Color(1f, 0.6f, 0.6f), 0.3f),
                new GradientColorKey(new Color(0.6f, 0, 0), 0.75f)
            },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );

        heatGradient = new Gradient();
        heatGradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.black, 0f),
                new GradientColorKey(new Color(0.6f,0.5f,0f), 0.3f),
                new GradientColorKey(new Color(0.4f,0f,0f), 0.5f),
                new GradientColorKey(Color.red, 0.75f)
            },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );
    }

    void Update()
    {
        if (gameCompleted) return;

        float t = heatGauge ? heatGauge.normalizedValue : 0f;
        Color coalColor = heatGradient.Evaluate(t);

        foreach (var r in coals)
        {
            if (!r) continue;
            r.material.color = coalColor;

            if (r.material.HasProperty("_EmissionColor"))
            {
                var emiss = Color.Lerp(Color.black, new Color(1f, 0.3f, 0f), t);
                r.material.SetColor("_EmissionColor", emiss * Mathf.LinearToGammaSpace(t * 1.2f));
            }
        }

        Color ingotColor = ingotGradient.Evaluate(t);
        foreach (var ingot in ingots)
        {
            if (!ingot) continue;
            ingot.material.color = ingotColor;

            if (ingot.material.HasProperty("_EmissionColor"))
            {
                var emiss = Color.Lerp(Color.black, new Color(1f, 0.2f, 0.2f), t);
                ingot.material.SetColor("_EmissionColor", emiss * Mathf.LinearToGammaSpace(t * 1.5f));
            }
        }

        if (heatGauge && heatGauge.value >= heatGauge.maxValue)
        {
            heatGauge.value = heatGauge.maxValue;
            gameCompleted = true;

            SaveHeatedIngotState();

            //  완료 신호만 보내고 종료는 상위 매니저가 처리
            StartCoroutine(NotifyDoneAfter(0.2f));
        }

        if (heatGauge && heatGauge.value > 0f)
            heatGauge.value -= decreaseSpeed * Time.deltaTime;
    }

    IEnumerator NotifyDoneAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        MiniGameState.FurnaceDone = true;
    }

    void SaveHeatedIngotState()
    {
        Renderer r = (ingots != null && ingots.Length > 0) ? ingots[0] : null;
        if (!r) { IngotHeatData.Clear(); return; }

        var mat = r.material;
        Color baseCol = mat.color;
        Color emissCol = mat.HasProperty("_EmissionColor") ? mat.GetColor("_EmissionColor") : Color.black;

        float t = heatGauge ? heatGauge.normalizedValue : 0f;
        IngotHeatData.Save(baseCol, emissCol, t);
    }

    void OnMouseEnter()
    {
        if (gameCompleted) return;
        if (bellowsRenderer != null)
            bellowsRenderer.material.color = Color.white;
    }

    void OnMouseExit()
    {
        if (bellowsRenderer != null)
            bellowsRenderer.material.color = originalColor;
    }

    void OnMouseDown()
    {
        if (gameCompleted) return;
        if (Time.time - lastClickTime < cooldownTime) return;
        lastClickTime = Time.time;

        if (heatGauge)
            heatGauge.value = Mathf.Min(heatGauge.maxValue, heatGauge.value + increaseAmount);
    }
}
