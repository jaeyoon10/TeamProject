public static class MiniGameState
{
    public static bool FurnaceDone;
    public static bool HammerDone;
    public static bool GrindingDone;

    public static void ResetAll()
    {
        FurnaceDone = false;
        HammerDone = false;
        GrindingDone = false;
    }
}
