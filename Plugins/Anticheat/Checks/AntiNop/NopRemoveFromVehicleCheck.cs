using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.AntiNop;

public class NopRemoveFromVehicleCheck
{
    private const long DeadlineMs = 4650;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public NopRemoveFromVehicleCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnRemovePlayerFromVehicle(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        st.NopRemoveFromVehiclePending = true;
        st.NopRemoveFromVehicleDeadline = Environment.TickCount64 + DeadlineMs;
    }

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("NopRemoveFromVehicle").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead || !st.NopRemoveFromVehiclePending) return;

        long now = Environment.TickCount64;
        if (now < st.NopRemoveFromVehicleDeadline) return;
        if (now - st.SpawnTick < 3000) { st.NopRemoveFromVehiclePending = false; return; }
        if (now - st.PutInVehicleTick < 2000) { st.NopRemoveFromVehiclePending = false; return; }

        var pState = player.State;

        if (pState == PlayerState.Driving || pState == PlayerState.Passenger)
            _warnings.AddWarning(player.Id, "NopRemoveFromVehicle",
                $"still in veh={player.Vehicle?.Id} state={pState}");

        st.NopRemoveFromVehiclePending = false;
    }

    public void OnPlayerSpawned(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.NopRemoveFromVehiclePending = false;
    }

    public void OnPlayerDied(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.NopRemoveFromVehiclePending = false;
    }

    public void OnPlayerExitVehicle(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.NopRemoveFromVehiclePending = false;
    }

    public void OnPlayerDisconnected(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.NopRemoveFromVehiclePending = false;
    }
}