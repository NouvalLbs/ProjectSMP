using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.World;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.Checks.AntiFlood;

public class SeatFloodCheck
{
    private const int WindowMs = 1000;
    private const int MaxChanges = 4;

    private readonly ConcurrentDictionary<int, Queue<long>> _windows = new();
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public SeatFloodCheck(WarningManager w, AnticheatConfig c)
        => (_warnings, _config) = (w, c);

    public bool OnPlayerEnterVehicle(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("SeatFlood").Enabled) return true;

        long now = Environment.TickCount64;
        var q = _windows.GetOrAdd(player.Id, _ => new Queue<long>());

        lock (q)
        {
            while (q.Count > 0 && now - q.Peek() > WindowMs) q.Dequeue();
            if (q.Count >= MaxChanges)
            {
                _warnings.AddWarning(player.Id, "SeatFlood", $"count={q.Count}");
                return false;
            }
            q.Enqueue(now);
        }
        return true;
    }

    public void OnPlayerDisconnected(int playerId) => _windows.TryRemove(playerId, out _);
}