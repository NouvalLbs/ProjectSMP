#nullable enable
using ProjectSMP.Plugins.ColAndreas;
using ProjectSMP.Plugins.EVF2;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Entities.Vehicles.Impact
{
    public class VehicleImpactArgs : EventArgs
    {
        public int VehicleId { get; set; }
        public int DriverId { get; set; }
        public float Force { get; set; }
        public Vector3 DeltaVelocity { get; set; }
        public Vector3 ImpactPosition { get; set; }
        public bool ImpactConfirmed { get; set; }
    }

    public static class VehicleImpactService
    {
        private static readonly Dictionary<int, Vector3> _lastVel = new();
        private static readonly Dictionary<int, int> _cooldown = new();
        private static Timer _timer = null!;

        public const float MinImpactForce = 0.12f;
        public const int CooldownTicks = 5;
        private const int Interval = 100;

        public static event EventHandler<VehicleImpactArgs>? VehicleImpacted;

        public static void Initialize()
        {
            _timer = new Timer(Interval, true);
            _timer.Tick += OnTick;
            EVFService.VehicleDestroyed += (_, id) => ClearVehicle(id);
        }

        public static void Dispose()
        {
            _timer?.Dispose();
            _lastVel.Clear();
            _cooldown.Clear();
        }

        public static void ClearVehicle(int vehicleId)
        {
            _lastVel.Remove(vehicleId);
            _cooldown.Remove(vehicleId);
        }

        private static void OnTick(object? sender, EventArgs e)
        {
            foreach (var bp in BasePlayer.All)
            {
                if (bp is not Player p || p.IsDisposed || p.State != PlayerState.Driving) continue;

                var veh = p.Vehicle as Vehicle;
                if (veh == null) continue;

                int vid = veh.Id;
                var vel = veh.Velocity;

                if (!_lastVel.TryGetValue(vid, out var last))
                {
                    _lastVel[vid] = vel;
                    continue;
                }

                _lastVel[vid] = vel;

                if (_cooldown.TryGetValue(vid, out int cd) && cd > 0)
                {
                    _cooldown[vid] = cd - 1;
                    continue;
                }

                float lastSpd = Magnitude(last);
                float curSpd = Magnitude(vel);

                if (curSpd >= lastSpd) continue;

                float dx = vel.X - last.X;
                float dy = vel.Y - last.Y;
                float dz = vel.Z - last.Z;
                float force = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);

                if (force < MinImpactForce) continue;

                _cooldown[vid] = CooldownTicks;

                var pos = veh.Position;
                bool confirmed = false;
                var impactPos = pos;

                if (lastSpd > 0.01f)
                {
                    float nx = last.X / lastSpd;
                    float ny = last.Y / lastSpd;
                    float nz = last.Z / lastSpd;

                    int hit = ColAndreasService.RayCastLine(
                        pos.X, pos.Y, pos.Z + 0.5f,
                        pos.X + nx * 3f, pos.Y + ny * 3f, pos.Z + 0.5f + nz * 3f,
                        out float hx, out float hy, out float hz);

                    if (hit != 0)
                    {
                        confirmed = true;
                        impactPos = new Vector3(hx, hy, hz);
                    }
                }

                VehicleImpacted?.Invoke(null, new VehicleImpactArgs
                {
                    VehicleId = vid,
                    DriverId = p.Id,
                    Force = force,
                    DeltaVelocity = new Vector3(dx, dy, dz),
                    ImpactPosition = impactPos,
                    ImpactConfirmed = confirmed
                });
            }
        }

        private static float Magnitude(Vector3 v)
            => (float)Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);

        public static float ForceToKmh(float force) => force * 180f;
    }
}