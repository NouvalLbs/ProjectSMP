using System;

namespace ProjectSMP.Plugins.Anticheat.Data;

public static class VehicleData
{
    public static readonly byte[] Type =
    {
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,
        0,0,0,0,0,2,0,0,0,0,3,6,0,0,0,0,0,6,0,0,
        0,10,0,0,0,0,3,2,5,7,0,0,3,3,3,0,0,0,0,0,
        1,5,5,5,9,9,0,0,5,2,0,5,3,3,0,0,1,0,0,0,
        0,4,0,0,3,0,0,2,2,0,0,0,0,3,0,0,0,2,0,0,
        0,9,0,0,0,0,0,0,0,4,4,1,1,1,0,0,0,0,0,1,
        1,5,5,5,0,0,0,0,0,0,0,0,0,0,0,0,0,7,7,1,
        0,0,0,0,0,0,0,0,2,0,0,0,0,1,0,0,0,0,0,0,
        0,0,0,2,10,0,0,0,0,8,8,0,0,0,0,0,0,1,0,0,
        0,5,0,0,0,0,5,0,0,0,0,0,1,1,10,3,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0
    };

    private static readonly uint[] _maxPassengersPacked =
    {
        0x10331113,0x11311131,0x11331313,0x80133301,0x1381F110,
        0x10311103,0x10001F10,0x11113311,0x13113311,0x31101100,
        0x30002301,0x11031311,0x11111331,0x10013111,0x01131100,
        0x11111110,0x11100031,0x11130111,0x33113311,0x11111101,
        0x33101133,0x10100510,0x03133111,0xFF11113F,0x13330111,
        0xFF131111,0x0000FF3F
    };

    public static int GetMaxPassengers(int modelId)
    {
        int idx = modelId - 400;
        if ((uint)idx >= 212u) return 0;
        int wordIdx = idx / 8, nibble = idx % 8;
        if ((uint)wordIdx >= (uint)_maxPassengersPacked.Length) return 0;
        return (int)((_maxPassengersPacked[wordIdx] >> (nibble * 4)) & 0xF);
    }

    public static byte GetType(int modelId)
    {
        int idx = modelId - 400;
        return (uint)idx < (uint)Type.Length ? Type[idx] : (byte)0;
    }

    public static bool IsBike(int modelId) => GetType(modelId) == 5;
    public static bool IsMotorcycle(int modelId) => GetType(modelId) == 6;
    public static bool IsBoat(int modelId) => GetType(modelId) == 1;
    public static bool IsHelicopter(int modelId) => GetType(modelId) == 3;
    public static bool IsAircraft(int modelId) => GetType(modelId) == 4;
    public static bool IsTrailer(int modelId) {
        return modelId is 435 or 450 or 569 or 570 or 584 or 590 or 591 or 606 or 607 or 608 or 610 or 611;
    }
    public static bool IsRC(int modelId) => GetType(modelId) == 8;
    public static bool IsTrain(int m) => m is 537 or 538 or 569 or 570 or 590 or 591;
    public static bool IsRemoteControl(int m) => m is 441 or 464 or 465 or 501 or 564 or 594;
    public static bool IsBus(int m) => m is 431 or 437 or 470 or 482 or 483 or 508 or 515 or 532 or 539;

    public static readonly (float X1, float Y1, float Z1, float X2, float Y2, float Z2)[] PayNSpray =
    {
        (2056.6f,-1835.9f,12.5443f,2071.3f,-1826.97f,18.5443f),
        (-2430.13f,1013.71f,49.3413f,-2421.2f,1027.76f,55.3413f),
        (-1424.11f,2576.61f,54.8156f,-1416.85f,2590.84f,60.8747f),
        (481.972f,-1747.55f,9.45516f,492.717f,-1735.77f,17.565f),
        (1021.81f,-1029.53f,30.9081f,1027.93f,-1018.71f,36.9081f),
        (-1908.93f,277.989f,40.0413f,-1900.11f,292.353f,45.539f),
        (1968.23f,2157.88f,9.59696f,1983.32f,2167.03f,16.2367f),
        (2389.6f,1483.26f,9.81843f,2398.11f,1497.84f,15.6841f),
        (715.806f,-462.403f,14.9635f,724.293f,-447.29f,21.4398f),
        (-103.636f,1112.42f,18.7017f,-96.3613f,1125.79f,24.5489f)
    };

    public static bool IsInPayNSpray(float x, float y, float z)
    {
        foreach (var (x1, y1, z1, x2, y2, z2) in PayNSpray)
            if (x >= x1 && x <= x2 && y >= y1 && y <= y2 && z >= z1 && z <= z2) return true;
        return false;
    }

    // Restaurants: Cluckin' Bell, Burger Shot, Well Stacked Pizza, Pizza Stack
    public static readonly (float X, float Y, float Z, float Radius)[] Restaurants =
    {
        // Cluckin' Bell (LS exterior)
        (365.512f,-1494.441f,30.852f,5f),
        // Cluckin' Bell (SF)
        (-2038.588f,139.757f,28.836f,5f),
        // Cluckin' Bell (LV)
        (2369.399f,2059.578f,10.820f,5f),
        // Cluckin' Bell interior (0 interior)
        (366.759f,-1494.441f,30.875f,5f),
        // Burger Shot (LS)
        (2461.172f,-1651.444f,13.343f,5f),
        (1197.219f,-1407.488f,14.150f,5f),
        // Burger Shot (SF)
        (-2088.203f,-422.324f,35.320f,5f),
        // Burger Shot (LV)
        (2370.104f,2064.372f,10.820f,5f),
        // Well Stacked Pizza (LS)
        (1708.839f,-1891.285f,13.376f,5f),
        (1959.727f,-1721.367f,13.376f,5f),
        // Well Stacked Pizza (SF)
        (-2154.093f,641.695f,35.672f,5f),
        // Well Stacked Pizza (LV)
        (2381.010f,2009.048f,10.820f,5f),
        // Pizza Stack (LS)
        (382.575f,-192.987f,1008.382f,5f),
        // Restaurants interior shared
        (367.472f,-1494.441f,30.875f,6f),
        (2461.172f,-1651.444f,13.343f,6f),
    };

