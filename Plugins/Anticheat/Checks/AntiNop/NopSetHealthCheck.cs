using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.AntiNop;

public class NopSetHealthCheck
{
    private const long DeadlineMs = 2850;
    private const float Tolerance = 1.0f;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public NopSetHealthCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnSetPlayerHealth(int playerId, float health)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        st.NopSetHealthExpected = health;
        st.NopSetHealthDeadline = Environment.TickCount64 + DeadlineMs;
    }

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("NopSetHealth").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (st.NopSetHealthExpected < 0f) return;
        if (now < st.NopSetHealthDeadline) return;
        if (now - st.SpawnTick < 3000) { st.NopSetHealthExpected = -1f; return; }

        float actual = player.Health;

        // Player may have taken damage after the set, so actual being lower is legitimate.
        // Flag only if actual is higher than expected — the client ignored the reduction.
        if (actual > st.NopSetHealthExpected + Tolerance)
            _warnings.AddWarning(player.Id, "NopSetHealth",
                $"expected={st.NopSetHealthExpected:F1} got={actual:F1}");

        st.NopSetHealthExpected = -1f;
    }

    public void OnPlayerSpawned(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.NopSetHealthExpected = -1f;
    }

    public void OnPlayerDied(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.NopSetHealthExpected = -1f;
    }
}