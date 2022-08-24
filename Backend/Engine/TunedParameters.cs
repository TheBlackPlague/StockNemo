namespace Backend.Engine;

public static class TunedParameters
{

    public static int AspirationDepth = 4;
    public static int AspirationSize = 16;
    public static int AspirationDelta = 23;
    public static int ReverseFutilityDepthThreshold = 7;
    public static int ReverseFutilityD = 67;
    public static int ReverseFutilityI = 76;
    public static int RazoringEvaluationThreshold = 150;
    public static int NullMoveDepth = 2;
    public static int NullMoveReduction = 4;
    public static int NullMoveScalingFactor = 3;
    public static int NullMoveScalingCorrection = 1;
    public static int LmpDepthThreshold = 3;
    public static int LmrDepthThreshold = 3;
    public static int LmrFullSearchThreshold = 4;

}