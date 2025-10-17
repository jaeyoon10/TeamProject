using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct GradeCounts
{
    public int perfect, great, good, miss;

    public static GradeCounts operator +(GradeCounts a, GradeCounts b)
        => new GradeCounts
        {
            perfect = a.perfect + b.perfect,
            great = a.great + b.great,
            good = a.good + b.good,
            miss = a.miss + b.miss
        };
}
public static class HammerResultData
{
    public static bool hasValue;
    public static GradeCounts counts;

    public static void Save(int perfect, int great, int good, int miss)
    {
        counts.perfect = perfect;
        counts.great = great;
        counts.good = good;
        counts.miss = miss;
        hasValue = true;
        Debug.Log($"[HammerResultData.Save] P:{perfect} G:{great} D:{good} M:{miss}");
    }

    public static GradeCounts Consume()
    {
        hasValue = false;
        Debug.Log($"[HammerResultData.Consume] -> P:{counts.perfect} G:{counts.great} D:{counts.good} M:{counts.miss}");
        return counts;
    }

    public static void Clear() { hasValue = false; counts = default; }
}

// 그라인딩(연마) 결과 저장소
public static class GrindingResultData
{
    public static bool hasValue;
    public static GradeCounts counts;

    public static void Save(int perfect, int great, int good, int miss)
    {
        counts.perfect = perfect;
        counts.great = great;
        counts.good = good;
        counts.miss = miss;
        hasValue = true;
        Debug.Log($"[GrindingResultData.Save] P:{perfect} G:{great} D:{good} M:{miss}");
    }

    public static GradeCounts Consume()
    {
        hasValue = false;
        Debug.Log($"[GrindingResultData.Consume] -> P:{counts.perfect} G:{counts.great} D:{counts.good} M:{counts.miss}");
        return counts;
    }

    public static void Clear() { hasValue = false; counts = default; }
}