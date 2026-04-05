using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.World;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.Checks.AntiFlood;

public class DosCheck
{
    private const int PacketWindowMs = 1000;
    private const int MaxPacketsPerSec = 1000;

    private readonly ConcurrentDictionary<int, Queue<long>> _windows = new();
    private readonly AnticheatConfig _config;
    private readonly AcLogger _logger;

    public DosCheck(AnticheatConfig c, AcLogger l) => (_config, _logger) = (c, l);

    public bool OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("DoS").Enabled) return true;

        long now = Environment.TickCount64;
        var q = _windows.GetOrAdd(player.Id, _ => new Queue<long>());

        lock (q)
        {
            while (q.Count > 0 && now - q.Peek() > PacketWindowMs) q.Dequeue();
            q.Enqueue(now);
            if (q.Count <= MaxPacketsPerSec) return true;
        }

        _logger.LogKick(player.Id, $"DoS pps={MaxPacketsPerSec} ip={player.IP}");
        player.Kick();
        return false;
    }

    public void OnPlayerDisconnected(int playerId) => _windows.TryRemove(playerId, out _);
}