    public static bool IsNearRestaurant(float x, float y, float z)
    {
        foreach (var (rx, ry, rz, r) in Restaurants)
        {
            float r2 = r * r;
            if ((x - rx) * (x - rx) + (y - ry) * (y - ry) + (z - rz) * (z - rz) < r2) return true;
        }
        return false;
    }

    public static readonly (float X, float Y, float Z, float Radius)[] Casinos =
    {
        (2230.5703f,1617.1563f,1006.2266f,8f),(2241.4453f,1617.1094f,1006.2266f,8f),
        (2242.3672f,1592.2578f,1006.2266f,10f),(2230.5703f,1592.2578f,1006.2266f,10f),
        (2241.3125f,1604.4375f,1006.1563f,10f),(2230.375f,1604.4531f,1006.1563f,10f),
        (2218.6641f,1588.3381f,1006.7656f,4f),(2218.6641f,1592.6428f,1006.7656f,4f),
        (2217.2834f,1603.9297f,1006.7656f,4f),(2220.9397f,1603.9297f,1006.7656f,4f),
        (2218.6641f,1614.4866f,1006.7656f,4f),(2218.6641f,1618.8225f,1006.7656f,4f),
        (2255.1875f,1609.8616f,1006.7656f,4f),(2255.1875f,1613.9084f,1006.7656f,4f),
        (2255.1875f,1617.8069f,1006.7656f,4f),(2269.51f,1606.6484f,1006.7656f,4f),
        (2273.5569f,1606.6484f,1006.7656f,4f),(2252.0313f,1586.1619f,1006.1563f,2f),
        (2261.6328f,1586.1697f,1006.1563f,2f),(2271.7266f,1586.1619f,1006.1563f,2f),
        (2253.7144f,1589.7891f,1006.0156f,6f),(2258.7378f,1589.7891f,1006.0156f,6f),
        (2264.1363f,1589.7891f,1006.0156f,6f),(2269.1988f,1589.7891f,1006.0156f,6f),
        (2273.9409f,1589.7891f,1006.0156f,6f),(2253.7144f,1596.4844f,1006.0156f,6f),
        (2258.7378f,1596.4844f,1006.0156f,6f),(2264.1363f,1596.4844f,1006.0156f,6f),
        (2269.1988f,1596.4844f,1006.0156f,6f),(2273.9409f,1596.4844f,1006.0156f,6f),
        (1961.5484f,1010.1172f,992.5078f,10f),(1961.3472f,1017.9141f,992.4688f,10f),
        (1961.5484f,1025.6953f,992.5078f,10f),(1957.5753f,987.4253f,992.9844f,4f),
        (1962.1809f,992.035f,992.9844f,4f),(1964.8303f,998.3747f,992.9844f,4f),
        (1957.4803f,1048.1866f,992.9844f,4f),(1962.1519f,1043.4894f,992.9844f,4f),
        (1964.8169f,1037.3113f,992.9844f,4f),(1936.3069f,986.625f,992.4688f,2f),
        (1940.6875f,990.9119f,992.4688f,2f),(1944.9588f,986.5234f,992.4688f,2f),
        (1941.5028f,1006.3394f,992.3125f,6f),(1940.3553f,1014.2188f,992.3125f,6f),
        (1940.3553f,1021.4141f,992.3125f,6f),(1941.1947f,1029.3028f,992.3125f,6f),
        (1968.0663f,1006.3438f,992.3125f,6f),(1968.0663f,1014.0f,992.3125f,6f),
        (1968.0663f,1021.6875f,992.3125f,6f),(1968.0663f,1029.6641f,992.3125f,6f),
        (1125.1484f,1.4687f,1000.5781f,2f),(1125.1406f,-4.9141f,1000.5781f,2f),
        (1128.5781f,-1.6797f,1000.5781f,2f),(1118.6012f,-1.6484f,1000.5781f,2f),
        (1135.0469f,-3.0781f,1000.5234f,3f),(1133.6875f,-1.625f,1000.5234f,3f),
        (1135.0f,-0.1797f,1000.5234f,3f),(1125.3203f,3.7969f,1000.5234f,3f),
        (1127.3828f,3.7969f,1000.5234f,3f)
    };

    public static bool IsNearCasino(float x, float y, float z, float extraRange = 0f)
    {
        foreach (var (cx, cy, cz, r) in Casinos)
        {
            float range = r + extraRange;
            if (MathF.Abs(z - cz) < range + 2f && (x - cx) * (x - cx) + (y - cy) * (y - cy) < range * range) return true;
        }
        return false;
    }

    public static readonly (float X, float Y, float Z, float Radius)[] Hospitals =
    {
        (1174.7f,-1323.7f,13.6f,10f),   // LS Hospital
        (-2642.4f,631.9f,14.1f,10f),    // SF Hospital
        (2013.6f,1916.9f,12.3f,10f),    // LV Hospital
        (1240.9f,-795.7f,1084.2f,10f),  // Verdant Meadows
    };

    public static bool IsNearHospital(float x, float y, float z)
    {
        foreach (var (hx, hy, hz, r) in Hospitals)
        {
            float r2 = r * r;
            if ((x - hx) * (x - hx) + (y - hy) * (y - hy) + (z - hz) * (z - hz) < r2) return true;
        }
        return false;
    }
}