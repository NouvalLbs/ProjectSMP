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
using System.Collections.Generic;

namespace ProjectSMP.Features.Jobs.Side.Mower
{
    public static class MowerService
    {
        private const int DelayMinutes = 15;
        private const int PaycheckAmount = 20000;
        private const float CpSize = 6.0f;

        private static readonly HashSet<int> _vehicleIds = new();
        private static readonly Dictionary<int, MowerSession> _sessions = new();
        private static readonly Dictionary<int, DynamicRaceCheckpoint> _checkpoints = new();

        private static readonly (float X, float Y, float Z, float A)[] SpawnPoints =
        {
            (2039.1313f, -1248.9849f, 23.3589f, 359.5252f),
            (2042.3505f, -1248.9352f, 23.3298f, 359.6588f),
            (2045.7902f, -1248.8533f, 23.3434f, 0.3639f),
            (2049.6299f, -1248.7933f, 23.3363f, 0.2729f)
        };

        private static readonly Vector3[] Route =
        {
            new(2042.5602f, -1236.3042f, 22.6192f),
            new(2035.3668f, -1195.2971f, 22.0680f),
            new(1992.0043f, -1164.1172f, 20.3473f),
            new(1939.8607f, -1161.8895f, 20.8457f),
            new(1911.4937f, -1176.4573f, 22.6597f),
            new(1922.5072f, -1207.6255f, 19.5168f),
            new(1911.6895f, -1222.9646f, 17.4940f),
            new(1894.0754f, -1208.2191f, 18.4516f),
            new(1938.9170f, -1180.1436f, 19.7342f),
            new(2007.5355f, -1180.9120f, 19.8499f),
            new(2014.2781f, -1213.4199f, 20.0716f),
            new(2016.8627f, -1238.0829f, 22.0594f),
            new(1995.7285f, -1241.7998f, 20.5814f),
            new(1997.9880f, -1226.7389f, 20.1805f),
            new(2037.1726f, -1180.0479f, 22.4230f),
            new(2046.6088f, -1172.7184f, 22.8083f),
            new(2052.5425f, -1197.7012f, 23.2124f),
            new(2052.7395f, -1220.0739f, 23.2642f),
            new(2052.1284f, -1237.3043f, 23.6373f)
        };

        private const string BriefingText =
            "{FFFFFF}Kamu ditugaskan untuk memotong rumput di area taman kota Los Santos menggunakan kendaraan Lawn Mower.\n\n" +
            "Untuk memulai, kendarai Mower menuju setiap titik checkpoint yang telah ditentukan secara berurutan.\n" +
            "Pastikan kamu mengikuti seluruh jalur yang ada hingga pekerjaan selesai.\n\n" +
            "{FFFF00}Catatan:\n{FFFFFF}" +
            "Jangan keluar dari kendaraan selama pekerjaan berlangsung atau pekerjaan akan dibatalkan.\n" +
            "Setelah semua checkpoint diselesaikan, pembayaran akan diberikan secara otomatis.\n" +
            "Kamu dapat mengecek total penghasilan menggunakan perintah {FFFF00}/salary{FFFFFF}.";

        public static void Initialize()
        {
            foreach (var (x, y, z, a) in SpawnPoints)
            {
                var v = Vehicle.CreateVehicle((VehicleModelType)572, new Vector3(x, y, z), a, -1, -1, 60);
                v.VehicleType = VehicleType.Job;
                _vehicleIds.Add(v.Id);
                SideJobVehicleManager.RegisterVehicle(v.Id, (VehicleModelType)572, new Vector3(x, y, z), a, -1, -1);
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
            if (!_sessions.TryGetValue(player.Id, out _)) return;

            CancelJob(player);
            player.SendClientMessage(Color.White, $"{Msg.Mower} Pekerjaan Mower dibatalkan karena keluar dari kendaraan.");
            SideJobVehicleManager.ScheduleRespawn(vehicle);
        }

        public static void OnPlayerDisconnect(Player player)
        {
            if (!_sessions.TryGetValue(player.Id, out var session)) return;

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
            player.ShowMessage("Side Job - Mower", "Anda akan bekerja sebagai Lawn Mower?")
                .WithButtons("Start Job", "Close")
                .Show(e =>
                {
                    var v = player.Vehicle as Vehicle;
                    if (e.DialogButton != DialogButton.Left)
                    {
                        if (v != null) SideJobVehicleManager.EjectAndScheduleRespawn(player, v);
                        return;
                    }

                    if (DelayService.HasJobDelay(player, "mower"))
                    {
                        var rem = DelayService.GetJobDelay(player, "mower");
                        player.SendClientMessage(Color.White,
                            $"{Msg.Mower} Kamu harus menunggu {{FF6347}}{rem} menit{{FFFFFF}} sebelum bekerja sebagai Mower lagi.");
                        if (v != null) SideJobVehicleManager.EjectAndScheduleRespawn(player, v);
                        return;
                    }

                    ShowBriefing(player);
                });
        }

        private static void ShowBriefing(Player player)
        {
            player.ShowMessage("Los Santos Mower", BriefingText)
                .WithButtons("Mulai", "Batal")
                .Show(e =>
                {
                    var v = player.Vehicle as Vehicle;
                    if (e.DialogButton != DialogButton.Left)
                    {
                        if (v != null) SideJobVehicleManager.EjectAndScheduleRespawn(player, v);
                        return;
                    }
                    StartJob(player);
                });
        }

        private static void StartJob(Player player)
        {
            var v = player.Vehicle as Vehicle;
            if (v == null) return;

            var session = new MowerSession { CheckpointIndex = 0, VehicleId = v.Id };
            _sessions[player.Id] = session;

            SetCheckpoint(player, session);
            player.SendClientMessage(Color.White, $"{Msg.Mower} Pekerjaan dimulai! Ikuti checkpoint di minimap.");
        }

        private static void Process(Player player, MowerSession session)
        {
            session.CheckpointIndex++;

            if (session.CheckpointIndex >= Route.Length)
            {
                ClearCheckpoint(player.Id);
                FinalizeJob(player, session);
                return;
            }

            SetCheckpoint(player, session);
        }

        private static void SetCheckpoint(Player player, MowerSession session)
        {
            ClearCheckpoint(player.Id);

            var idx = session.CheckpointIndex;
            var pos = Route[idx];
            var next = idx + 1 < Route.Length ? Route[idx + 1] : pos;
            var type = idx == Route.Length - 1 ? CheckpointType.Finish : CheckpointType.Normal;

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

        private static void FinalizeJob(Player player, MowerSession session)
        {
            _sessions.Remove(player.Id);
            SideJobVehicleManager.StopAndEject(player);

            DelayService.SetJobDelay(player, "mower", DelayMinutes);
            PaycheckService.GivePaycheck(player, PaycheckAmount, "Sidejob(Mower)");

            player.SendClientMessage(Color.White,
                $"{Msg.Mower} Pekerjaan selesai! Paycheck {{00FF00}}{Utilities.GroupDigits(PaycheckAmount)}{{FFFFFF}} ditambahkan. " +
                $"Delay {{FF6347}}{DelayMinutes} menit{{FFFFFF}} dimulai.");

            var v = BaseVehicle.Find(session.VehicleId) as Vehicle;
            if (v != null) SideJobVehicleManager.ScheduleRespawn(v);
        }

        private static void CancelJob(Player player)
        {
            _sessions.Remove(player.Id);
            ClearCheckpoint(player.Id);
        }
    }
}