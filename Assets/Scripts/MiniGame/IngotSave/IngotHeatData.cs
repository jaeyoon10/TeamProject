using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngotHeatData
{
    public static bool hasData;
    public static Color baseColor;       // 재질 color
    public static Color emissionColor;   // _EmissionColor
    public static float heatT;           // 0~1 (원하면 사용)

    public static void Save(Color baseCol, Color emissCol, float t01)
    {
        hasData = true;
        baseColor = baseCol;
        emissionColor = emissCol;
        heatT = Mathf.Clamp01(t01);
    }

    public static void Clear()
    {
        hasData = false;
    }
}