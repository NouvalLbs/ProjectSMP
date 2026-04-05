using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.World;

namespace ProjectSMP.Plugins.Anticheat.Checks.AntiCrash;

public class AttachedObjectCrasherCheck
{
    private const int MaxSlot = 9;
    private const int MaxModel = 19999;
    private const int MinModel = 1;

    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public AttachedObjectCrasherCheck(WarningManager w, AnticheatConfig c)
        => (_warnings, _config) = (w, c);

    public bool ValidateAttachedObject(BasePlayer player, int slot, int modelId)
    {
        if (!_config.Enabled || !_config.GetCheck("AttachedObjectCrasher").Enabled) return true;

        if (slot < 0 || slot > MaxSlot)
        {
            _warnings.AddWarning(player.Id, "AttachedObjectCrasher",
                $"slot={slot}");
            return false;
        }

        if (modelId < MinModel || modelId > MaxModel)
        {
            _warnings.AddWarning(player.Id, "AttachedObjectCrasher",
                $"modelId={modelId}");
            return false;
        }

        return true;
    }
}