using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.World;

namespace ProjectSMP.Plugins.Anticheat.Checks.AntiFlood;

public class CallbackFloodCheck
{
    private readonly FloodRateLimiter _flood;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public CallbackFloodCheck(FloodRateLimiter f, WarningManager w, AnticheatConfig c)
        => (_flood, _warnings, _config) = (f, w, c);

    public bool Check(BasePlayer player, int callbackId)
    {
        if (!_config.Enabled || !_config.GetCheck("CallbackFlood").Enabled) return true;

        if (!_flood.Check(player.Id, callbackId))
        {
            _warnings.AddWarning(player.Id, "CallbackFlood", $"cb={callbackId}");
            return false;
        }
        return true;
    }
}