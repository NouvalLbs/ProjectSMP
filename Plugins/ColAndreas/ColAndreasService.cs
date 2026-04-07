using SampSharp.GameMode;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.ColAndreas
{
    public static class ColAndreasService
    {
        public const int WaterObject = 20000;
        public const int MaxMulticastSize = 99;

        private static ColAndreasNatives N => ColAndreasNatives.Instance;

        #region Core Natives

        public static int Init() => N.CA_Init();

        public static int RemoveBuilding(int modelid, float x, float y, float z, float radius)
            => N.CA_RemoveBuilding(modelid, x, y, z, radius);

        public static int RestoreBuilding(int modelid, float x, float y, float z, float radius)
            => N.CA_RestoreBuilding(modelid, x, y, z, radius);

        public static int RayCastLine(float startX, float startY, float startZ, float endX, float endY, float endZ, out float x, out float y, out float z)
            => N.CA_RayCastLine(startX, startY, startZ, endX, endY, endZ, out x, out y, out z);

        public static int RayCastLineID(float startX, float startY, float startZ, float endX, float endY, float endZ, out float x, out float y, out float z)
            => N.CA_RayCastLineID(startX, startY, startZ, endX, endY, endZ, out x, out y, out z);

        public static int RayCastLineExtraID(int type, float startX, float startY, float startZ, float endX, float endY, float endZ, out float x, out float y, out float z)
            => N.CA_RayCastLineExtraID(type, startX, startY, startZ, endX, endY, endZ, out x, out y, out z);

        public static int RayCastMultiLine(float startX, float startY, float startZ, float endX, float endY, float endZ, out float[] retX, out float[] retY, out float[] retZ, out float[] retDist, out int[] modelIds, int size = MaxMulticastSize)
            => N.CA_RayCastMultiLine(startX, startY, startZ, endX, endY, endZ, out retX, out retY, out retZ, out retDist, out modelIds, size);

        public static int RayCastLineAngle(float startX, float startY, float startZ, float endX, float endY, float endZ, out float x, out float y, out float z, out float rx, out float ry, out float rz)
            => N.CA_RayCastLineAngle(startX, startY, startZ, endX, endY, endZ, out x, out y, out z, out rx, out ry, out rz);

        public static int RayCastReflectionVector(float startX, float startY, float startZ, float endX, float endY, float endZ, out float x, out float y, out float z, out float nx, out float ny, out float nz)
            => N.CA_RayCastReflectionVector(startX, startY, startZ, endX, endY, endZ, out x, out y, out z, out nx, out ny, out nz);

        public static int RayCastLineNormal(float startX, float startY, float startZ, float endX, float endY, float endZ, out float x, out float y, out float z, out float nx, out float ny, out float nz)
            => N.CA_RayCastLineNormal(startX, startY, startZ, endX, endY, endZ, out x, out y, out z, out nx, out ny, out nz);

        public static int RayCastLineEx(float startX, float startY, float startZ, float endX, float endY, float endZ, out float x, out float y, out float z, out float rx, out float ry, out float rz, out float rw, out float cx, out float cy, out float cz)
            => N.CA_RayCastLineEx(startX, startY, startZ, endX, endY, endZ, out x, out y, out z, out rx, out ry, out rz, out rw, out cx, out cy, out cz);

        public static int RayCastLineAngleEx(float startX, float startY, float startZ, float endX, float endY, float endZ, out float x, out float y, out float z, out float rx, out float ry, out float rz, out float ocX, out float ocY, out float ocZ, out float orX, out float orY, out float orZ)
            => N.CA_RayCastLineAngleEx(startX, startY, startZ, endX, endY, endZ, out x, out y, out z, out rx, out ry, out rz, out ocX, out ocY, out ocZ, out orX, out orY, out orZ);

        public static int ContactTest(int modelid, float x, float y, float z, float rx, float ry, float rz)
            => N.CA_ContactTest(modelid, x, y, z, rx, ry, rz);

        public static int EulerToQuat(float rx, float ry, float rz, out float x, out float y, out float z, out float w)
            => N.CA_EulerToQuat(rx, ry, rz, out x, out y, out z, out w);

        public static int QuatToEuler(float x, float y, float z, float w, out float rx, out float ry, out float rz)
            => N.CA_QuatToEuler(x, y, z, w, out rx, out ry, out rz);

        public static int GetModelBoundingSphere(int modelid, out float offX, out float offY, out float offZ, out float radius)
            => N.CA_GetModelBoundingSphere(modelid, out offX, out offY, out offZ, out radius);

        public static int GetModelBoundingBox(int modelid, out float minX, out float minY, out float minZ, out float maxX, out float maxY, out float maxZ)
            => N.CA_GetModelBoundingBox(modelid, out minX, out minY, out minZ, out maxX, out maxY, out maxZ);

        public static int SetObjectExtraID(int index, int type, int data)
            => N.CA_SetObjectExtraID(index, type, data);

        public static int GetObjectExtraID(int index, int type)
            => N.CA_GetObjectExtraID(index, type);

        public static int LoadFromDff(int newid, string dffFileName)
            => N.CA_LoadFromDff(newid, dffFileName);

        public static int CreateObject(int modelid, float x, float y, float z, float rx, float ry, float rz, bool add = false)
            => N.CA_CreateObject(modelid, x, y, z, rx, ry, rz, add);

        public static int DestroyObject(int index) => N.CA_DestroyObject(index);

        public static int IsValidObject(int index) => N.CA_IsValidObject(index);

        public static int SetObjectPos(int index, float x, float y, float z)
            => N.CA_SetObjectPos(index, x, y, z);

        public static int SetObjectRot(int index, float rx, float ry, float rz)
            => N.CA_SetObjectRot(index, rx, ry, rz);

        #endregion

        #region Building Removal

        private static readonly int[] BarrierIds =
        {
            4504, 4505, 4506, 4507, 4508, 4509, 4510, 4511, 4512, 4513, 4514, 4515, 4516, 4517,
            4518, 4519, 4520, 4521, 4522, 4523, 4524, 4525, 4526, 4527,
            16436, 16437, 16438, 16439, 1662
        };

        private static readonly int[] BreakableIds =
        {
            625, 626, 627, 628, 629, 630, 631, 632, 633, 642, 643, 644, 646, 650, 716, 717, 737,
            738, 792, 858, 881, 882, 883, 884, 885, 886, 887, 888, 889, 890, 891, 892, 893, 894,
            895, 904, 905, 941, 955, 956, 959, 961, 990, 993, 996, 1209, 1211, 1213, 1219, 1220,
            1221, 1223, 1224, 1225, 1226, 1227, 1228, 1229, 1230, 1231, 1232, 1235, 1238, 1244,
            1251, 1255, 1257, 1262, 1264, 1265, 1270, 1280, 1281, 1282, 1283, 1284, 1285, 1286,
            1287, 1288, 1289, 1290, 1291, 1293, 1294, 1297, 1300, 1302, 1315, 1328, 1329, 1330,
            1338, 1350, 1351, 1352, 1370, 1373, 1374, 1375, 1407, 1408, 1409, 1410, 1411, 1412,
            1413, 1414, 1415, 1417, 1418, 1419, 1420, 1421, 1422, 1423, 1424, 1425, 1426, 1428,
            1429, 1431, 1432, 1433, 1436, 1437, 1438, 1440, 1441, 1443, 1444, 1445, 1446, 1447,
            1448, 1449, 1450, 1451, 1452, 1456, 1457, 1458, 1459, 1460, 1461, 1462, 1463, 1464,
            1465, 1466, 1467, 1468, 1469, 1470, 1471, 1472, 1473, 1474, 1475, 1476, 1477, 1478,
            1479, 1480, 1481, 1482, 1483, 1514, 1517, 1520, 1534, 1543, 1544, 1545, 1551, 1553,
            1554, 1558, 1564, 1568, 1582, 1583, 1584, 1588, 1589, 1590, 1591, 1592, 1645, 1646,
            1647, 1654, 1664, 1666, 1667, 1668, 1669, 1670, 1672, 1676, 1684, 1686, 1775, 1776,
            1949, 1950, 1951, 1960, 1961, 1962, 1975, 1976, 1977, 2647, 2663, 2682, 2683, 2885,
            2886, 2887, 2900, 2918, 2920, 2925, 2932, 2933, 2942, 2943, 2945, 2947, 2958, 2959,
            2966, 2968, 2971, 2977, 2987, 2988, 2989, 2991, 2994, 3006, 3018, 3019, 3020, 3021,
            3022, 3023, 3024, 3029, 3032, 3036, 3058, 3059, 3067, 3083, 3091, 3221, 3260, 3261,
            3262, 3263, 3264, 3265, 3267, 3275, 3276, 3278, 3280, 3281, 3282, 3302, 3374, 3409,
            3460, 3516, 3794, 3795, 3797, 3853, 3855, 3864, 3884, 11103, 12840, 16627, 16628,
            16629, 16630, 16631, 16632, 16633, 16634, 16635, 16636, 16732, 17968
        };

        public static void RemoveBarriers()
        {
            foreach (var id in BarrierIds)
                RemoveBuilding(id, 0f, 0f, 0f, 4242.6407f);
        }

        public static void RemoveBreakableBuildings()
        {
            foreach (var id in BreakableIds)
                RemoveBuilding(id, 0f, 0f, 0f, 4242.6407f);
        }

        #endregion

        #region Helper Methods

        public static bool FindZFor2DCoord(float x, float y, out float z)
        {
            z = 0f;
            return RayCastLine(x, y, 700f, x, y, -1000f, out x, out _, out z) != 0;
        }

        public static bool IsOnSurface(Vector3 position, float tolerance = 1.5f)
            => RayCastLine(position.X, position.Y, position.Z, position.X, position.Y, position.Z - tolerance, out _, out _, out _) != 0;

        public static bool IsInWater(Vector3 pos, out float depth, out float entityDepth)
        {
            depth = 0f;
            entityDepth = 0f;

            int count = RayCastMultiLine(pos.X, pos.Y, pos.Z + 1000f, pos.X, pos.Y, pos.Z - 1000f,
                out _, out _, out float[] retZ, out _, out int[] modelIds, 10);

            if (count <= 0) return false;

            for (int i = 0; i < count; i++)
            {
                if (modelIds[i] != WaterObject) continue;

                float minZ = float.MaxValue;
                for (int j = 0; j < count; j++)
                    if (retZ[j] < minZ) minZ = retZ[j];

                depth = retZ[i] - minZ;
                if (depth is > -0.001f and < 0.001f) depth = 100f;
                entityDepth = retZ[i] - pos.Z;
                return entityDepth >= -2f;
            }

            return false;
        }

        public static bool IsNearWater(Vector3 pos, float dist = 3f, float height = 3f)
        {
            for (int i = 0; i < 6; i++)
            {
                float rad = i * 60f * MathF.PI / 180f;
                float ox = dist * MathF.Sin(rad);
                float oy = dist * MathF.Cos(rad);
                if (RayCastLine(pos.X + ox, pos.Y + oy, pos.Z + height,
                                pos.X + ox, pos.Y + oy, pos.Z - height,
                                out _, out _, out _) == WaterObject)
                    return true;
            }
            return false;
        }

        public static bool IsFacingWater(Vector3 pos, float facingAngle, float dist = 3f, float height = 3f)
        {
            float rad = -facingAngle * MathF.PI / 180f;
            float ox = dist * MathF.Sin(rad);
            float oy = dist * MathF.Cos(rad);
            return RayCastLine(pos.X + ox, pos.Y + oy, pos.Z,
                               pos.X + ox, pos.Y + oy, pos.Z - height,
                               out _, out _, out _) == WaterObject;
        }

        public static bool IsBlocked(Vector3 pos, float facingAngle, float dist = 1.5f, float checkHeight = 0.5f)
        {
            float z = pos.Z - 1f - checkHeight;
            float rad = -facingAngle * MathF.PI / 180f;
            float ex = pos.X + dist * MathF.Sin(rad);
            float ey = pos.Y + dist * MathF.Cos(rad);
            return RayCastLine(pos.X, pos.Y, z, ex, ey, z, out _, out _, out _) != 0;
        }

        public static float GetRoomHeight(Vector3 pos)
        {
            if (RayCastLine(pos.X, pos.Y, pos.Z, pos.X, pos.Y, pos.Z - 1000f, out float fx, out float fy, out float fz) == 0) return 0f;
            if (RayCastLine(pos.X, pos.Y, pos.Z, pos.X, pos.Y, pos.Z + 1000f, out float cx, out float cy, out float cz) == 0) return 0f;
            return MathF.Sqrt((fx - cx) * (fx - cx) + (fy - cy) * (fy - cy) + (fz - cz) * (fz - cz));
        }

        public static float GetRoomCenter(Vector3 pos, out float mx, out float my)
        {
            mx = 0f;
            my = 0f;

            if (RayCastLine(pos.X, pos.Y, pos.Z, pos.X + 1000f, pos.Y, pos.Z, out float pt1x, out float pt1y, out _) == 0) return -1f;
            if (RayCastLine(pos.X, pos.Y, pos.Z,
                    pos.X + 1000f * MathF.Cos(120f * MathF.PI / 180f),
                    pos.Y + 1000f * MathF.Sin(120f * MathF.PI / 180f), pos.Z,
                    out float pt2x, out float pt2y, out _) == 0) return -1f;
            if (RayCastLine(pos.X, pos.Y, pos.Z,
                    pos.X + 1000f * MathF.Cos(-120f * MathF.PI / 180f),
                    pos.Y + 1000f * MathF.Sin(-120f * MathF.PI / 180f), pos.Z,
                    out float pt3x, out float pt3y, out _) == 0) return -1f;

            float xda = pt2x - pt1x, yda = pt2y - pt1y;
            float xdb = pt3x - pt2x, ydb = pt3y - pt2y;

            if (MathF.Abs(xda) <= 1e-9f && MathF.Abs(ydb) <= 1e-9f)
            {
                mx = 0.5f * (pt2x + pt3x);
                my = 0.5f * (pt1y + pt2y);
                float ddx = mx - pt1x, ddy = my - pt1y;
                return MathF.Sqrt(ddx * ddx + ddy * ddy);
            }

            float aSlope = yda / xda;
            float bSlope = ydb / xdb;
            if (MathF.Abs(aSlope - bSlope) <= 1e-9f) return -1f;

            mx = (aSlope * bSlope * (pt1y - pt3y) + bSlope * (pt1x + pt2x) - aSlope * (pt2x + pt3x)) / (2f * (bSlope - aSlope));
            my = -1f * (mx - (pt1x + pt2x) / 2f) / aSlope + (pt1y + pt2y) / 2f;

            float dx = mx - pt1x, dy = my - pt1y;
            return MathF.Sqrt(dx * dx + dy * dy);
        }

        public static List<(float x, float y, float z)> RayCastExplode(float cx, float cy, float cz, float radius, float intensity = 20f)
        {
            var results = new List<(float, float, float)>();
            if (intensity < 1f || intensity > 360f) return results;

            for (float lat = -180f; lat < 180f; lat += intensity * 0.75f)
            {
                for (float lon = -90f; lon < 90f; lon += intensity)
                {
                    float latR = lat * MathF.PI / 180f;
                    float lonR = lon * MathF.PI / 180f;
                    float ox = -radius * MathF.Cos(latR) * MathF.Cos(lonR);
                    float oy = radius * MathF.Cos(latR) * MathF.Sin(lonR);
                    float oz = radius * MathF.Sin(latR);
                    if (RayCastLine(cx, cy, cz, cx + ox, cy + oy, cz + oz, out float rx, out float ry, out float rz) != 0)
                        results.Add((rx, ry, rz));
                }
            }

            return results;
        }

        #endregion
    }
}