using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.World;
using System.Collections.Concurrent;

namespace ProjectSMP.Plugins.Anticheat.Checks.Server;

public class PingCheck
{
    private readonly ConcurrentDictionary<int, int> _warnCount = new();
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;
    private readonly AcLogger _logger;

    public PingCheck(WarningManager w, AnticheatConfig c, AcLogger l)
        => (_warnings, _config, _logger) = (w, c, l);

    public void Tick()
    {
        if (!_config.Enabled || !_config.GetCheck("HighPing").Enabled) return;

        foreach (var player in BasePlayer.All)
        {
            if (player.Ping > _config.MaxPing)
                _warnings.AddWarning(player.Id, "HighPing", $"ping={player.Ping}");
        }
    }

    public void OnPlayerDisconnected(int playerId) => _warnCount.TryRemove(playerId, out _);
}