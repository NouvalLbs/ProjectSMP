using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.World;
using System.Collections.Concurrent;

namespace ProjectSMP.Plugins.Anticheat.Checks.Server;

public class RconProtection
{
    private readonly ConcurrentDictionary<int, int> _attempts = new();
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;
    private readonly AcLogger _logger;

    public RconProtection(WarningManager w, AnticheatConfig c, AcLogger l)
        => (_warnings, _config, _logger) = (w, c, l);

    public void OnRconLoginAttempt(BasePlayer player, string password, bool success)
    {
        if (!_config.Enabled || !_config.GetCheck("RconHack").Enabled) return;
        if (success) return;

        int count = _attempts.AddOrUpdate(player.Id, 1, (_, v) => v + 1);
        _logger.Log($"Rcon fail: P:{player.Id} ip={player.IP} attempt={count}");

        if (count >= 1)
        {
            _warnings.AddWarning(player.Id, "RconHack", $"attempt={count}");
        }
    }

    public void OnPlayerDisconnected(int playerId) => _attempts.TryRemove(playerId, out _);
}