using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public GameObject[] coalParents;   // Hierarchy에 있는 eggs_basket 5개를 드래그
    private List<Renderer> coals = new List<Renderer>(); // 내부 egg Renderer 자동 수집

    [Header("주괴 (ingot 2개")]
    public Renderer[] ingots; //화로 안 2개 주괴
    private Gradient ingotGradient;

    private Gradient heatGradient;
    private bool gameCompleted = false;

    void Start()
    {
        // 풀무 강조용 Renderer
        if (bellows != null)
        {
            bellowsRenderer = bellows.GetComponent<Renderer>();
            if (bellowsRenderer != null)
                originalColor = bellowsRenderer.material.color;
        }

        if (heatGauge != null)
            heatGauge.value = 0f;

        // eggs_basket 안에 있는 egg MeshRenderer 전부 수집
        coals.Clear();
        foreach (var parent in coalParents)
        {
            if (parent != null)
            {
                Renderer[] childRenderers = parent.GetComponentsInChildren<Renderer>(true);
                coals.AddRange(childRenderers);
            }
        }
        //주괴 색상 
        ingotGradient = new Gradient();
        ingotGradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.gray, 0f),
                new GradientColorKey(new Color(1f, 0.6f, 0.6f), 0.3f),
                new GradientColorKey(new Color(0.6f, 0, 0), 0.75f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
            );


        // 달궈지는 색상 그라디언트
        heatGradient = new Gradient();
        heatGradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.black, 0f),
                new GradientColorKey(new Color(0.6f,0.5f,0f), 0.3f),// 어두운 주황
                new GradientColorKey(new Color(0.4f,0f,0f), 0.5f),
                new GradientColorKey(Color.red, 0.75f)             // 빨강
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        );
    }

    void Update()
    {
        if (gameCompleted) return;

        // 색상 업데이트 (eggs_basket 안의 모든 egg)
        float t = heatGauge.normalizedValue;
        Color coalColor = heatGradient.Evaluate(t);

        foreach (var r in coals)
        {
            if (r == null) continue;
            r.material.color = coalColor;

            // Emission 효과 (불빛 느낌)
            if (r.material.HasProperty("_EmissionColor"))
            {
                var emiss = Color.Lerp(Color.black, new Color(1f, 0.3f, 0f), t);
                r.material.SetColor("_EmissionColor", emiss * Mathf.LinearToGammaSpace(t * 1.2f));
            }
        }

        Color ingotColor = ingotGradient.Evaluate(t);
        foreach (var ingot in ingots)
        {
            if (ingot != null)
            {
                ingot.material.color = ingotColor;

                if (ingot.material.HasProperty("_EmissionColor"))
                {
                    var emiss = Color.Lerp(Color.black, new Color(1f, 0.2f, 0.2f), t);
                    ingot.material.SetColor("_EmissionColor", emiss * Mathf.LinearToGammaSpace(t * 1.5f));
                }
            }
        }

        // 완료 체크
        if (heatGauge.value >= heatGauge.maxValue)
        {
            heatGauge.value = heatGauge.maxValue;
            gameCompleted = true;
            SaveHeatedIngotState();

            StartCoroutine(EndMiniGameAfterDelay(1f)); // 1초 후 줌 아웃
        }

        // 게이지 자연 감소
        if (heatGauge.value > 0f)
            heatGauge.value -= decreaseSpeed * Time.deltaTime;
    }

    void SaveHeatedIngotState()
    {
        // 대표 주괴 하나 기준(0번). 둘 중 더 뜨거운걸 쓰고 싶으면 t 비교해서 선택해도 됨.
        Renderer r = (ingots != null && ingots.Length > 0) ? ingots[0] : null;
        if (r == null) { IngotHeatData.Clear(); return; }

        // 머티리얼에서 현재 컬러/에미션 뽑기
        var mat = r.material; // 인스턴스 복사
        Color baseCol = mat.color;
        Color emissCol = Color.black;
        if (mat.HasProperty("_EmissionColor"))
            emissCol = mat.GetColor("_EmissionColor");

        float t = (heatGauge != null) ? heatGauge.normalizedValue : 0f;

        IngotHeatData.Save(baseCol, emissCol, t);
    }

    IEnumerator EndMiniGameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        var camTrans = Camera.main.GetComponent<CameraSceneTransition>();
        if (camTrans != null)
            camTrans.StartZoomOut("Ingame_main");
        else
            SceneManager.LoadScene("Ingame_main"); // fallback
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

        heatGauge.value = Mathf.Min(heatGauge.maxValue, heatGauge.value + increaseAmount);
    }
}