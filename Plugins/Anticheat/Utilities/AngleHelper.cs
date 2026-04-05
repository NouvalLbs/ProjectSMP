using System;

namespace ProjectSMP.Plugins.Anticheat.Utilities;

public static class AngleHelper
{
    public static float Normalize(float angle)
    {
        angle %= 360f;
        if (angle < 0f) angle += 360f;
        return angle;
    }

    public static float Diff(float a, float b)
    {
        float d = MathF.Abs(Normalize(a) - Normalize(b));
        return d > 180f ? 360f - d : d;
    }
}