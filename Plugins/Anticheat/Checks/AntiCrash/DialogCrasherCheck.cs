using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;

namespace ProjectSMP.Plugins.Anticheat.Checks.AntiCrash;

public class DialogCrasherCheck
{
    private const int MaxDialogId = 32767;

    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public DialogCrasherCheck(WarningManager w, AnticheatConfig c)
        => (_warnings, _config) = (w, c);

    public bool OnDialogResponse(BasePlayer player, DialogResponseEventArgs e)
    {
        if (!_config.Enabled || !_config.GetCheck("DialogCrasher").Enabled) return true;
        if (e.DialogId < 0 || e.DialogId > MaxDialogId) {
            _warnings.AddWarning(player.Id, "DialogCrasher",
                $"dialogId={e.DialogId}");
            return false;
        }

        return true;
    }
}