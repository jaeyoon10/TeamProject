using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HammerResultData
{
    public static bool hasValue;
    public static int fails;
    public static int perfect;

    public static void Save(int f, int p)
    {
        fails = f; perfect = p; hasValue = true;
    }

    public static (int fails, int perfect) Consume()
    {
        hasValue = false;
        return (fails, perfect);
    }

    public static void Clear() { hasValue = false; fails = 0; perfect = 0; }
}

public static class PolishResultData
{
    public static bool hasValue;
    public static int fails;

    public static void Save(int f)
    {
        fails = f; hasValue = true;
    }

    public static int Consume()
    {
        hasValue = false;
        return fails;
    }

    public static void Clear() { hasValue = false; fails = 0; }
}