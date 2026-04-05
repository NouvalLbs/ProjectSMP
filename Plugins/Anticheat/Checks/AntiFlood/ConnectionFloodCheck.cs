using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.World;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.Checks.AntiFlood;

public class ConnectionFloodCheck
{
    private const int WindowMs = 10_000;
    private const int MaxConnects = 5;

    private readonly ConcurrentDictionary<string, Queue<long>> _ipWindows = new();
    private readonly AnticheatConfig _config;
    private readonly AcLogger _logger;

    public ConnectionFloodCheck(AnticheatConfig c, AcLogger l) => (_config, _logger) = (c, l);

    public bool OnPlayerConnected(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("ConnectionFlood").Enabled) return true;

        string ip = player.IP;
        long now = Environment.TickCount64;
        var q = _ipWindows.GetOrAdd(ip, _ => new Queue<long>());

        lock (q)
        {
            while (q.Count > 0 && now - q.Peek() > WindowMs) q.Dequeue();
            if (q.Count >= MaxConnects)
            {
                _logger.LogKick(player.Id, $"ConnectionFlood ip={ip} count={q.Count}");
                player.Kick();
                return false;
            }
            q.Enqueue(now);
        }
        return true;
    }

    public void OnPlayerDisconnected(string ip)
    {
        if (_ipWindows.TryGetValue(ip, out var q))
            lock (q) { if (q.Count == 0) _ipWindows.TryRemove(ip, out _); }
    }
}