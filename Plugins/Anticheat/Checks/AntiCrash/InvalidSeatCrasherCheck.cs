using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Data;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;

namespace ProjectSMP.Plugins.Anticheat.Checks.AntiCrash;

public class InvalidSeatCrasherCheck
{
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public InvalidSeatCrasherCheck(WarningManager w, AnticheatConfig c)
        => (_warnings, _config) = (w, c);

    public bool OnPlayerEnterVehicle(BasePlayer player, EnterVehicleEventArgs e)
    {
        if (!_config.Enabled || !_config.GetCheck("InvalidSeatCrasher").Enabled) return true;

        int model = (int)e.Vehicle.Model;
        int maxPassengers = VehicleData.GetMaxPassengers(model);

        if (e.IsPassenger && maxPassengers == 0)
        {
            _warnings.AddWarning(player.Id, "InvalidSeatCrasher",
                $"passenger on model={model} maxPassengers=0");
            return false;
        }

        return true;
    }
}