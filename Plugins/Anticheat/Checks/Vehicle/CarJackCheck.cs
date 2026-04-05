using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Vehicle;

public class CarJackCheck
{
    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public CarJackCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public bool OnPlayerEnterVehicle(BasePlayer player, EnterVehicleEventArgs e)
    {
        if (!_config.Enabled || !_config.GetCheck("CarJack").Enabled) return true;

        var st = _players.Get(player.Id);
        if (st is null) return true;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return true;
        if (now - st.EnterVehicleTick < 500) return true;

        if (st.VehicleId != -1 && e.Vehicle.Id != st.VehicleId
            && now - st.PutInVehicleTick > 2000
            && now - st.RemoveFromVehicleTick > 1000)
        {
            _warnings.AddWarning(player.Id, "CarJack",
                $"inVeh={st.VehicleId} entering={e.Vehicle.Id}");
            return false;
        }
        return true;
    }
}