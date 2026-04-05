using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.World;
using System.Collections.Concurrent;

namespace ProjectSMP.Plugins.Anticheat.Checks.Server;

public class SandboxProtection
{
    private readonly ConcurrentDictionary<string, int> _ipConnects = new();
    private readonly AnticheatConfig _config;
    private readonly AcLogger _logger;

    public SandboxProtection(AnticheatConfig c, AcLogger l)
        => (_config, _logger) = (c, l);

    public void OnPlayerConnected(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("Sandbox").Enabled) return;

        string ip = player.IP;
        int count = _ipConnects.AddOrUpdate(ip, 1, (_, v) => v + 1);

        if (count > _config.MaxConnectsPerIp)
        {
            _logger.Log($"Sandbox block: {ip} count={count}");
            player.Kick();
        }
    }

    public void OnPlayerDisconnected(BasePlayer player)
    {
        string ip = player.IP;
        if (_ipConnects.TryGetValue(ip, out int c) && c > 1)
            _ipConnects[ip] = c - 1;
        else
            _ipConnects.TryRemove(ip, out _);
    }
}