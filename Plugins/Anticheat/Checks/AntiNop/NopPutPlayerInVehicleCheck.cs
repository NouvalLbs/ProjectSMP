using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.AntiNop;

public class NopPutPlayerInVehicleCheck
{
    private const long DeadlineMs = 2650;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public NopPutPlayerInVehicleCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPutPlayerInVehicle(int playerId, int vehicleId, int seatId = 0)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        st.NopPutInVehicleExpected = vehicleId;
        st.NopPutInVehicleSeat = seatId;
        st.NopPutInVehicleDeadline = Environment.TickCount64 + DeadlineMs;
    }

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("NopPutInVehicle").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (st.NopPutInVehicleExpected == -1) return;
        if (now < st.NopPutInVehicleDeadline) return;
        if (now - st.SpawnTick < 3000) { st.NopPutInVehicleExpected = -1; return; }

        var pState = player.State;

        bool inExpectedVehicle = (pState == PlayerState.Driving || pState == PlayerState.Passenger)
                      && player.Vehicle is not null
                      && player.Vehicle.Id == st.NopPutInVehicleExpected;

        if (!inExpectedVehicle)
        {
            _warnings.AddWarning(player.Id, "NopPutInVehicle",
                $"expected=veh:{st.NopPutInVehicleExpected} state={pState} " +
                $"got=veh:{(player.Vehicle?.Id.ToString() ?? "none")}");
        }
        else if (st.NopPutInVehicleSeat >= 0)
        {
            int actualSeat = player.VehicleSeat;
            if (actualSeat != st.NopPutInVehicleSeat)
                _warnings.AddWarning(player.Id, "NopPutInVehicle",
                    $"expected=seat:{st.NopPutInVehicleSeat} got=seat:{actualSeat}");
        }

        st.NopPutInVehicleExpected = -1;
    }

    public void OnPlayerSpawned(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.NopPutInVehicleExpected = -1;
    }

    public void OnPlayerDied(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.NopPutInVehicleExpected = -1;
    }

    public void OnPlayerExitVehicle(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.NopPutInVehicleExpected = -1;
    }
}