using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Data;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;

namespace ProjectSMP.Plugins.Anticheat.Checks.AntiCrash;

public class TuningCrasherCheck
{
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public TuningCrasherCheck(WarningManager w, AnticheatConfig c)
        => (_warnings, _config) = (w, c);

    public bool OnVehicleMod(BaseVehicle vehicle, BasePlayer player, int componentId)
    {
        if (!_config.Enabled || !_config.GetCheck("TuningCrasher").Enabled) return true;

        int model = (int)vehicle.Model;
        if (!TuningData.IsValidComponent(model, componentId))
        {
            _warnings.AddWarning(player.Id, "TuningCrasher",
                $"comp={componentId} model={model}");
            return false;
        }
        return true;
    }
}