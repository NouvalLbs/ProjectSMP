using ProjectSMP.Core;
using SampSharp.GameMode;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Entities.Vehicles.Handbrake
{
    internal sealed class HandbrakeState
    {
        public bool Engaged;
        public Vector3 Position;
        public float Angle;
    }

    public static class HandbrakeService
    {
        private static readonly Dictionary<int, HandbrakeState> _states = new();
        private static Timer _timer = null!;

        public static void Initialize()
        {
            _timer = new Timer(1000, true);
            _timer.Tick += OnTick;
        }

        public static void Dispose() => _timer?.Dispose();

        public static void OnPlayerExitVehicle(Player player, Vehicle vehicle)
        {
            if (!_states.TryGetValue(vehicle.Id, out var state)) return;
            state.Position = vehicle.Position;
            state.Angle = vehicle.Angle;
        }

        public static void OnVehicleRemoved(int vehicleId) => _states.Remove(vehicleId);

        public static void SetHandbrake(Player player, Vehicle vehicle, bool engage)
        {
            if (!player.Settings.ToggleAutoHandbrake) return;
            if (!vehicle.IsACar) return;

            if (!_states.TryGetValue(vehicle.Id, out var state))
            {
                state = new HandbrakeState();
                _states[vehicle.Id] = state;
            }

            if (state.Engaged == engage) return;
            state.Engaged = engage;

            if (engage)
            {
                state.Position = vehicle.Position;
                state.Angle = vehicle.Angle;
                player.SendClientMessage(-1, $"{Msg.Vehicles} Handbrakes {{00FF00}}ON");
            }
            else
            {
                player.SendClientMessage(-1, $"{Msg.Vehicles} Handbrakes {{FF0000}}OFF");
            }
        }

        public static void EngageSilent(Vehicle vehicle)
        {
            if (!vehicle.IsACar) return;
            var state = new HandbrakeState {
                Engaged = true,
                Position = vehicle.Position,
                Angle = vehicle.Angle
            };
            _states[vehicle.Id] = state;
        }

        public static bool IsEngaged(int vehicleId) =>
            _states.TryGetValue(vehicleId, out var s) && s.Engaged;

        private static void OnTick(object? sender, EventArgs e)
        {
            foreach (var (vehicleId, state) in _states)
            {
                if (!state.Engaged) continue;

                var vehicle = BaseVehicle.Find(vehicleId) as Vehicle;
                if (vehicle == null || !vehicle.IsACar) continue;

                if (vehicle.Position.DistanceTo(state.Position) >= 2.0f)
                {
                    vehicle.Position = state.Position;
                    vehicle.Angle = state.Angle;
                }
            }
        }
    }
}