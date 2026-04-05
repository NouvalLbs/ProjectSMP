#nullable enable
using SampSharp.GameMode;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.WeaponConfig
{
    internal readonly struct VendingMachineEntry
    {
        public readonly int Model;
        public readonly int Interior;
        public readonly Vector3 Position;
        public readonly Vector3 Rotation;
        public readonly float FrontX;
        public readonly float FrontY;

        public VendingMachineEntry(int model, int interior,
            float x, float y, float z,
            float rx, float ry, float rz,
            float frontX, float frontY)
        {
            Model = model;
            Interior = interior;
            Position = new Vector3(x, y, z);
            Rotation = new Vector3(rx, ry, rz);
            FrontX = frontX;
            FrontY = frontY;
        }
    }

    internal static class WeaponConfigVendingMachines
    {
        private const float RemoveRadius = 0.5f;
        private const float UseRadius = 2.0f;
        private const int CooldownMs = 60_000;
        private const float DefaultHealth = 5f;

        private static bool _enabled;
        private static readonly List<GlobalObject> _objects = new();
        private static readonly Dictionary<int, int> _lastUsed = new();
        private static readonly HashSet<int> _removedForPlayers = new();

        public static event EventHandler<VendingMachineArgs>? PlayerUseVendingMachine;

        private static readonly VendingMachineEntry[] Machines =
        {
            new(955,  0,    -862.82f,  1536.60f,   21.98f, 0f, 0f, 180f,  -862.84f,  1537.60f),
            new(956,  0,    2271.72f,   -76.46f,   25.96f, 0f, 0f,   0f,  2271.72f,   -77.46f),
            new(955,  0,    1277.83f,   372.51f,   18.95f, 0f, 0f,  64f,  1278.73f,   372.07f),
            new(956,  0,     662.42f,  -552.16f,   15.71f, 0f, 0f, 180f,   662.41f,  -551.16f),
            new(955,  0,     201.01f,  -107.61f,    0.89f, 0f, 0f, 270f,   200.01f,  -107.63f),
            new(955,  0,    -253.74f,  2597.95f,   62.24f, 0f, 0f,  90f,  -252.74f,  2597.95f),
            new(956,  0,    -253.74f,  2599.75f,   62.24f, 0f, 0f,  90f,  -252.74f,  2599.75f),
            new(956,  0,     -76.03f,  1227.99f,   19.12f, 0f, 0f,  90f,   -75.03f,  1227.99f),
            new(955,  0,     -14.70f,  1175.35f,   18.95f, 0f, 0f, 180f,   -14.72f,  1176.35f),
            new(1977, 7,     316.87f,  -140.35f,  998.58f, 0f, 0f, 270f,   315.87f,  -140.36f),
            new(1775, 17,    373.82f,  -178.14f, 1000.73f, 0f, 0f,   0f,   373.82f,  -179.14f),
            new(1776, 17,    379.03f,  -178.88f, 1000.73f, 0f, 0f, 270f,   378.03f,  -178.90f),
            new(1775, 17,    495.96f,   -24.32f, 1000.73f, 0f, 0f, 180f,   495.95f,   -23.32f),
            new(1776, 17,    500.56f,    -1.36f, 1000.73f, 0f, 0f,   0f,   500.56f,    -2.36f),
            new(1775, 17,    501.82f,    -1.42f, 1000.73f, 0f, 0f,   0f,   501.82f,    -2.42f),
            new(956,  0,   -1455.11f,  2591.66f,   55.23f, 0f, 0f, 180f, -1455.13f,  2592.66f),
            new(955,  0,    2352.17f, -1357.15f,   23.77f, 0f, 0f,  90f,  2353.17f, -1357.15f),
            new(955,  0,    2325.97f, -1645.13f,   14.21f, 0f, 0f,   0f,  2325.97f, -1646.13f),
            new(956,  0,    2139.51f, -1161.48f,   23.35f, 0f, 0f,  87f,  2140.51f, -1161.53f),
            new(956,  0,    2153.23f, -1016.14f,   62.23f, 0f, 0f, 127f,  2154.03f, -1015.54f),
            new(955,  0,    1928.73f, -1772.44f,   12.94f, 0f, 0f,  90f,  1929.73f, -1772.44f),
            new(1776, 1,    2222.36f,  1602.64f, 1000.06f, 0f, 0f,  90f,  2223.36f,  1602.64f),
            new(1775, 1,    2222.20f,  1606.77f, 1000.05f, 0f, 0f,  90f,  2223.20f,  1606.77f),
            new(1775, 1,    2155.90f,  1606.77f, 1000.05f, 0f, 0f,  90f,  2156.90f,  1606.77f),
            new(1775, 1,    2209.90f,  1607.19f, 1000.05f, 0f, 0f, 270f,  2208.90f,  1607.17f),
            new(1776, 1,    2155.84f,  1607.87f, 1000.06f, 0f, 0f,  90f,  2156.84f,  1607.87f),
            new(1776, 1,    2202.45f,  1617.00f, 1000.06f, 0f, 0f, 180f,  2202.43f,  1618.00f),
            new(1776, 1,    2209.24f,  1621.21f, 1000.06f, 0f, 0f,   0f,  2209.24f,  1620.21f),
            new(1776, 3,     330.67f,   178.50f, 1020.07f, 0f, 0f,   0f,   330.67f,   177.50f),
            new(1776, 3,     331.92f,   178.50f, 1020.07f, 0f, 0f,   0f,   331.92f,   177.50f),
            new(1776, 3,     350.90f,   206.08f, 1008.47f, 0f, 0f,  90f,   351.90f,   206.08f),
            new(1776, 3,     361.56f,   158.61f, 1008.47f, 0f, 0f, 180f,   361.54f,   159.61f),
            new(1776, 3,     371.59f,   178.45f, 1020.07f, 0f, 0f,   0f,   371.59f,   177.45f),
            new(1776, 3,     374.89f,   188.97f, 1008.47f, 0f, 0f,   0f,   374.89f,   187.97f),
            new(1775, 2,    2576.70f, -1284.43f, 1061.09f, 0f, 0f, 270f,  2575.70f, -1284.44f),
            new(1775, 15,   2225.20f, -1153.42f, 1025.90f, 0f, 0f, 270f,  2224.20f, -1153.43f),
            new(955,  0,    1154.72f, -1460.89f,   15.15f, 0f, 0f, 270f,  1153.72f, -1460.90f),
            new(956,  0,    2480.85f, -1959.27f,   12.96f, 0f, 0f, 180f,  2480.84f, -1958.27f),
            new(955,  0,    2060.11f, -1897.64f,   12.92f, 0f, 0f,   0f,  2060.11f, -1898.64f),
            new(955,  0,    1729.78f, -1943.04f,   12.94f, 0f, 0f,   0f,  1729.78f, -1944.04f),
            new(956,  0,    1634.10f, -2237.53f,   12.89f, 0f, 0f,   0f,  1634.10f, -2238.53f),
            new(955,  0,    1789.21f, -1369.26f,   15.16f, 0f, 0f, 270f,  1788.21f, -1369.28f),
            new(956,  0,   -2229.18f,   286.41f,   34.70f, 0f, 0f, 180f, -2229.20f,   287.41f),
            new(955,  256, -1980.78f,   142.66f,   27.07f, 0f, 0f, 270f, -1981.78f,   142.64f),
            new(955,  256, -2118.96f,  -423.64f,   34.72f, 0f, 0f, 255f, -2119.93f,  -423.40f),
            new(955,  256, -2118.61f,  -422.41f,   34.72f, 0f, 0f, 255f, -2119.58f,  -422.17f),
            new(955,  256, -2097.27f,  -398.33f,   34.72f, 0f, 0f, 180f, -2097.29f,  -397.33f),
            new(955,  256, -2092.08f,  -490.05f,   34.72f, 0f, 0f,   0f, -2092.08f,  -491.05f),
            new(955,  256, -2063.27f,  -490.05f,   34.72f, 0f, 0f,   0f, -2063.27f,  -491.05f),
            new(955,  256, -2005.64f,  -490.05f,   34.72f, 0f, 0f,   0f, -2005.64f,  -491.05f),
            new(955,  256, -2034.46f,  -490.05f,   34.72f, 0f, 0f,   0f, -2034.46f,  -491.05f),
            new(955,  256, -2068.56f,  -398.33f,   34.72f, 0f, 0f, 180f, -2068.58f,  -397.33f),
            new(955,  256, -2039.85f,  -398.33f,   34.72f, 0f, 0f, 180f, -2039.86f,  -397.33f),
            new(955,  256, -2011.14f,  -398.33f,   34.72f, 0f, 0f, 180f, -2011.15f,  -397.33f),
            new(955,  2048,-1350.11f,   492.28f,   10.58f, 0f, 0f,  90f, -1349.11f,   492.28f),
            new(956,  2048,-1350.11f,   493.85f,   10.58f, 0f, 0f,  90f, -1349.11f,   493.85f),
            new(955,  0,    2319.99f,  2532.85f,   10.21f, 0f, 0f,   0f,  2319.99f,  2531.85f),
            new(956,  0,    2845.72f,  1295.04f,   10.78f, 0f, 0f,   0f,  2845.72f,  1294.04f),
            new(955,  0,    2503.14f,  1243.69f,   10.21f, 0f, 0f, 180f,  2503.12f,  1244.69f),
            new(956,  0,    2647.69f,  1129.66f,   10.21f, 0f, 0f,   0f,  2647.69f,  1128.66f),
            new(1209, 0,   -2420.21f,   984.57f,   44.29f, 0f, 0f,  90f, -2419.21f,   984.57f),
            new(1302, 0,   -2420.17f,   985.94f,   44.29f, 0f, 0f,  90f, -2419.17f,   985.94f),
            new(955,  0,    2085.77f,  2071.35f,   10.45f, 0f, 0f,  90f,  2086.77f,  2071.35f),
            new(956,  0,    1398.84f,  2222.60f,   10.42f, 0f, 0f, 180f,  1398.82f,  2223.60f),
            new(956,  0,    1659.46f,  1722.85f,   10.21f, 0f, 0f,   0f,  1659.46f,  1721.85f),
            new(955,  0,    1520.14f,  1055.26f,   10.00f, 0f, 0f, 270f,  1519.14f,  1055.24f),
            new(1775, 6,     -19.03f,   -57.83f, 1003.63f, 0f, 0f, 180f,   -19.05f,   -56.83f),
            new(1775, 18,    -16.11f,   -91.64f, 1003.63f, 0f, 0f, 180f,   -16.13f,   -90.64f),
            new(1775, 16,    -15.10f,  -140.22f, 1003.63f, 0f, 0f, 180f,   -15.11f,  -139.22f),
            new(1775, 17,    -32.44f,  -186.69f, 1003.63f, 0f, 0f, 180f,   -32.46f,  -185.69f),
            new(1775, 16,    -35.72f,  -140.22f, 1003.63f, 0f, 0f, 180f,   -35.74f,  -139.22f),
            new(1776, 6,     -36.14f,   -57.87f, 1003.63f, 0f, 0f, 180f,   -36.16f,   -56.87f),
            new(1776, 18,    -17.54f,   -91.71f, 1003.63f, 0f, 0f, 180f,   -17.56f,   -90.71f),
            new(1776, 16,    -16.53f,  -140.29f, 1003.63f, 0f, 0f, 180f,   -16.54f,  -139.29f),
            new(1776, 17,    -33.87f,  -186.76f, 1003.63f, 0f, 0f, 180f,   -33.89f,  -185.76f),
        };

        public static void Init(bool enabled)
        {
            _enabled = enabled;
            if (!enabled) return;

            foreach (var m in Machines)
            {
                if (m.Interior != 0) continue;
                _objects.Add(new GlobalObject(m.Model, m.Position, m.Rotation));
            }
        }

        public static void Dispose()
        {
            foreach (var obj in _objects) obj.Dispose();
            _objects.Clear();
            _removedForPlayers.Clear();
        }

        public static void OnConnect(BasePlayer player)
        {
            if (!_enabled) return;
            if (!_removedForPlayers.Add(player.Id)) return;

            foreach (var m in Machines)
            {
                if (m.Interior == 0)
                    GlobalObject.Remove(player, m.Model, m.Position, RemoveRadius);
            }
        }

        public static void OnDisconnect(BasePlayer player)
        {
            _lastUsed.Remove(player.Id);
            _removedForPlayers.Remove(player.Id);
        }

        public static void OnStartSpectating(BasePlayer player)
            => _lastUsed.Remove(player.Id);

        public static void OnFirstSpawn(BasePlayer player)
        {
        }

        public static void OnUpdate(Player player)
        {
            if (!_enabled) return;

            var now = Environment.TickCount;
            var pos = player.Position;

            foreach (var m in Machines)
            {
                if (m.Interior != player.Interior) continue;
                if (Dist2D(pos.X, pos.Y, m.FrontX, m.FrontY) > UseRadius) continue;

                if (_lastUsed.TryGetValue(player.Id, out var last) &&
                    now - last < CooldownMs) continue;

                _lastUsed[player.Id] = now;

                var args = new VendingMachineArgs { Player = player, HealthGiven = DefaultHealth };
                PlayerUseVendingMachine?.Invoke(null, args);

                if (!args.Cancel && args.HealthGiven > 0)
                    WeaponConfigService.HealPlayer(player, args.HealthGiven);

                break;
            }
        }

        private static float Dist2D(float ax, float ay, float bx, float by)
        {
            var dx = ax - bx; var dy = ay - by;
            return MathF.Sqrt(dx * dx + dy * dy);
        }
    }
}