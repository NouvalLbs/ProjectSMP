using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.World;
using System;
using System.Collections.Concurrent;

namespace ProjectSMP.Plugins.Anticheat.Checks.Server;

public class ReconnectCheck
{
    private readonly ConcurrentDictionary<string, long> _lastDisconnect = new();
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;
    private readonly AcLogger _logger;

    public ReconnectCheck(WarningManager w, AnticheatConfig c, AcLogger l)
        => (_warnings, _config, _logger) = (w, c, l);

    public void OnPlayerConnected(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("Reconnect").Enabled) return;

        string ip = player.IP;
        long now = Environment.TickCount64;
        long minMs = _config.MinReconnectSeconds * 1000L;

        if (_lastDisconnect.TryGetValue(ip, out long lastTick) && lastTick > 0)
        {
            long elapsed = now - lastTick;
            if (elapsed < minMs)
            {
                _logger.LogKick(player.Id, $"Reconnect ip={ip} elapsed={elapsed}ms min={minMs}ms");
                player.Kick();
            }
        }
    }

    public void OnPlayerDisconnected(BasePlayer player)
        => _lastDisconnect[player.IP] = Environment.TickCount64;
}