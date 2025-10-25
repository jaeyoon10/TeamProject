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
    public float cooldownTime = 0.5f;

    [Header("풀무")]
    public GameObject bellows;
    private Renderer bellowsRenderer;
    private Color originalColor;
    private float lastClickTime;

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioSource fireLoopSource;

    public AudioClip bellowsSound;
    public AudioClip fireSound;

    [Range(0f, 1f)] public float bellowsVolume = 1.0f;
    [Range(0f, 1f)] public float fireVolumeMax = 0.6f;
    [Range(0.5f, 1.2f)] public float bellowsPitchJitter = 0.08f;

    public float fireStartThreshold = 0.5f;

    public float fireStopThreshold = 0.45f;

    private bool fireLoopStarted = false;

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
        if (bellowsSound == null) bellowsSound = Resources.Load<AudioClip>("bellows-sound");
        if (fireSound == null) fireSound = Resources.Load<AudioClip>("fire-sound");

        if (fireSound != null)
        {
            if (fireLoopSource.clip == null && fireSound != null) 
                fireLoopSource.clip = fireSound;

            fireLoopSource.loop = true;
            fireLoopSource.playOnAwake = false;
            fireLoopSource.volume = 0f;
        }
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

        if (fireLoopSource != null)
        {
            // 시작 조건: 50% 이상 & 아직 시작 안함
            if (!fireLoopStarted && t >= fireStartThreshold && fireSound != null)
            {
                if (fireLoopSource.clip == null) fireLoopSource.clip = fireSound;
                fireLoopSource.volume = 0f;
                fireLoopSource.Play();
                fireLoopStarted = true;
            }
            // 정지 조건: 45% 미만으로 떨어지면 멈춤
            if (fireLoopStarted && t < fireStopThreshold)
            {
                fireLoopSource.Stop();
                fireLoopStarted = false;
            }

            //  게이지에 따라 볼륨 살짝 올려주기
            if (fireLoopStarted && fireLoopSource.isPlaying)
            {
                // 0.5~1.0 -> 0~1로 리매핑
                float fireT = Mathf.InverseLerp(fireStartThreshold, 1f, t);
                float targetVol = Mathf.Lerp(0.15f, fireVolumeMax, fireT);
                fireLoopSource.volume = Mathf.MoveTowards(fireLoopSource.volume, targetVol, Time.deltaTime * 1.5f);
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

        if (sfxSource != null && bellowsSound != null)
        {
            sfxSource.pitch = 1f + Random.Range(-bellowsPitchJitter, bellowsPitchJitter);
            sfxSource.PlayOneShot(bellowsSound, bellowsVolume);
        }
    }

}
