using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Data;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Movement;

public class SpeedHackCheck
{
    private const float MaxOnfootSpeed = 0.44f;
    private const float MaxOnfootJetpack = 0.34f;
    private const float MaxVehicleDefault = 0.95f;
    private const float MaxBoatSpeed = 0.50f;
    private const float MaxBikeSpeed = 0.65f;
    private const float MaxHelicopterSpeed = 0.80f;
    private const float MaxAircraftSpeed = 1.90f;
    private const float MaxPlaneSpeed = 1.50f;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly VehicleStateManager _vehicles;
    private readonly AnticheatConfig _config;

    public SpeedHackCheck(PlayerStateManager p, VehicleStateManager v, WarningManager w, AnticheatConfig c)
        => (_players, _vehicles, _warnings, _config) = (p, v, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        var st = _players.Get(player.Id);
        if (st is null || st.IsDead || !_config.Enabled) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;
        if (now - st.PlayerVelocityTick < 2000) return;

        var vel = player.Velocity;
        float spd = VectorMath.Speed(vel.X, vel.Y, vel.Z);

        if (player.State == PlayerState.OnFoot)
        {
            if (!_config.GetCheck("SpeedHackOnfoot").Enabled) return;
            float limit = player.SpecialAction == SpecialAction.Usejetpack
                ? MaxOnfootJetpack : MaxOnfootSpeed;
            if (spd > limit)
                _warnings.AddWarning(player.Id, "SpeedHackOnfoot", $"spd={spd:F3} lim={limit:F2}");
        }
        else if (player.State == PlayerState.Driving)
        {
            if (!_config.GetCheck("SpeedHackVehicle").Enabled) return;
            long delayMs = _config.SpeedHackVehResetDelay * 1000L;
            if (now - st.VehicleVelocityTick < delayMs) return;

            if (player.Vehicle is null) return;
            int model = (int)player.Vehicle.Model;
            float limit = model >= 400 ? GetLimit(model) : MaxVehicleDefault;

            var vst = _vehicles.Get(player.Vehicle.Id);
            if (vst is not null && vst.TrSpeed != -1) {
                int currentSpeed = (int)(spd * 100.0f);
                int speedDiff = currentSpeed - vst.TrSpeed;
                if (speedDiff > 0)
                {
                    if (speedDiff > vst.TrSpeedDiff) vst.TrSpeedDiff = speedDiff;
                    spd -= vst.TrSpeedDiff / 100.0f;
                }
            }

            if (spd > limit)
                _warnings.AddWarning(player.Id, "SpeedHackVehicle", $"spd={spd:F3} lim={limit:F2} mdl={model}");
        }
    }

    private static float GetLimit(int model) => VehicleData.GetType(model) switch
    {
        1 => MaxBoatSpeed,
        3 => MaxHelicopterSpeed,
        4 => MaxAircraftSpeed,
        5 or 6 => MaxBikeSpeed,
        _ => IsHighSpeedPlane(model) ? MaxPlaneSpeed : MaxVehicleDefault
    };

    private static bool IsHighSpeedPlane(int model)
        => model is 592 or 577 or 511 or 512 or 593 or 520 or 553 or 476 or 519 or 460 or 513 or 548;
}