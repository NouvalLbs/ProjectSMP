using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.World;

namespace ProjectSMP.Plugins.Anticheat.Checks.Server;

public class VersionCheck
{
    private const string ValidVersion = "0.3.7";

    private readonly AnticheatConfig _config;
    private readonly AcLogger _logger;

    public VersionCheck(AnticheatConfig c, AcLogger l)
        => (_config, _logger) = (c, l);

    public void OnPlayerConnected(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("InvalidVersion").Enabled) return;

        string ver = player.Version;
        if (!ver.StartsWith(ValidVersion))
        {
            _logger.Log($"Invalid version: P:{player.Id} ver={ver}");
            player.Kick();
        }
    }
}