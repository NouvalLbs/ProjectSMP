#nullable enable
using ProjectSMP.Core;
using ProjectSMP.Entities.Vehicles.Impact;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Entities.Vehicles.Seatbelt
{
    public static class SeatbeltService
    {
        private static readonly Dictionary<int, bool> _seatbelts = new();
        private static readonly Dictionary<int, int> _drunkTicks = new();
        private static SampSharp.GameMode.SAMP.Timer _timer = null!;

        public static void Initialize()
        {
            VehicleImpactService.VehicleImpacted += OnVehicleImpacted;
            _timer = new SampSharp.GameMode.SAMP.Timer(100, true);
            _timer.Tick += OnTick;
        }

        public static void Dispose()
        {
            VehicleImpactService.VehicleImpacted -= OnVehicleImpacted;
            _timer?.Dispose();
            _seatbelts.Clear();
            _drunkTicks.Clear();
        }

        public static void OnPlayerStateChanged(Player player, PlayerState newState)
        {
            if (newState != PlayerState.Driving) return;

            var auto = player.Settings.ToggleSeatbeltHelmet;
            _seatbelts[player.Id] = auto;

            if (auto)
                player.SendClientMessage(-1, $"{Msg.Vehicles} Seatbelt {{00FF00}}ON{{FFFFFF}}");
            else
                player.SendClientMessage(-1, $"{Msg.Vehicles} Seatbelt {{FF0000}}OFF{{FFFFFF}}");
        }

        public static void OnPlayerExitVehicle(Player player)
        {
            _seatbelts[player.Id] = false;
        }

        public static void OnPlayerDisconnect(Player player)
        {
            _seatbelts.Remove(player.Id);
            _drunkTicks.Remove(player.Id);
        }

        public static void ToggleSeatbelt(Player player)
        {
            if (player.State != PlayerState.Driving)
            {
                player.SendClientMessage(-1, $"{Msg.Error} Kamu harus berada di dalam kendaraan.");
                return;
            }

            var current = IsWearing(player);
            _seatbelts[player.Id] = !current;

            var status = _seatbelts[player.Id] ? "{00FF00}ON" : "{FF0000}OFF";
            player.SendClientMessage(-1, $"{Msg.Vehicles} Seatbelt {status}");
        }

        public static bool IsWearing(Player player) =>
            _seatbelts.TryGetValue(player.Id, out var v) && v;

        private static void OnVehicleImpacted(object? sender, VehicleImpactArgs e)
        {
            if (BasePlayer.Find(e.DriverId) is not Player player || player.IsDisposed) return;
            if (IsWearing(player)) return;

            var durationTicks = Math.Clamp((int)(e.Force * 400), 30, 150);
            _drunkTicks[player.Id] = durationTicks;
        }

        private static void OnTick(object? sender, EventArgs e)
        {
            foreach (var id in new List<int>(_drunkTicks.Keys))
            {
                if (BasePlayer.Find(id) is not Player p || p.IsDisposed)
                {
                    _drunkTicks.Remove(id);
                    continue;
                }

                _drunkTicks[id]--;
                p.DrunkLevel = 2000;

                if (_drunkTicks[id] <= 0)
                {
                    _drunkTicks.Remove(id);
                    p.DrunkLevel = 0;
                }
            }
        }
    }
}