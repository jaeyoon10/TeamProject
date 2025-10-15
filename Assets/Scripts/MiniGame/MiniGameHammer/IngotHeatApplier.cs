using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngotHeatApplier : MonoBehaviour
{
    public Renderer target;
    public bool usePropertyBlock = true;
    MaterialPropertyBlock mpb;

    void Awake()
    {
        if (!target) target = GetComponent<Renderer>();
        if (usePropertyBlock) mpb = new MaterialPropertyBlock();
    }

    void OnEnable()
    {
        if (!target || !IngotHeatData.hasData) return;

        foreach (var m in target.sharedMaterials)
        {
            if (!m) continue;
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }

        if (usePropertyBlock && mpb != null)
        {
            target.GetPropertyBlock(mpb);
            mpb.SetColor("_Color", IngotHeatData.baseColor);
            mpb.SetColor("_EmissionColor", IngotHeatData.emissionColor);
            target.SetPropertyBlock(mpb);
        }
        else
        {
            var mat = target.material;
            mat.color = IngotHeatData.baseColor;
            if (mat.HasProperty("_EmissionColor"))
                mat.SetColor("_EmissionColor", IngotHeatData.emissionColor);
        }
    }
}