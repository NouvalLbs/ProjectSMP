#nullable enable
using ProjectSMP.Core;
using ProjectSMP.Entities;
using ProjectSMP.Entities.Players.Delay;
using ProjectSMP.Entities.Vehicles.Handbrake;
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
using System.Linq;

namespace ProjectSMP.Features.Jobs.Side.Forklifter
{
    public static class ForklifterService
    {
        private const int MaxCycles = 5;
        private const int PaycheckAmount = 20000;
        private const int DelayMinutes = 15;
        private const int ProgressDuration = 6;
        private const float CheckpointSize = 4.0f;
        private const float CheckpointCheckRadius = 4.5f;

        private static readonly HashSet<int> _vehicleIds = new();
        private static readonly Dictionary<int, ForklifterSession> _sessions = new();
        private static readonly Dictionary<int, DynamicRaceCheckpoint> _checkpoints = new();
        private static readonly Random _rng = new();

        private static readonly Vector3[] LoadPositions =
        {
            new(2555.8987f, -2465.7002f, 13.4002f),
            new(2783.2563f, -2369.0671f, 13.4004f),
            new(2445.2659f, -2490.4629f, 13.4105f),
            new(2402.7097f, -2565.7771f, 13.3183f),
            new(2349.3796f, -2332.3906f, 13.2158f)
        };

        private static readonly Vector3[] UnloadPositions =
        {
            new(2778.0747f, -2414.1794f, 13.4041f),
            new(2793.6707f, -2458.9797f, 13.3978f),
            new(2779.5942f, -2459.3501f, 13.4039f),
            new(2787.3235f, -2490.9282f, 13.4180f)
        };

        private static readonly (float X, float Y, float Z, float A)[] SpawnPoints =
        {
            (2758.7400f, -2385.7976f, 13.4077f, 177.1400f),
            (2749.7400f, -2385.7957f, 13.4244f, 177.1422f),
            (2753.1106f, -2385.8047f, 13.4057f, 177.4073f),
            (2755.7400f, -2385.7966f, 13.4036f, 177.1400f)
        };

        private const string BriefingText =
            "{FFFFFF}Kamu ditugaskan untuk membantu proses pemuatan barang di area Ocean Docks menggunakan Forklift.\n" +
            "Kamu akan diarahkan oleh checkpoint untuk mengambil barang yang akan dipindahkan ke gudang.\n" +
            "Pastikan kamu menunggu hingga indikator Progress bar terisi penuh sebelum melanjutkan proses berikutnya.\n" +
            "Setelah pemuatan selesai, kamu akan diarahkan menuju gudang untuk menyimpan box yang sudah diangkut menggunakan Forklift.\n\n" +
            "{FFFF00}Catatan:\n{FFFFFF}" +
            "Pekerjaan ini mengharuskan kamu memindahkan total {FFFF00}5 box{FFFFFF} agar tugas dapat diselesaikan sepenuhnya.\n" +
            "Setelah semua box berhasil dipindahkan, harap kembalikan Forklift ke lokasi yang telah ditentukan oleh checkpoint.\n" +
            "Gaji akan otomatis masuk setelah pekerjaan selesai. Kamu dapat mengeceknya menggunakan perintah {FFFF00}/salary{FFFFFF}.";

