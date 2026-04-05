using System;

namespace ProjectSMP.Plugins.Anticheat.Utilities;

public static class VectorMath
{
    public static float Dist(float x1, float y1, float z1, float x2, float y2, float z2)
        => MathF.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1) + (z2 - z1) * (z2 - z1));

    public static float Dist2D(float x1, float y1, float x2, float y2)
        => MathF.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));

    public static float Speed(float vx, float vy, float vz)
        => MathF.Sqrt(vx * vx + vy * vy + vz * vz);

    public static float ElevationAngle(float w, float x, float y, float z)
    {
        float horiz = MathF.Sqrt(x * x + y * y);
        return horiz == 0f ? 0f : MathF.Atan2(z - w, horiz) * (180f / MathF.PI);
    }
}