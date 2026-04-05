using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Vehicle;

public class VehicleTeleportCheck
{
    private const float MaxEnterDist = 10f;
    private const float MaxPickupTeleportDist = 20f;
    private const float MaxVehicleToPlayerDist = 15f;

    private readonly PlayerStateManager _players;
    private readonly VehicleStateManager _vehicles;
    private readonly PickupStateManager _pickups;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public VehicleTeleportCheck(PlayerStateManager p, VehicleStateManager v, PickupStateManager pk, WarningManager w, AnticheatConfig c)
        => (_players, _vehicles, _pickups, _warnings, _config) = (p, v, pk, w, c);

    public void OnPlayerEnterVehicle(BasePlayer player, EnterVehicleEventArgs e)
    {
        if (!_config.Enabled || !_config.GetCheck("TeleportVehicleEnter").Enabled) return;
        var st = _players.Get(player.Id);
        if (st is null) return;
        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000 || now - st.SetPosTick < 2000 || now - st.PutInVehicleTick < 2000) return;
        var vpos = e.Vehicle.Position;
        float dist = VectorMath.Dist(st.X, st.Y, st.Z, vpos.X, vpos.Y, vpos.Z);
        if (dist > MaxEnterDist)
            _warnings.AddWarning(player.Id, "TeleportVehicleEnter", $"d={dist:F1}");
    }

    public void OnPlayerPickUpPickup(BasePlayer player, PickUpPickupEventArgs e)
    {
        if (!_config.Enabled || !_config.GetCheck("TeleportPickup").Enabled) return;
        var st = _players.Get(player.Id);
        if (st is null) return;
        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000 || now - st.SetPosTick < 2000) return;

        var registered = _pickups.Get(e.Pickup.Id);
        float px = registered?.X ?? e.Pickup.Position.X;
        float py = registered?.Y ?? e.Pickup.Position.Y;
        float pz = registered?.Z ?? e.Pickup.Position.Z;

        float dist = VectorMath.Dist(st.X, st.Y, st.Z, px, py, pz);
        if (dist > MaxPickupTeleportDist)
            _warnings.AddWarning(player.Id, "TeleportPickup", $"d={dist:F1} registered={registered is not null}");
    }

    public void OnPlayerStateChanged(BasePlayer player, StateEventArgs e)
    {
        if (!_config.Enabled || !_config.GetCheck("TeleportVehicleToPlayer").Enabled) return;
        if (e.OldState != PlayerState.Driving || e.NewState != PlayerState.OnFoot) return;
        var st = _players.Get(player.Id);
        if (st is null) return;
        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000 || now - st.SetPosTick < 2000
            || now - st.PutInVehicleTick < 2000 || now - st.RemoveFromVehicleTick < 1500) return;
        var pos = player.Position;
        float dist = VectorMath.Dist(st.X, st.Y, st.Z, pos.X, pos.Y, pos.Z);

        if (dist > MaxVehicleToPlayerDist) {
            bool isRespawn = false;

            if (st.VehicleId != -1) {
                var vst = _vehicles.Get(st.VehicleId);
                if (vst is not null)
                {
                    float spawnDist = VectorMath.Dist(pos.X, pos.Y, pos.Z,
                        vst.SpawnPosX, vst.SpawnPosY, vst.SpawnPosZ);
                    if (spawnDist < 15.0f) isRespawn = true;
                }
            }

            if (!isRespawn)
                _warnings.AddWarning(player.Id, "TeleportVehicleToPlayer", $"d={dist:F1}");
        }
    }
}