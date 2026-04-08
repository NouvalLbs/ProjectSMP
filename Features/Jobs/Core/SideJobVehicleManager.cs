#nullable enable
using ProjectSMP.Entities;
using ProjectSMP.Entities.Vehicles.Handbrake;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System.Collections.Generic;

namespace ProjectSMP.Features.Jobs.Core
{
    public static class SideJobVehicleManager
    {
        private static readonly Dictionary<int, Timer> _pending = new();
        private static readonly Dictionary<int, VehicleSpawnData> _spawnData = new();

        private class VehicleSpawnData
        {
            public VehicleModelType Model { get; set; }
            public Vector3 Position { get; set; }
            public float Rotation { get; set; }
            public int Color1 { get; set; }
            public int Color2 { get; set; }
        }

        public static void RegisterVehicle(int vehicleId, VehicleModelType model, Vector3 position, float rotation, int color1, int color2)
        {
            _spawnData[vehicleId] = new VehicleSpawnData
            {
                Model = model,
                Position = position,
                Rotation = rotation,
                Color1 = color1,
                Color2 = color2
            };
        }

        public static void ScheduleRespawn(Vehicle vehicle, int delayMs = 3000)
        {
            if (vehicle == null || vehicle.IsDisposed) return;

            var vid = vehicle.Id;
            Cancel(vid);

            if (!_spawnData.TryGetValue(vid, out var data))
            {
                vehicle.RespawnAtSpawnPoint();
                return;
            }

            var t = new Timer(delayMs, false);
            t.Tick += (s, e) =>
            {
                t.Dispose();
                _pending.Remove(vid);

                var v = BaseVehicle.Find(vid) as Vehicle;
                if (v == null || v.IsDisposed) return;

                if (IsOccupied(vid))
                {
                    ScheduleRespawn(v, delayMs);
                    return;
                }

                TrueRespawn(v, data);
            };

            _pending[vid] = t;
        }

        public static void EjectAndScheduleRespawn(Player player, Vehicle vehicle, int delayMs = 3000)
        {
            if (player != null && !player.IsDisposed && player.IsConnected)
                player.RemoveFromVehicle();

            ScheduleRespawn(vehicle, delayMs);
        }

        public static bool IsPendingRespawn(int vehicleId) => _pending.ContainsKey(vehicleId);

        private static void Cancel(int vid)
        {
            if (!_pending.TryGetValue(vid, out var t)) return;
            t.Dispose();
            _pending.Remove(vid);
        }

        private static bool IsOccupied(int vid)
        {
            foreach (var p in BasePlayer.All)
                if (p.IsConnected && p.Vehicle?.Id == vid) return true;
            return false;
        }

        private static void TrueRespawn(Vehicle oldVehicle, VehicleSpawnData data)
        {
            oldVehicle.Dispose();

            var newVehicle = Vehicle.CreateVehicle(data.Model, data.Position, data.Rotation, data.Color1, data.Color2, 60);
            newVehicle.VehicleType = VehicleType.Job;

            _spawnData[newVehicle.Id] = data;
        }

        public static void Dispose()
        {
            foreach (var t in _pending.Values) t.Dispose();
            _pending.Clear();
            _spawnData.Clear();
        }
    }
}