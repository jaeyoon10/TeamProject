using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerGlow : MonoBehaviour
{
    [Header("Renderers (비우면 자식 포함 자동 검색)")]
    public Renderer[] renderers;

    [Header("Pulse Settings")]
    public Color glowColor = new Color(1f, 0.95f, 0.6f); // 따뜻한 빛
    public float minIntensity = 0f;   // 0~8 정도
    public float maxIntensity = 3f;
    public float speed = 2f;

    private readonly List<MaterialPropertyBlock> _blocks = new List<MaterialPropertyBlock>();
    private bool _running;

    void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>(true);

        // 렌더러 각각에 MPB 준비 + Emission 키워드 강제 활성
        _blocks.Clear();
        foreach (var r in renderers)
        {
            if (!r) continue;
            var mpb = new MaterialPropertyBlock();
            r.GetPropertyBlock(mpb);

            // Emission 키워드 On (Standard/URP 공통)
            foreach (var m in r.sharedMaterials)
            {
                if (!m) continue;
                m.EnableKeyword("_EMISSION");
                // 일부 파이프라인 대비: 베이크용 플래그
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }

            _blocks.Add(mpb);
        }
    }

    void OnEnable()
    {
        _running = true;
        StartCoroutine(CoPulse());
    }

    void OnDisable()
    {
        _running = false;
        // 끌 때 원상복구
        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (!r) continue;
            var mpb = _blocks[i];
            mpb.SetColor("_EmissionColor", Color.black);
            r.SetPropertyBlock(mpb);
        }
    }

    IEnumerator CoPulse()
    {
        while (_running)
        {
            float t = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f; // 0~1
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
            Color emiss = glowColor * intensity;

            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (!r) continue;
                var mpb = _blocks[i];
                // 표준/URP 대부분 "_EmissionColor" 사용
                mpb.SetColor("_EmissionColor", emiss);
                r.SetPropertyBlock(mpb);
            }
            yield return null;
        }
    }

    public void StopGlow()
    {
        enabled = false; // OnDisable에서 정리됨
    }
}