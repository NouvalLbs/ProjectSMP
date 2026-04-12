using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Combat;

public class QuickTurnCheck
{
    private const int MinSpeed = 15;
    private const int MaxSpeedDiff = 25;

    private readonly PlayerStateManager _players;
    private readonly VehicleStateManager _vehicles;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public QuickTurnCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    // Tambah VehicleStateManager di constructor
    public QuickTurnCheck(PlayerStateManager p, VehicleStateManager v, WarningManager w, AnticheatConfig c)
        => (_players, _vehicles, _warnings, _config) = (p, v, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("QuickTurn").Enabled) return;
        if (player.State != PlayerState.Driving) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;
        if (now - st.VehicleVelocityTick < 2000) return;
        if (now - st.EnterVehicleTick < 2000) return;

        var vehicle = player.Vehicle;
        if (vehicle is null) return;

        var vst = _vehicles?.Get(vehicle.Id);
        if (vst is null) return;

        var vel = vehicle.Velocity;
        float spd = (int)(MathF.Sqrt(vel.X * vel.X + vel.Y * vel.Y + vel.Z * vel.Z) * 179.28625f);
        if (spd <= MinSpeed) return;

        float curAngle = vehicle.Rotation.Z;
        float lastAngle = st.LastVehicleZAngle;
        if (lastAngle < 0f) { st.LastVehicleZAngle = curAngle; st.LastVehVelX = vel.X; st.LastVehVelY = vel.Y; st.LastVehVelZ = vel.Z; return; }

        float speedDiff = MathF.Abs(spd - st.LastVehicleSpeed);
        if (speedDiff >= MaxSpeedDiff) { st.LastVehicleZAngle = curAngle; st.LastVehicleSpeed = spd; return; }

        float angleDiff = MathF.Abs(curAngle - lastAngle);
        if (angleDiff > 180f) angleDiff = 360f - angleDiff;

        bool angle180 = MathF.Round(angleDiff) == 180f;
        bool xInverted = (vel.X < 0f) != (st.LastVehVelX < 0f);
        bool yInverted = (vel.Y < 0f) != (st.LastVehVelY < 0f);
        bool zInverted = (vel.Z < 0f) != (st.LastVehVelZ < 0f);

        if (angle180 && xInverted && yInverted && zInverted)
            _warnings.AddWarning(player.Id, "QuickTurn",
                $"spd={spd} diff={speedDiff:F1} angle={angleDiff:F1}");

        st.LastVehicleZAngle = curAngle;
        st.LastVehicleSpeed = spd;
        st.LastVehVelX = vel.X;
        st.LastVehVelY = vel.Y;
        st.LastVehVelZ = vel.Z;
    }
}