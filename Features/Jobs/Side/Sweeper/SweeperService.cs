#nullable enable
using ProjectSMP.Core;
using ProjectSMP.Entities;
using ProjectSMP.Entities.Players.Delay;
using ProjectSMP.Entities.Vehicles.Handbrake;
using ProjectSMP.Extensions;
using ProjectSMP.Features.Bank.Paycheck;
using ProjectSMP.Features.Jobs.Core;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using SampSharp.Streamer.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Features.Jobs.Side.Sweeper
{
    public static class SweeperService
    {
        private const int DelayMinutes = 30;
        private const float CpSize = 6.0f;
        private const float CpRadius = 6.5f;

        private static readonly HashSet<int> _vehicleIds = new();
        private static readonly Dictionary<int, SweeperSession> _sessions = new();
        private static readonly HashSet<SweeperRoute> _activeRoutes = new();
        private static readonly Dictionary<int, DynamicRaceCheckpoint> _checkpoints = new();

        private static readonly (float X, float Y, float Z, float A)[] Spawns =
        {
            (1626.1691f, -1896.3145f, 13.2919f, 357.4081f),
            (1622.7128f, -1896.3403f, 13.2932f, 358.0085f),
            (1619.3044f, -1896.3998f, 13.2896f, 359.5278f)
        };

        private static readonly Vector3[][] Routes =
        {
            new[] {
                new Vector3(1620.3401f,-1878.2645f,13.1080f),
                new Vector3(1577.7328f,-1870.1625f,13.1080f),
                new Vector3(1571.6348f,-1746.6425f,13.1079f),
                new Vector3(1535.3398f,-1729.6554f,13.1078f),
                new Vector3(1532.1979f,-1606.9507f,13.1080f),
                new Vector3(1442.3958f,-1589.8590f,13.1078f),
                new Vector3(1456.9329f,-1454.3501f,13.0914f),
                new Vector3(1423.5216f,-1422.6418f,13.1080f),
                new Vector3(1371.3789f,-1393.1516f,13.1803f),
                new Vector3(1295.0308f,-1562.8911f,13.1157f),
                new Vector3(1295.1956f,-1839.0238f,13.1080f),
                new Vector3(1805.4341f,-1835.0656f,13.1080f),
                new Vector3(1832.5861f,-1934.8855f,13.1018f),
                new Vector3(1953.1790f,-1934.8962f,13.1079f),
                new Vector3(1963.9730f,-1765.3276f,13.1079f),
                new Vector3(1833.5233f,-1750.0853f,13.1079f),
                new Vector3(1763.9897f,-1730.0765f,13.1079f),
                new Vector3(1701.8733f,-1729.8707f,13.1080f),
                new Vector3(1579.2531f,-1730.0669f,13.1080f),
                new Vector3(1567.0856f,-1860.2482f,13.1079f),
                new Vector3(1620.6377f,-1879.8987f,13.3828f),
            },
            new[] {
                new Vector3(1620.3401f,-1878.2645f,13.1080f),
                new Vector3(1577.7328f,-1870.1625f,13.1080f),
                new Vector3(1571.6348f,-1746.6425f,13.1079f),
                new Vector3(1535.3398f,-1729.6554f,13.1078f),
                new Vector3(1532.1979f,-1606.9507f,13.1080f),
                new Vector3(1442.3958f,-1589.8590f,13.1078f),
                new Vector3(1456.9329f,-1454.3501f,13.0914f),
                new Vector3(1423.5216f,-1422.6418f,13.1080f),
                new Vector3(1371.3789f,-1393.1516f,13.1803f),
                new Vector3(1295.0308f,-1562.8911f,13.1157f),
                new Vector3(1295.1956f,-1839.0238f,13.1080f),
                new Vector3(1620.6377f,-1879.8987f,13.3828f),
            },
            new[] {
                new Vector3(1620.3401f,-1878.2645f,13.1080f),
                new Vector3(1541.5181f,-1869.7919f,13.1080f),
                new Vector3(1324.5179f,-1850.4954f,13.1080f),
                new Vector3(1186.3342f,-1849.1866f,13.1314f),
                new Vector3(1181.5270f,-1711.9373f,13.2262f),
                new Vector3(1044.5283f,-1709.8528f,13.1078f),
                new Vector3(1040.0022f,-1585.2041f,13.1080f),
                new Vector3(929.4984f,-1569.6561f,13.1080f),
                new Vector3(914.7279f,-1584.2065f,13.1080f),
                new Vector3(914.8431f,-1758.9738f,13.1042f),
                new Vector3(818.7015f,-1767.0338f,13.1232f),
                new Vector3(830.6664f,-1625.0950f,13.1080f),
                new Vector3(903.3846f,-1575.4491f,13.1080f),
                new Vector3(1284.4688f,-1574.7339f,13.1080f),
                new Vector3(1295.4666f,-1847.0640f,13.1080f),
                new Vector3(1514.4852f,-1875.3000f,13.1080f),
                new Vector3(1620.6377f,-1879.8987f,13.3828f),
            }
        };

        private static readonly int[] Salaries = { 10000, 12000, 14000 };

        private static readonly string[] RouteLabels =
        {
            "Route A: Idlewood - Commerce",
            "Route B: Pershing Square - Commerce",
            "Route C: Commerce - Marina"
        };

        private const string BriefingText =
            "{FFFFFF}Selamat datang di pekerjaan pembersihan kota.\n" +
            "Tugas kamu adalah menjaga kebersihan jalan dengan menggunakan kendaraan Sweeper. Untuk memulai, segera datangi area parkir Sweeper lalu gunakan kendaraan yang tersedia.\n" +
            "Setelah berada di dalam kendaraan, pilih jalur kerja yang ingin kamu ambil. Jika semua jalur sedang digunakan, harap menunggu hingga rute berikutnya tersedia.\n" +
            "Sistem akan memandu perjalanan kamu melalui beberapa titik tugas di sepanjang area pembersihan. Selesaikan seluruh titik tersebut untuk menuntaskan pekerjaan.\n" +
            "Ketika semua tugas selesai, pembayaran akan langsung diberikan secara otomatis. Kamu bisa melihat total gaji menggunakan perintah {FFFF00}/salary{FFFFFF}.";

        public static void Initialize()
        {
            foreach (var (x, y, z, a) in Spawns)
            {
                var v = Vehicle.CreateVehicle((VehicleModelType)574, new Vector3(x, y, z), a, -1, -1, 60);
                v.VehicleType = VehicleType.Job;
                _vehicleIds.Add(v.Id);
                SideJobVehicleManager.RegisterVehicle(v.Id, (VehicleModelType)574, new Vector3(x, y, z), a, -1, -1);
            }
        }

        public static void Dispose()
        {
            foreach (var cp in _checkpoints.Values) cp.Dispose();
            _checkpoints.Clear();
        }

        public static void OnPlayerEnterVehicle(Player player, Vehicle? vehicle, bool isPassenger)
        {
            if (isPassenger || vehicle == null || !_vehicleIds.Contains(vehicle.Id)) return;
            if (!player.IsCharLoaded || _sessions.ContainsKey(player.Id)) return;
            if (SideJobVehicleManager.IsPendingRespawn(vehicle.Id))
            {
                var lastPos = player.Position;
                var tm = new Timer(100, false);
                tm.Tick += (s, e) => { tm.Dispose(); if (!player.IsConnected) return; player.RemoveFromVehicle(); player.SetPositionSafe(lastPos); };
            }
        }

        public static void OnPlayerExitVehicle(Player player, Vehicle? vehicle)
        {
            if (vehicle == null || !_vehicleIds.Contains(vehicle.Id)) return;
            if (!_sessions.TryGetValue(player.Id, out var session)) return;

            CancelJob(player, session);
            player.SendClientMessage(Color.White, $"{Msg.Sweeper} Pekerjaan Sweeper dibatalkan karena keluar dari kendaraan.");
            SideJobVehicleManager.ScheduleRespawn(vehicle);
        }

        public static void OnPlayerDisconnect(Player player)
        {
            if (!_sessions.TryGetValue(player.Id, out var session)) return;

            _activeRoutes.Remove(session.Route);
            _sessions.Remove(player.Id);
            ClearCheckpoint(player.Id);

            var v = BaseVehicle.Find(session.VehicleId) as Vehicle;
            if (v != null) SideJobVehicleManager.ScheduleRespawn(v);
        }

        public static void OnPlayerStateChanged(Player player, PlayerState newState, PlayerState oldState)
        {
            if (newState != PlayerState.Driving) return;
            var vehicle = player.Vehicle as Vehicle;
            if (vehicle == null || !_vehicleIds.Contains(vehicle.Id)) return;
            if (!player.IsCharLoaded || _sessions.ContainsKey(player.Id)) return;
            if (SideJobVehicleManager.IsPendingRespawn(vehicle.Id)) return;
            ShowStartDialog(player);
        }

        private static void ShowStartDialog(Player player)
        {
            player.ShowMessage("Side Job - Sweeper", "Kamu akan bekerja sebagai pembersih jalan?")
                .WithButtons("Start Job", "Close")
                .Show(e =>
                {
                    var v = player.Vehicle as Vehicle;
                    if (e.DialogButton != DialogButton.Left)
                    {
                        if (v != null) SideJobVehicleManager.EjectAndScheduleRespawn(player, v);
                        return;
                    }

                    if (DelayService.HasJobDelay(player, "sweeper"))
                    {
                        var rem = DelayService.GetJobDelay(player, "sweeper");
                        player.SendClientMessage(Color.White,
                            $"{Msg.Sweeper} Kamu harus menunggu {{FF6347}}{rem} menit{{FFFFFF}} sebelum bekerja sebagai Sweeper lagi.");
                        if (v != null) SideJobVehicleManager.EjectAndScheduleRespawn(player, v);
                        return;
                    }

                    ShowRouteDialog(player);
                });
        }

        private static void ShowRouteDialog(Player player)
        {
            var rows = new List<string[]>();
            for (var i = 0; i < 3; i++)
            {
                var isActive = _activeRoutes.Contains((SweeperRoute)i);
                var col = isActive
                    ? "{FF6347}Cleaning{ffffff}"
                    : $"{{00FF00}}{Utilities.GroupDigits(Salaries[i])}{{ffffff}}";
                rows.Add(new[] { RouteLabels[i], col });
            }

            player.ShowTabList("Sweeper Sidejob", new[] { "Route", "Salary" })
                .WithRows(rows.ToArray())
                .WithButtons("Select", "Cancel")
                .Show(e =>
                {
                    var v = player.Vehicle as Vehicle;
                    if (e.DialogButton != DialogButton.Left)
                    {
                        if (v != null) SideJobVehicleManager.EjectAndScheduleRespawn(player, v);
                        return;
                    }

                    if (e.ListItem < 0 || e.ListItem > 2) return;
                    var route = (SweeperRoute)e.ListItem;

                    if (_activeRoutes.Contains(route))
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Sweeper} Rute ini sedang digunakan oleh orang lain!");
                        ShowRouteDialog(player);
                        return;
                    }

                    ShowBriefing(player, route);
                });
        }

        private static void ShowBriefing(Player player, SweeperRoute route)
        {
            player.ShowMessage("Los Santos City Sweeping Co.", BriefingText)
                .WithButtons("Mulai", "Batal")
                .Show(e =>
                {
                    var v = player.Vehicle as Vehicle;
                    if (e.DialogButton != DialogButton.Left)
                    {
                        if (v != null) SideJobVehicleManager.EjectAndScheduleRespawn(player, v);
                        return;
                    }
                    StartJob(player, route);
                });
        }

        private static void StartJob(Player player, SweeperRoute route)
        {
            var v = player.Vehicle as Vehicle;
            if (v == null) return;

            _activeRoutes.Add(route);
            var session = new SweeperSession { Route = route, CheckpointIndex = 0, VehicleId = v.Id };
            _sessions[player.Id] = session;

            SetCheckpoint(player, session);
            player.SendClientMessage(Color.White,
                $"{Msg.Sweeper} Rute {{FFFF00}}{RouteLabels[(int)route]}{{FFFFFF}} dimulai! Ikuti checkpoint di minimap.");
        }

        private static void Process(Player player, SweeperSession session)
        {
            var pts = Routes[(int)session.Route];
            session.CheckpointIndex++;

            if (session.CheckpointIndex >= pts.Length)
            {
                ClearCheckpoint(player.Id);
                FinalizeJob(player, session);
                return;
            }

            SetCheckpoint(player, session);
        }

        private static void SetCheckpoint(Player player, SweeperSession session)
        {
            ClearCheckpoint(player.Id);

            var pts = Routes[(int)session.Route];
            var idx = session.CheckpointIndex;
            var pos = pts[idx];
            var next = idx + 1 < pts.Length ? pts[idx + 1] : pos;
            var type = idx == pts.Length - 1 ? CheckpointType.Finish : CheckpointType.Normal;

            var cp = new DynamicRaceCheckpoint(type, pos, next, CpSize, -1, -1, player, 1500.0f);
            cp.Enter += (s, e) =>
            {
                if (e.Player != player || !_sessions.TryGetValue(player.Id, out var sess)) return;
                Process(player, sess);
            };

            _checkpoints[player.Id] = cp;
        }

        private static void ClearCheckpoint(int pid)
        {
            if (_checkpoints.TryGetValue(pid, out var cp)) { cp.Dispose(); _checkpoints.Remove(pid); }
        }

        private static void FinalizeJob(Player player, SweeperSession session)
        {
            _activeRoutes.Remove(session.Route);
            _sessions.Remove(player.Id);
            player.RemoveFromVehicle();

            var salary = Salaries[(int)session.Route];
            DelayService.SetJobDelay(player, "sweeper", DelayMinutes);
            PaycheckService.GivePaycheck(player, salary, "Sidejob(Sweeper)");

            player.SendClientMessage(Color.White,
                $"{Msg.Sweeper} Pembersihan selesai! Paycheck {{00FF00}}{Utilities.GroupDigits(salary)}{{FFFFFF}} ditambahkan. " +
                $"Delay {{FF6347}}{DelayMinutes} menit{{FFFFFF}} dimulai.");

            var v = BaseVehicle.Find(session.VehicleId) as Vehicle;
            if (v != null) SideJobVehicleManager.ScheduleRespawn(v);
        }

        private static void CancelJob(Player player, SweeperSession session)
        {
            _activeRoutes.Remove(session.Route);
            _sessions.Remove(player.Id);
            ClearCheckpoint(player.Id);
        }
    }
}