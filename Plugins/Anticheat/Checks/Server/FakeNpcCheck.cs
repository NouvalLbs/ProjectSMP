using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.World;

namespace ProjectSMP.Plugins.Anticheat.Checks.Server;

public class FakeNpcCheck
{
    private readonly AnticheatConfig _config;
    private readonly AcLogger _logger;

    public FakeNpcCheck(AnticheatConfig c, AcLogger l)
        => (_config, _logger) = (c, l);

    public bool OnPlayerConnected(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("FakeNpc").Enabled) return true;

        if (player.IsNPC)
        {
            _logger.Log($"FakeNpc block: P:{player.Id} name={player.Name}");
            player.Kick();
            return false;
        }
        return true;
    }
}