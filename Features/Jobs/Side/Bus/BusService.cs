#nullable enable
using ProjectSMP.Core;
using ProjectSMP.Entities;
using ProjectSMP.Entities.Players.Delay;
using ProjectSMP.Extensions;
using ProjectSMP.Features.Bank.Paycheck;
using ProjectSMP.Features.Jobs.Core;
using ProjectSMP.Features.ProgressBar;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using SampSharp.Streamer.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Features.Jobs.Side.Bus
{
    public static class BusService
    {
        private const int DelayMinutes = 25;
        private const float CpSize = 6.0f;
        private const float CpRadius = 6.5f;
        private const int StopWaitDuration = 15;

        private static readonly HashSet<int> _vehicleIds = new();
        private static readonly Dictionary<int, BusSession> _sessions = new();
        private static readonly HashSet<BusRoute> _activeRoutes = new();
        private static readonly Dictionary<int, DynamicRaceCheckpoint> _checkpoints = new();

        private static readonly (float X, float Y, float Z, float A)[] Spawns =
        {
            (1698.3337f, -1496.5632f, 13.4454f, 359.1142f),
            (1698.2653f, -1511.8376f, 13.4449f, 359.8352f),
            (1698.4172f, -1526.7098f, 13.4457f, 0.4048f)
        };

        private static readonly Vector3[][] Routes =
        {
            new[] {
                new Vector3(1672.2297f, -1477.4254f, 13.4835f),
                new Vector3(1655.8987f, -1576.3344f, 13.4836f),
                new Vector3(1329.5511f, -1570.6804f, 13.4726f),
                new Vector3(1359.1057f, -1424.8951f, 13.4846f),
                new Vector3(1301.6941f, -1392.8998f, 13.3665f),
                new Vector3(656.4448f, -1392.6232f, 13.5632f),
                new Vector3(544.7765f, -1403.7097f, 15.5084f),
                new Vector3(494.4586f, -1312.7657f, 15.7449f),
                new Vector3(155.6975f, -1541.1519f, 10.9875f),
                new Vector3(374.9712f, -1719.7097f, 7.3549f),
                new Vector3(1004.8592f, -1809.8195f, 14.1480f),
                new Vector3(1025.2909f, -2253.6135f, 13.0509f),
                new Vector3(1305.1884f, -2466.7974f, 7.7640f),
                new Vector3(1473.1068f, -2687.9290f, 11.8876f),
                new Vector3(2174.5894f, -2594.9338f, 13.4704f),
                new Vector3(2294.2764f, -2284.8462f, 13.4759f),
                new Vector3(2245.2012f, -2208.1697f, 13.4233f),
                new Vector3(1980.6378f, -2107.2607f, 13.4505f),
                new Vector3(1964.5698f, -1767.6537f, 13.4828f),
                new Vector3(1836.4011f, -1749.6033f, 13.4827f),
                new Vector3(1824.1890f, -1628.6423f, 13.4851f),
                new Vector3(1673.1405f, -1589.9279f, 13.4850f),
                new Vector3(1660.3385f, -1556.1967f, 13.4891f),
                new Vector3(1687.0594f, -1550.5013f, 13.3828f)
            },
            new[] {
                new Vector3(1670.7985f, -1477.7079f, 13.4836f),
                new Vector3(1655.7999f, -1579.5162f, 13.4847f),
                new Vector3(1673.4508f, -1594.5579f, 13.4756f),
                new Vector3(1807.1559f, -1614.3226f, 13.4546f),
                new Vector3(1832.1064f, -1553.8787f, 13.4743f),
                new Vector3(1852.4220f, -1479.2195f, 13.4842f),
                new Vector3(1971.8844f, -1469.0979f, 13.4912f),
                new Vector3(1989.3937f, -1436.8660f, 14.1965f),
                new Vector3(1989.5898f, -1356.6455f, 23.8869f),
                new Vector3(2053.2014f, -1343.2856f, 23.9210f),
                new Vector3(2073.3665f, -1241.0449f, 23.9058f),
                new Vector3(2074.4663f, -1111.5432f, 24.4950f),
                new Vector3(1995.2961f, -1054.7124f, 24.5102f),
                new Vector3(1864.8937f, -1070.3771f, 23.7835f),
                new Vector3(1863.7941f, -1164.6207f, 23.7768f),
                new Vector3(1656.0852f, -1158.5059f, 23.8820f),
                new Vector3(1594.0417f, -1158.5602f, 24.0073f),
                new Vector3(1544.6841f, -1050.4414f, 23.7226f),
                new Vector3(1461.6492f, -1031.6943f, 23.7512f),
                new Vector3(1385.3629f, -1032.9503f, 25.9522f),
                new Vector3(1346.6372f, -1081.8737f, 24.7022f),
                new Vector3(1341.0253f, -1247.7905f, 13.5371f),
                new Vector3(1340.0535f, -1375.4524f, 13.5610f),
                new Vector3(1377.2910f, -1409.0233f, 13.4821f),
                new Vector3(1395.0640f, -1427.5823f, 13.4891f),
                new Vector3(1636.2471f, -1442.9843f, 13.4831f),
                new Vector3(1655.6055f, -1539.3159f, 13.3828f)
            },
            new[] {
                new Vector3(1675.8811f, -1478.1013f, 13.4732f),
                new Vector3(1655.6061f, -1554.3219f, 13.4852f),
                new Vector3(1508.8109f, -1589.8887f, 13.4828f),
                new Vector3(1426.6741f, -1665.7053f, 13.4801f),
                new Vector3(1386.8522f, -1800.9695f, 13.4817f),
                new Vector3(1131.5002f, -1849.5167f, 13.4840f),
                new Vector3(981.2806f, -1782.4598f, 14.1825f),
                new Vector3(878.5309f, -1767.7321f, 13.4767f),
                new Vector3(641.0339f, -1622.0313f, 15.3255f),
                new Vector3(640.0079f, -1345.6520f, 13.4927f),
                new Vector3(653.9548f, -1205.6824f, 18.2400f),
                new Vector3(853.4523f, -1026.4595f, 27.3663f),
                new Vector3(923.0790f, -982.7656f, 38.2466f),
                new Vector3(1121.8904f, -957.8655f, 42.6443f),
                new Vector3(1353.7388f, -988.2960f, 29.3819f),
                new Vector3(1461.6157f, -1037.0037f, 23.7534f),
                new Vector3(1569.6902f, -1101.8577f, 23.5607f),
                new Vector3(1663.3933f, -1163.4125f, 23.7931f),
                new Vector3(1712.1957f, -1276.3503f, 13.4828f),
                new Vector3(1712.6160f, -1408.6566f, 13.4848f),
                new Vector3(1672.1544f, -1439.0385f, 13.4833f),
                new Vector3(1655.6210f, -1492.7557f, 13.4836f),
                new Vector3(1681.0796f, -1550.4260f, 13.3828f)
            }
        };

        private static readonly bool[][] StopCheckpoints =
        {
            new[] { false, false, true, false, false, true, false, false, true, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false },
            new[] { false, false, false, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, false },
            new[] { false, false, false, true, false, false, false, true, false, true, false, false, true, false, false, true, false, false, false, false, false, false, false }
        };

        private static readonly int[] Salaries = { 15000, 20000, 25000 };

        private static readonly string[] RouteLabels =
        {
            "Route A: Commerce - Ocean Dock",
            "Route B: Commerce - Los Santos Bank",
            "Route C: Jefferson"
        };

        private const string BriefingText =
            "{FFFFFF}Kamu bekerja sebagai pengemudi bus kota yang bertanggung jawab " +
            "mengantar penumpang menuju tujuan mereka dengan aman dan sesuai rute.\n\n" +

            "Untuk memulai pekerjaan, datangi area kendaraan Bus yang tersedia lalu " +
            "masuk ke dalam kendaraan tersebut.\n" +
            "Setelah itu, pilih rute perjalanan yang ingin kamu jalankan berdasarkan " +
            "pilihan yang muncul di layar.\n\n" +

            "Sistem akan memandu perjalanan melalui sejumlah titik pemberhentian, " +
            "termasuk area penjemputan dan penurunan penumpang sesuai jalur yang dipilih.\n" +
            "Ikuti setiap titik hingga perjalanan selesai.\n\n" +

            "Setelah seluruh rute berhasil diselesaikan, pembayaran akan diberikan " +
            "secara otomatis.\n" +
            "Kamu dapat melihat total penghasilan menggunakan perintah " +
            "{FFFF00}/salary{FFFFFF}.";

        public static void Initialize()
        {
            foreach (var (x, y, z, a) in Spawns)
            {
                var v = Vehicle.CreateVehicle((VehicleModelType)431, new Vector3(x, y, z), a, -1, -1, 60);
                v.VehicleType = VehicleType.Job;
                _vehicleIds.Add(v.Id);
                SideJobVehicleManager.RegisterVehicle(v.Id, (VehicleModelType)431, new Vector3(x, y, z), a, -1, -1);
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
            player.SendClientMessage(Color.White, $"{Msg.Bus} Pekerjaan Bus dibatalkan karena keluar dari kendaraan.");
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
            player.ShowMessage("Side Job - Bus", "Anda akan bekerja sebagai pengangkut penumpang bus?")
                .WithButtons("Start Job", "Close")
                .Show(e =>
                {
                    var v = player.Vehicle as Vehicle;
                    if (e.DialogButton != DialogButton.Left)
                    {
                        if (v != null) SideJobVehicleManager.EjectAndScheduleRespawn(player, v);
                        return;
                    }

                    if (DelayService.HasJobDelay(player, "bus"))
                    {
                        var rem = DelayService.GetJobDelay(player, "bus");
                        player.SendClientMessage(Color.White,
                            $"{Msg.Bus} Kamu harus menunggu {{FF6347}}{rem} menit{{FFFFFF}} sebelum bekerja sebagai Bus Driver lagi.");
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
                var isActive = _activeRoutes.Contains((BusRoute)i);
                var col = isActive
                    ? "{FF6347}Taken{ffffff}"
                    : $"{{00FF00}}{Utilities.GroupDigits(Salaries[i])}{{ffffff}}";
                rows.Add(new[] { RouteLabels[i], col });
            }

            player.ShowTabList("Bus Driver Sidejob", new[] { "Route", "Salary" })
                .WithRows(rows.ToArray())
                .WithButtons("Select", "Close")
                .Show(e =>
                {
                    var v = player.Vehicle as Vehicle;
                    if (e.DialogButton != DialogButton.Left)
                    {
                        if (v != null) SideJobVehicleManager.EjectAndScheduleRespawn(player, v);
                        return;
                    }

                    if (e.ListItem < 0 || e.ListItem > 2) return;
                    var route = (BusRoute)e.ListItem;

                    if (_activeRoutes.Contains(route))
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Bus} Rute ini sedang digunakan oleh orang lain!");
                        ShowRouteDialog(player);
                        return;
                    }

                    ShowBriefing(player, route);
                });
        }

        private static void ShowBriefing(Player player, BusRoute route)
        {
            player.ShowMessage("Bus Driver", BriefingText)
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

        private static void StartJob(Player player, BusRoute route)
        {
            var v = player.Vehicle as Vehicle;
            if (v == null) return;

            _activeRoutes.Add(route);
            var session = new BusSession
            {
                Route = route,
                CheckpointIndex = 0,
                VehicleId = v.Id,
                Phase = BusPhase.Driving
            };
            _sessions[player.Id] = session;

            SetCheckpoint(player, session);
            player.SendClientMessage(Color.White,
                $"{Msg.Bus} Rute {{FFFF00}}{RouteLabels[(int)route]}{{FFFFFF}} dimulai! Ikuti checkpoint di minimap.");
        }

        private static void SetCheckpoint(Player player, BusSession session)
        {
            ClearCheckpoint(player.Id);

            var pts = Routes[(int)session.Route];
            var stops = StopCheckpoints[(int)session.Route];
            var idx = session.CheckpointIndex;
            var pos = pts[idx];
            var next = idx + 1 < pts.Length ? pts[idx + 1] : pos;

            var isStop = idx < stops.Length && stops[idx];
            var isLast = idx == pts.Length - 1;
            var type = (isLast || isStop) ? CheckpointType.Finish : CheckpointType.Normal;

            var cp = new DynamicRaceCheckpoint(type, pos, next, CpSize, -1, -1, player, 1500.0f);
            cp.Enter += (s, e) =>
            {
                if (e.Player != player || !_sessions.TryGetValue(player.Id, out var sess)) return;
                if (sess.Phase != BusPhase.Driving) return;

                var curStops = StopCheckpoints[(int)sess.Route];
                var curIdx = sess.CheckpointIndex;
                var isStopPoint = curIdx < curStops.Length && curStops[curIdx];

                if (isStopPoint)
                {
                    ClearCheckpoint(player.Id);
                    sess.Phase = BusPhase.Waiting;
                    player.ToggleControllable(false);
                    ProgressBarService.StartProgress(player, StopWaitDuration, "PLEASE_WAIT");

                    var waitTimer = new Timer(StopWaitDuration * 1000, false);
                    waitTimer.Tick += (ws, we) =>
                    {
                        waitTimer.Dispose();
                        if (!player.IsConnected || !_sessions.TryGetValue(player.Id, out var s2)) return;

                        player.ToggleControllable(true);
                        s2.Phase = BusPhase.Driving;
                        s2.CheckpointIndex++;

                        var pts2 = Routes[(int)s2.Route];
                        if (s2.CheckpointIndex >= pts2.Length)
                        {
                            FinalizeJob(player, s2);
                            return;
                        }

                        SetCheckpoint(player, s2);
                    };
                }
                else
                {
                    sess.CheckpointIndex++;
                    var pts2 = Routes[(int)sess.Route];

                    if (sess.CheckpointIndex >= pts2.Length)
                    {
                        ClearCheckpoint(player.Id);
                        FinalizeJob(player, sess);
                        return;
                    }

                    SetCheckpoint(player, sess);
                }
            };

            _checkpoints[player.Id] = cp;
        }

        private static void ClearCheckpoint(int pid)
        {
            if (_checkpoints.TryGetValue(pid, out var cp)) { cp.Dispose(); _checkpoints.Remove(pid); }
        }

        private static void FinalizeJob(Player player, BusSession session)
        {
            _activeRoutes.Remove(session.Route);
            _sessions.Remove(player.Id);
            player.RemoveFromVehicle();

            var salary = Salaries[(int)session.Route];
            DelayService.SetJobDelay(player, "bus", DelayMinutes);
            PaycheckService.GivePaycheck(player, salary, "Sidejob(Bus)");

            player.SendClientMessage(Color.White,
                $"{Msg.Bus} Rute selesai! Paycheck {{00FF00}}{Utilities.GroupDigits(salary)}{{FFFFFF}} ditambahkan. " +
                $"Delay {{FF6347}}{DelayMinutes} menit{{FFFFFF}} dimulai.");

            var v = BaseVehicle.Find(session.VehicleId) as Vehicle;
            if (v != null) SideJobVehicleManager.ScheduleRespawn(v);
        }

        private static void CancelJob(Player player, BusSession session)
        {
            _activeRoutes.Remove(session.Route);
            _sessions.Remove(player.Id);
            ClearCheckpoint(player.Id);
            player.ToggleControllable(true);

            if (player.ProgressBarData.IsActive)
                ProgressBarService.DestroyProgressBar(player);
        }
    }
}