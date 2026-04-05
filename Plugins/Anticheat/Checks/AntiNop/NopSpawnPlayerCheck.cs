using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.AntiNop;

public class NopSpawnPlayerCheck
{
    private const long DeadlineMs = 2650;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public NopSpawnPlayerCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnSpawnPlayer(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        st.NopSpawnPlayerPending = true;
        st.NopSpawnPlayerDeadline = Environment.TickCount64 + DeadlineMs;
    }

    // Called when OnPlayerSpawned fires — client acknowledged the spawn
    public void OnPlayerSpawned(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.NopSpawnPlayerPending = false;
    }

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("NopSpawnPlayer").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null || !st.NopSpawnPlayerPending) return;

        long now = Environment.TickCount64;
        if (now < st.NopSpawnPlayerDeadline) return;

        // Deadline elapsed without OnPlayerSpawned firing — client ignored the packet
        _warnings.AddWarning(player.Id, "NopSpawnPlayer", "spawn not acknowledged");
        st.NopSpawnPlayerPending = false;
    }

    public void OnPlayerDisconnected(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.NopSpawnPlayerPending = false;
    }
}