        public static void Initialize()
        {
            foreach (var (x, y, z, a) in SpawnPoints)
            {
                var v = Vehicle.CreateVehicle((VehicleModelType)530, new Vector3(x, y, z), a, -1, -1, 60);
                v.VehicleType = VehicleType.Job;
                _vehicleIds.Add(v.Id);
                SideJobVehicleManager.RegisterVehicle(v.Id, (VehicleModelType)530, new Vector3(x, y, z), a, -1, -1);
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

            CancelJob(player);
            player.SendClientMessage(Color.White, $"{Msg.Forklifter} Pekerjaan Forklift dibatalkan karena keluar dari kendaraan.");
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
            player.ShowMessage("Side Job - Forklift", "Anda akan bekerja sebagai forklift?")
                .WithButtons("Start Job", "Close")
                .Show(e =>
                {
                    var vehicle = player.Vehicle as Vehicle;

                    if (e.DialogButton != DialogButton.Left)
                    {
                        if (vehicle != null)
                            SideJobVehicleManager.EjectAndScheduleRespawn(player, vehicle);
                        return;
                    }

                    if (DelayService.HasJobDelay(player, "forklifter"))
                    {
                        var rem = DelayService.GetJobDelay(player, "forklifter");
                        player.SendClientMessage(Color.White,
                            $"{Msg.Forklifter} Kamu harus menunggu {{FF6347}}{rem} menit{{FFFFFF}} sebelum bekerja sebagai Forklift lagi.");
                        if (vehicle != null)
                            SideJobVehicleManager.EjectAndScheduleRespawn(player, vehicle);
                        return;
                    }

                    ShowBriefing(player);
                });
        }

        private static void ShowBriefing(Player player)
        {
            player.ShowMessage("{FFFF00}Los Santos Dock Forklifter{FFFFFF}", BriefingText)
                .WithButtons("Mulai", "Batal")
                .Show(e =>
                {
                    var vehicle = player.Vehicle as Vehicle;

                    if (e.DialogButton != DialogButton.Left)
                    {
                        if (vehicle != null)
                            SideJobVehicleManager.EjectAndScheduleRespawn(player, vehicle);
                        return;
                    }

                    StartJob(player);
                });
        }

        private static void StartJob(Player player)
        {
            var vehicle = player.Vehicle as Vehicle;
            if (vehicle == null) return;

            var session = new ForklifterSession
            {
                IsActive = true,
                Phase = ForklifterPhase.GoToLoad,
                CycleCount = 0,
                VehicleId = vehicle.Id,
                LoadQueue = BuildShuffledQueue(LoadPositions.Length),
                UnloadQueue = BuildShuffledQueue(UnloadPositions.Length)
            };
            _sessions[player.Id] = session;

            var loadIdx = NextIndex(session.LoadQueue, LoadPositions.Length);
            var nextLoadIdx = PeekOrFallback(session.LoadQueue, (loadIdx + 1) % LoadPositions.Length);
            SetCheckpoint(player.Id, LoadPositions[loadIdx], LoadPositions[nextLoadIdx], CheckpointType.Normal);

            player.SendClientMessage(Color.White, $"{Msg.Forklifter} Kamu bertugas memindahkan beberapa box dari trailer lalu menaruhnya ke rak penyimpanan.");
            player.SendClientMessage(Color.White, $"{Msg.Forklifter} Silakan menuju marker yang tersedia untuk mengambil box yang telah disiapkan sebelumnya.");
        }

        private static void Process(Player player, ForklifterSession session)
        {
            switch (session.Phase)
            {
                case ForklifterPhase.GoToLoad:
                    ClearCheckpoint(player.Id);
                    session.Phase = ForklifterPhase.Loading;
                    player.ToggleControllable(false);
                    ProgressBarService.StartProgress(player, ProgressDuration, "Loading_Cargo...");

                    var loadTimer = new Timer(ProgressDuration * 1000, false);
                    loadTimer.Tick += (s, e) =>
                    {
                        loadTimer.Dispose();
                        if (!player.IsConnected || !_sessions.ContainsKey(player.Id)) return;
                        player.ToggleControllable(true);
                        session.CycleCount++;

                        var unloadIdx = NextIndex(session.UnloadQueue, UnloadPositions.Length);
                        var nextUnloadIdx = PeekOrFallback(session.UnloadQueue, (unloadIdx + 1) % UnloadPositions.Length);
                        SetCheckpoint(player.Id, UnloadPositions[unloadIdx], UnloadPositions[nextUnloadIdx], CheckpointType.Normal);

                        session.Phase = ForklifterPhase.GoToUnload;
                        player.SendClientMessage(Color.White,
                            $"{Msg.Forklifter} Kargo dimuat! Antarkan ke {{FFFF00}}titik unload{{FFFFFF}}. ({session.CycleCount}/{MaxCycles})");
                    };
                    break;

                case ForklifterPhase.GoToUnload:
                    ClearCheckpoint(player.Id);
                    session.Phase = ForklifterPhase.Unloading;
                    player.ToggleControllable(false);
                    ProgressBarService.StartProgress(player, ProgressDuration, "Unloading_Cargo...");

                    var unloadTimer = new Timer(ProgressDuration * 1000, false);
                    unloadTimer.Tick += (s, e) =>
                    {
                        unloadTimer.Dispose();
                        if (!player.IsConnected || !_sessions.ContainsKey(player.Id)) return;
                        player.ToggleControllable(true);

                        if (session.CycleCount >= MaxCycles)
                        {
                            session.Phase = ForklifterPhase.ReturnToParking;
                            var returnVehicle = BaseVehicle.Find(session.VehicleId) as Vehicle;
                            var returnPos = returnVehicle?.SpawnPosition ?? new Vector3(2753.0f, -2380.0f, 13.4f);
                            SetCheckpoint(player.Id, returnPos, returnPos, CheckpointType.Finish);
                            player.SendClientMessage(Color.White,
                                $"{Msg.Forklifter} Semua box dipindahkan! Kembalikan Forklift ke {{FFFF00}}tempat parkir{{FFFFFF}}.");
                            return;
                        }

                        var loadIdx = NextIndex(session.LoadQueue, LoadPositions.Length);
                        var nextLoadIdx = PeekOrFallback(session.LoadQueue, (loadIdx + 1) % LoadPositions.Length);
                        SetCheckpoint(player.Id, LoadPositions[loadIdx], LoadPositions[nextLoadIdx], CheckpointType.Normal);

                        session.Phase = ForklifterPhase.GoToLoad;
                        player.SendClientMessage(Color.White,
                            $"{Msg.Forklifter} Kargo diturunkan! Kembali ke {{FFFF00}}titik muat{{FFFFFF}}. ({session.CycleCount}/{MaxCycles})");
                    };
                    break;

                case ForklifterPhase.ReturnToParking:
                    ClearCheckpoint(player.Id);
                    var finishVehicle = BaseVehicle.Find(session.VehicleId) as Vehicle;
                    FinalizeJob(player, finishVehicle);
                    break;
            }
        }

        private static void FinalizeJob(Player player, Vehicle? vehicle)
        {
            _sessions.Remove(player.Id);
            ClearCheckpoint(player.Id);
            SideJobVehicleManager.StopAndEject(player);

            if (player.ProgressBarData.IsActive)
                ProgressBarService.DestroyProgressBar(player);

            DelayService.SetJobDelay(player, "forklifter", DelayMinutes);
            PaycheckService.GivePaycheck(player, PaycheckAmount, "Sidejob(Forklift)");

            player.SendClientMessage(Color.White,
                $"{Msg.Forklifter} Kerja bagus! Paycheck {{00FF00}}{Utilities.GroupDigits(PaycheckAmount)}{{FFFFFF}} ditambahkan dan delay {{FF6347}}{DelayMinutes} menit{{FFFFFF}} dimulai.");

            if (vehicle != null)
                SideJobVehicleManager.ScheduleRespawn(vehicle);
        }

        private static void CancelJob(Player player)
        {
            _sessions.Remove(player.Id);
            ClearCheckpoint(player.Id);
            player.ToggleControllable(true);
            if (player.ProgressBarData.IsActive)
                ProgressBarService.DestroyProgressBar(player);
        }

        private static void SetCheckpoint(int playerId, Vector3 pos, Vector3 nextPos, CheckpointType type)
        {
            ClearCheckpoint(playerId);

            var player = BasePlayer.Find(playerId) as Player;
            if (player == null) return;

            var cp = new DynamicRaceCheckpoint(type, pos, nextPos, CheckpointSize, -1, -1, player, 1000.0f);
            cp.Enter += (s, e) =>
            {
                if (e.Player != player || !_sessions.TryGetValue(playerId, out var session)) return;
                Process(player, session);
            };

            _checkpoints[playerId] = cp;
        }

        private static void ClearCheckpoint(int playerId)
        {
            if (_checkpoints.TryGetValue(playerId, out var cp))
            {
                cp.Dispose();
                _checkpoints.Remove(playerId);
            }
        }

        private static Queue<int> BuildShuffledQueue(int count)
        {
            return new Queue<int>(Enumerable.Range(0, count).OrderBy(_ => _rng.Next()));
        }

        private static int NextIndex(Queue<int> queue, int totalCount)
        {
            if (queue.Count == 0)
            {
                foreach (var i in Enumerable.Range(0, totalCount).OrderBy(_ => _rng.Next()))
                    queue.Enqueue(i);
            }
            return queue.Dequeue();
        }

        private static int PeekOrFallback(Queue<int> queue, int fallback)
        {
            return queue.Count > 0 ? queue.Peek() : fallback;
        }
    }
}