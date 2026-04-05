using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.AntiNop;

public class NopSetVehicleHealthCheck
{
    private const long DeadlineMs = 2850;
    private const float Tolerance = 50f;

    private readonly PlayerStateManager _players;
    private readonly VehicleStateManager _vehicles;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public NopSetVehicleHealthCheck(PlayerStateManager p, VehicleStateManager v, WarningManager w, AnticheatConfig c)
        => (_players, _vehicles, _warnings, _config) = (p, v, w, c);

    public void OnSetVehicleHealth(int vehicleId, float health)
    {
        var vst = _vehicles.Get(vehicleId);
        if (vst is null) return;

        vst.NopSetHealthExpected = health;
        vst.NopSetHealthDeadline = Environment.TickCount64 + DeadlineMs;
    }

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("NopSetVehicleHealth").Enabled) return;
        if (player.State != PlayerState.Driving) return;

        var pst = _players.Get(player.Id);
        if (pst is null || pst.IsDead) return;

        long now = Environment.TickCount64;
        if (now - pst.SpawnTick < 3000) return;
        if (now - pst.EnterVehicleTick < 2000) return;

        var vehicle = player.Vehicle;
        if (vehicle is null) return;

        var vst = _vehicles.Get(vehicle.Id);
        if (vst is null || vst.NopSetHealthExpected < 0f) return;
        if (now < vst.NopSetHealthDeadline) return;

        float actual = vehicle.Health;

        if (actual > vst.NopSetHealthExpected + Tolerance)
            _warnings.AddWarning(player.Id, "NopSetVehicleHealth",
                $"veh={vehicle.Id} expected={vst.NopSetHealthExpected:F1} got={actual:F1}");

        vst.NopSetHealthExpected = -1f;
    }

    public void OnVehicleDestroyed(int vehicleId)
    {
        var vst = _vehicles.Get(vehicleId);
        if (vst is not null) vst.NopSetHealthExpected = -1f;
    }

    public void OnRepairVehicle(int vehicleId)
    {
        var vst = _vehicles.Get(vehicleId);
        if (vst is null) return;

        vst.NopSetHealthExpected = 1000f;
        vst.NopSetHealthDeadline = Environment.TickCount64 + DeadlineMs;
    }
}