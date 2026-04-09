#nullable enable
using ProjectSMP.Core;
using ProjectSMP.Entities;
using ProjectSMP.Entities.Players.Delay;
using ProjectSMP.Extensions;
using ProjectSMP.Features.Bank.Paycheck;
using ProjectSMP.Features.Jobs.Core;
using ProjectSMP.Features.Jobs.Side.Trashmaster.DynamicTrash;
using ProjectSMP.Features.ProgressBar;
using ProjectSMP.Plugins.EVF2;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using SampSharp.Streamer.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Features.Jobs.Side.Trashmaster
{
    public static class TrashmasterService
    {
        private const int MaxTrash = 10;
        private const int DelayMinutes = 25;
        private const int PaycheckAmount = 30000;
        private const int CollectDuration = 5;
        private const int AttachmentIndex = 4;
        private const float CpSize = 6.0f;

        private static readonly HashSet<int> _vehicleIds = new();
        private static readonly Dictionary<int, TrashmasterSession> _sessions = new();
        private static readonly Dictionary<int, DynamicRaceCheckpoint> _checkpoints = new();
        private static readonly Random _rng = new();
        private static readonly Dictionary<int, Timer> _exitTimers = new();
        private static readonly Dictionary<int, int> _exitCountdown = new();

        private static DynamicPickup? _dropoutPickup;
        private static DynamicTextLabel? _dropoutLabel;

        private static readonly Vector3 DropoutPos = new(2101.9451f, -2121.9099f, 13.6328f);
        private const float DropoutInteractRadius = 5.0f;

        private static readonly (float X, float Y, float Z, float A)[] SpawnPoints =
        {
            (2098.9634f, -2090.6951f, 14.1241f, 173.4436f),
            (2105.5652f, -2090.2820f, 14.1237f, 174.1630f),
            (2111.7312f, -2091.3362f, 14.1265f, 177.4669f),
            (2117.8586f, -2093.3643f, 14.1314f, 172.5858f)
        };

        public static void Initialize()
        {
            foreach (var (x, y, z, a) in SpawnPoints)
            {
                var v = Vehicle.CreateVehicle((VehicleModelType)408, new Vector3(x, y, z), a, -1, -1, 60);
                v.VehicleType = VehicleType.Job;
                _vehicleIds.Add(v.Id);
                SideJobVehicleManager.RegisterVehicle(v.Id, (VehicleModelType)408, new Vector3(x, y, z), a, -1, -1);
            }

            _dropoutPickup = new DynamicPickup(1239, 23, DropoutPos);
            _dropoutLabel = new DynamicTextLabel(
                "{C6E2FF}[Trashmaster]\n{FFFFFF}Tekan '{FF0000}H{FFFFFF}' untuk drop sampah",
                Color.White, DropoutPos + new Vector3(0, 0, 0.5f), 10.0f);
        }

        public static void Dispose()
        {
            foreach (var cp in _checkpoints.Values) cp.Dispose();
            _checkpoints.Clear();
            _dropoutPickup?.Dispose();
            _dropoutLabel?.Dispose();
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

            if (!_sessions.ContainsKey(player.Id))
            {
                SideJobVehicleManager.ScheduleRespawn(vehicle);
                return;
            }

            StartExitCountdown(player);
        }

        public static void OnPlayerDisconnect(Player player)
        {
            if (!_sessions.TryGetValue(player.Id, out var session)) return;

            _sessions.Remove(player.Id);
            CancelExitCountdown(player.Id);
            ClearCheckpoint(player.Id);
            CleanupAttachment(player);

            var v = BaseVehicle.Find(session.VehicleId) as Vehicle;
            if (v != null) SideJobVehicleManager.ScheduleRespawn(v);
        }

        public static void OnPlayerStateChanged(Player player, PlayerState newState, PlayerState oldState)
        {
            if (newState != PlayerState.Driving) return;
            var vehicle = player.Vehicle as Vehicle;
            if (vehicle == null || !_vehicleIds.Contains(vehicle.Id)) return;

            if (!player.IsCharLoaded) return;
            if (_sessions.TryGetValue(player.Id, out var existingSession))
            {
                if (vehicle.Id == existingSession.VehicleId)
                    CancelExitCountdown(player.Id);
                return;
            }

            if (SideJobVehicleManager.IsPendingRespawn(vehicle.Id)) return;
            ShowStartDialog(player);
        }

        public static bool HandleYKey(Player player)
        {
            if (!_sessions.TryGetValue(player.Id, out var session)) return false;
            if (session.Phase != TrashmasterPhase.GoToTrash) return false;
            if (session.CurrentTrashId == -1) return false;

            var trash = TrashService.GetTrash(session.CurrentTrashId);
            if (trash == null || trash.Amount <= 0) return false;

            var trashPos = new Vector3(trash.PosX, trash.PosY, trash.PosZ);
            if (player.Position.DistanceTo(trashPos) > 3.0f) return false;

            StartCollecting(player, session, session.CurrentTrashId);
            return true;
        }

        public static void HandleDropoutInteract(Player player)
        {
            if (!_sessions.TryGetValue(player.Id, out var session)) return;
            if (session.Phase != TrashmasterPhase.GoToDropout) return;
            if (player.Position.DistanceTo(DropoutPos) > DropoutInteractRadius) return;

            FinalizeJob(player, session);
        }

        private static void ShowStartDialog(Player player)
        {
            player.ShowMessage("Side Job - Trashmaster", "Anda akan bekerja sebagai Trashmaster?")
                .WithButtons("Start Job", "Close")
                .Show(e =>
                {
                    var v = player.Vehicle as Vehicle;
                    if (e.DialogButton != DialogButton.Left)
                    {
                        if (v != null) SideJobVehicleManager.EjectAndScheduleRespawn(player, v);
                        return;
                    }

                    if (DelayService.HasJobDelay(player, "trashmaster"))
                    {
                        var rem = DelayService.GetJobDelay(player, "trashmaster");
                        player.SendClientMessage(Color.White,
                            $"{Msg.Trashmaster} Kamu harus menunggu {{FF6347}}{rem} menit{{FFFFFF}} sebelum bekerja sebagai Trashmaster lagi.");
                        if (v != null) SideJobVehicleManager.EjectAndScheduleRespawn(player, v);
                        return;
                    }

                    ShowBriefing(player);
                });
        }

        private static void ShowBriefing(Player player)
        {
            const string text =
                "{FFFFFF}Kamu ditugaskan untuk membantu proses pengangkutan sampah di wilayah kota Los Santos. " +
                "Mulailah dengan menuju kendaraan Trashmaster, lalu masuk ke dalamnya untuk memulai pekerjaan.\n\n" +
                "Setelah siap, sistem akan mengarahkan kamu ke beberapa titik lokasi sampah melalui waypoint yang telah ditentukan. " +
                "Bawa kendaraan mendekati area tersebut untuk melakukan proses pengambilan sampah. " +
                "Tekan tombol {FFFF00}Y{FFFFFF} saat berada di lokasi agar sampah dapat dimuat ke kendaraan.\n\n" +
                "Setelah sampah berhasil dikumpulkan, lanjutkan dengan menuju bagian belakang kendaraan Trashmaster " +
                "untuk membuang muatan yang telah diambil.\n\n" +
                "{FFFF00}Catatan:\n{FFFFFF}" +
                "Perhatikan indikator Trash Garbage Bin yang menunjukkan jumlah sampah yang sudah berhasil dikumpulkan. " +
                "Jika masih belum penuh, lanjutkan perjalanan ke titik sampah berikutnya yang tersedia.\n" +
                "Apabila indikator menunjukkan jumlah maksimum, berarti seluruh tugas pengangkutan telah selesai " +
                "dan pekerjaan dapat diakhiri.";

            player.ShowMessage("Los Santos Waste Management", text)
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

            var session = new TrashmasterSession
            {
                IsActive = true,
                Phase = TrashmasterPhase.GoToTrash,
                TrashCollected = 0,
                VehicleId = v.Id,
                CurrentTrashId = -1
            };
            _sessions[player.Id] = session;

            player.SendClientMessage(Color.White, $"{Msg.Trashmaster} Pekerjaan dimulai! Ikuti checkpoint menuju lokasi sampah.");
            GoToNextTrash(player, session);
        }

        private static void GoToNextTrash(Player player, TrashmasterSession session)
        {
            var available = TrashService.GetAvailableTrashIds();
            if (available.Count == 0)
            {
                player.SendClientMessage(Color.White, $"{Msg.Trashmaster} Tidak ada sampah tersedia saat ini. Pekerjaan dibatalkan.");
                CancelJob(player);
                return;
            }

            var trashId = available[_rng.Next(available.Count)];
            var trash = TrashService.GetTrash(trashId)!;
            session.CurrentTrashId = trashId;
            session.Phase = TrashmasterPhase.GoToTrash;

            var pos = new Vector3(trash.PosX, trash.PosY, trash.PosZ);
            SetCheckpoint(player, pos, pos, CheckpointType.Finish);

            player.SendClientMessage(Color.White,
                $"{Msg.Trashmaster} Menuju lokasi sampah! Turun lalu tekan {{FFFF00}}Y{{FFFFFF}} untuk mengambil. ({session.TrashCollected}/{MaxTrash})");
        }

        private static void StartCollecting(Player player, TrashmasterSession session, int trashId)
        {
            session.Phase = TrashmasterPhase.Collecting;
            ClearCheckpoint(player.Id);
            player.ToggleControllable(false);
            ProgressBarService.StartProgress(player, CollectDuration, "Collecting_Trash...", animIndex: 294, animLib: "CASINO", animName: "cards_win");

            var t = new Timer(CollectDuration * 1000, false);
            t.Tick += (s, e) =>
            {
                t.Dispose();
                if (!player.IsConnected || !_sessions.TryGetValue(player.Id, out var sess)) return;

                player.ToggleControllable(true);
                if (player.IsAttachedObjectSlotUsed(AttachmentIndex))
                    player.RemoveAttachedObject(AttachmentIndex);

                player.SetAttachedObjectSafe(AttachmentIndex, 1264, (Bone)6,
                    new Vector3(0.222f, 0.024f, 0.128f),
                    new Vector3(1.90f, -90.0f, 0.0f),
                    new Vector3(0.5f, 0.5f, 0.5f),
                    0, 0);

                TrashService.CollectAmount(trashId, 10);

                var trunkPos = EVFVehiclePart.GetPosNearVehiclePart(sess.VehicleId, VehiclePart.Trunk, 0.5f);
                sess.Phase = TrashmasterPhase.GoToTrunk;
                SetCheckpoint(player, trunkPos, trunkPos, CheckpointType.Finish);

                player.SendClientMessage(Color.White,
                    $"{Msg.Trashmaster} Sampah diambil! Bawa ke bagian belakang kendaraan.");
            };
        }

        private static void DumpToTruck(Player player, TrashmasterSession session)
        {
            session.Phase = TrashmasterPhase.Dumping;
            ClearCheckpoint(player.Id);
            player.ToggleControllable(false);
            player.ApplyAnimation("GRENADE", "WEAPON_throw", 4.1f, false, false, false, false, 0);

            var t = new Timer(1500, false);
            t.Tick += (s, e) =>
            {
                t.Dispose();
                if (!player.IsConnected || !_sessions.TryGetValue(player.Id, out var sess)) return;

                player.ToggleControllable(true);
                player.ClearAnimations();

                if (player.IsAttachedObjectSlotUsed(AttachmentIndex))
                    player.RemoveAttachedObject(AttachmentIndex);

                sess.TrashCollected++;
                player.SendClientMessage(Color.White,
                    $"{Msg.Trashmaster} Sampah dimasukkan! ({sess.TrashCollected}/{MaxTrash})");

                if (sess.TrashCollected >= MaxTrash)
                {
                    sess.Phase = TrashmasterPhase.GoToDropout;
                    SetCheckpoint(player, DropoutPos, DropoutPos, CheckpointType.Finish);
                    player.SendClientMessage(Color.White,
                        $"{Msg.Trashmaster} Semua sampah terkumpul! Menuju titik pembuangan. Tekan {{FFFF00}}H{{FFFFFF}} saat tiba.");
                }
                else
                {
                    GoToNextTrash(player, sess);
                }
            };
        }

        private static void FinalizeJob(Player player, TrashmasterSession session)
        {
            _sessions.Remove(player.Id);
            ClearCheckpoint(player.Id);
            CleanupAttachment(player);
            player.RemoveFromVehicle();

            DelayService.SetJobDelay(player, "trashmaster", DelayMinutes);
            PaycheckService.GivePaycheck(player, PaycheckAmount, "Sidejob(Trashmaster)");

            player.SendClientMessage(Color.White,
                $"{Msg.Trashmaster} Pekerjaan selesai! Paycheck {{00FF00}}{Utilities.GroupDigits(PaycheckAmount)}{{FFFFFF}} ditambahkan. " +
                $"Delay {{FF6347}}{DelayMinutes} menit{{FFFFFF}} dimulai.");

            var v = BaseVehicle.Find(session.VehicleId) as Vehicle;
            if (v != null) SideJobVehicleManager.ScheduleRespawn(v);
        }

        private static void CancelJob(Player player)
        {
            _sessions.Remove(player.Id);
            CancelExitCountdown(player.Id);
            ClearCheckpoint(player.Id);
            CleanupAttachment(player);
            player.ToggleControllable(true);
            if (player.ProgressBarData.IsActive)
                ProgressBarService.DestroyProgressBar(player);
        }

        private static void CleanupAttachment(Player player)
        {
            if (player.IsConnected && player.IsAttachedObjectSlotUsed(AttachmentIndex))
                player.RemoveAttachedObject(AttachmentIndex);
        }

        private static void SetCheckpoint(Player player, Vector3 pos, Vector3 next, CheckpointType type)
        {
            ClearCheckpoint(player.Id);
            var cp = new DynamicRaceCheckpoint(type, pos, next, CpSize, -1, -1, player, 1500.0f);
            cp.Enter += (s, e) =>
            {
                if (e.Player != player || !_sessions.TryGetValue(player.Id, out var session)) return;
                switch (session.Phase)
                {
                    case TrashmasterPhase.GoToTrash:
                        ClearCheckpoint(player.Id);
                        player.SendClientMessage(Color.White,
                            $"{Msg.Trashmaster} Kamu telah tiba! Turun lalu tekan {{FFFF00}}Y{{FFFFFF}} untuk mengambil sampah.");
                        break;
                    case TrashmasterPhase.GoToTrunk:
                        DumpToTruck(player, session);
                        break;
                    case TrashmasterPhase.GoToDropout:
                        ClearCheckpoint(player.Id);
                        player.SendClientMessage(Color.White,
                            $"{Msg.Trashmaster} Kamu telah tiba di titik pembuangan! Tekan {{FFFF00}}H{{FFFFFF}} untuk drop sampah.");
                        break;
                }
            };
            _checkpoints[player.Id] = cp;
        }

        private static void ClearCheckpoint(int pid)
        {
            if (_checkpoints.TryGetValue(pid, out var cp)) { cp.Dispose(); _checkpoints.Remove(pid); }
        }

        private static void StartExitCountdown(Player player)
        {
            CancelExitCountdown(player.Id);

            _exitCountdown[player.Id] = 100;
            player.GameText($"~r~Kembali ke kendaraan: ~w~{_exitCountdown[player.Id]}s", 2000, 3);

            var t = new Timer(1000, true);
            t.Tick += (s, e) =>
            {
                if (!player.IsConnected || !_sessions.ContainsKey(player.Id))
                {
                    t.Dispose();
                    _exitTimers.Remove(player.Id);
                    _exitCountdown.Remove(player.Id);
                    return;
                }

                _exitCountdown[player.Id]--;

                if (_exitCountdown[player.Id] <= 0)
                {
                    t.Dispose();
                    _exitTimers.Remove(player.Id);
                    _exitCountdown.Remove(player.Id);

                    player.GameText("~r~Waktu habis! Pekerjaan dibatalkan.", 3000, 3);
                    player.SendClientMessage(Color.White,
                        $"{Msg.Trashmaster} Kamu terlalu lama di luar kendaraan. Pekerjaan dibatalkan!");

                    if (_sessions.TryGetValue(player.Id, out var session))
                    {
                        var v = BaseVehicle.Find(session.VehicleId) as Vehicle;
                        CancelJob(player);
                        if (v != null) SideJobVehicleManager.ScheduleRespawn(v);
                    }
                    return;
                }

                player.GameText($"~r~Kembali ke kendaraan: ~w~{_exitCountdown[player.Id]}s", 2000, 3);
            };

            _exitTimers[player.Id] = t;
        }

        private static void CancelExitCountdown(int playerId)
        {
            if (!_exitTimers.TryGetValue(playerId, out var t)) return;
            t.Dispose();
            _exitTimers.Remove(playerId);
            _exitCountdown.Remove(playerId);
        }
    }
}