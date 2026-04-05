using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.Checks.Combat;

public class RapidFireCheck
{
    private static readonly Dictionary<int, int> _minInterval = new()
    {
        {22,450},{23,450},{24,350},
        {25,900},{26,600},{27,500},
        {33,900},{34,1400},{35,2000},{36,2000}
    };

    private readonly ConcurrentDictionary<int, (int WepId, long Tick)> _lastShot = new();
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;
    private readonly PlayerStateManager _players;

    public RapidFireCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerWeaponShot(BasePlayer player, WeaponShotEventArgs e)
    {
        if (!_config.Enabled || !_config.GetCheck("RapidFire").Enabled) return;

        int wid = (int)e.Weapon;
        if (!_minInterval.TryGetValue(wid, out int minMs)) return;

        long now = Environment.TickCount64;

        if (_lastShot.TryGetValue(player.Id, out var prev) && prev.WepId == wid)
        {
            long elapsed = now - prev.Tick;
            if (elapsed < minMs)
            {
                var st = _players.Get(player.Id);
                if (st is null || now - st.ReloadTick > 2000)
                    _warnings.AddWarning(player.Id, "RapidFire",
                        $"wid={wid} elapsed={elapsed}ms min={minMs}ms");
            }
        }

        _lastShot[player.Id] = (wid, now);
    }

    public void OnPlayerDisconnected(int playerId) => _lastShot.TryRemove(playerId, out _);
}