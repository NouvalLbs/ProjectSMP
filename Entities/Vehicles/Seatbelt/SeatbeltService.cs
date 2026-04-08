#nullable enable
using ProjectSMP.Core;
using ProjectSMP.Entities.Vehicles.Impact;
using ProjectSMP.Features.Drunk;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Entities.Vehicles.Seatbelt
{
    public static class SeatbeltService
    {
        private static readonly Dictionary<int, bool> _seatbelts = new();

        public static void Initialize()
        {
            VehicleImpactService.VehicleImpacted += OnVehicleImpacted;
        }

        public static void Dispose()
        {
            VehicleImpactService.VehicleImpacted -= OnVehicleImpacted;
            _seatbelts.Clear();
        }

        public static void OnPlayerStateChanged(Player player, PlayerState newState)
        {
            if (newState != PlayerState.Driving) return;

            var auto = player.Settings.ToggleSeatbeltHelmet;
            _seatbelts[player.Id] = auto;

            if (auto)
                player.SendClientMessage(-1, $"{Msg.Vehicles} Seatbelt {{00FF00}}ON{{FFFFFF}}");
        }

        public static void OnPlayerExitVehicle(Player player)
        {
            if (IsWearing(player)) {
                _seatbelts[player.Id] = false;
                player.SendClientMessage(-1, $"{Msg.Vehicles} Seatbelt {{FF0000}}OFF{{FFFFFF}}");
            }
        }

        public static void OnPlayerDisconnect(Player player)
        {
            _seatbelts.Remove(player.Id);
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

            var level = Math.Clamp((int)(e.Force * 400), 30, 150) * 100;
            DrunkManager.SetDrunk(player, DrunkSource.Seatbelt, level, decayPerTick: 100);
        }
    }
}