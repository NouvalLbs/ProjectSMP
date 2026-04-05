using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Player;

public class AfkGhostCheck
{
    private const long GhostThresholdMs = 30_000;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public AfkGhostCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void Tick()
    {
        if (!_config.Enabled || !_config.GetCheck("AfkGhost").Enabled) return;

        long now = Environment.TickCount64;

        foreach (var player in BasePlayer.All)
        {
            var st = _players.Get(player.Id);
            if (st is null || st.IsDead || st.IsSpectating || !st.IsOnline) continue;
            if (st.UpdateTick == 0) continue;
            if (now - st.SpawnTick < 5000) continue;

            if (now - st.UpdateTick > GhostThresholdMs)
                _warnings.AddWarning(player.Id, "AfkGhost",
                    $"no update for {now - st.UpdateTick}ms");
        }
    }

    public void OnPlayerSpawned(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.UpdateTick = Environment.TickCount64;
    }

    public void OnPlayerDied(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.UpdateTick = Environment.TickCount64;
    }
}