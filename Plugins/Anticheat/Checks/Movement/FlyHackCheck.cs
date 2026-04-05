using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Data;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Movement;

public class FlyHackCheck
{
    private const float MaxLiftZ = 0.22f;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public FlyHackCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player) {
        if (!_config.Enabled) return;
        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;
        if (now - st.SetPosTick < 2000) return;
        if (now - st.PlayerVelocityTick < 2000) return;

        var vel = player.Velocity;
        var pState = player.State;

        if (pState == PlayerState.OnFoot) {
            if (!_config.GetCheck("FlyHackOnfoot").Enabled) return;

            if (player.SpecialAction == SpecialAction.Usejetpack) return;

            if (st.IsParachuting) return;
            if (player.SurfingVehicle is not null) return;

            if (vel.Z > MaxLiftZ)
                _warnings.AddWarning(player.Id, "FlyHackOnfoot", $"vz={vel.Z:F3}");
        } else if (pState == PlayerState.Driving) {
            if (player.Vehicle is null) return;

            int model = (int)player.Vehicle.Model;
            byte vType = VehicleData.GetType(model);
            if (vType is 3 or 4 or 8) return;

            bool isBike = VehicleData.IsBike(model);

            if (!_config.GetCheck("FlyHackVehicle").Enabled) return;
            if (now - st.VehicleVelocityTick < 2000) return;

            if (vel.Z > MaxLiftZ) {
                _warnings.AddWarning(player.Id, "FlyHackVehicle", $"vz={vel.Z:F3}");
            }
        }
    }
}