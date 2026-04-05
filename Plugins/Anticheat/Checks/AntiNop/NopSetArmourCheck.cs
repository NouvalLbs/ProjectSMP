using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.AntiNop;

public class NopSetArmourCheck
{
    private const long DeadlineMs = 2850;
    private const float Tolerance = 1.0f;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public NopSetArmourCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnSetPlayerArmour(int playerId, float armour)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        st.NopSetArmourExpected = armour;
        st.NopSetArmourDeadline = Environment.TickCount64 + DeadlineMs;
    }

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("NopSetArmour").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (st.NopSetArmourExpected < 0f) return;
        if (now < st.NopSetArmourDeadline) return;
        if (now - st.SpawnTick < 3000) { st.NopSetArmourExpected = -1f; return; }

        float actual = player.Armour;

        if (actual > st.NopSetArmourExpected + Tolerance)
            _warnings.AddWarning(player.Id, "NopSetArmour",
                $"expected={st.NopSetArmourExpected:F1} got={actual:F1}");

        st.NopSetArmourExpected = -1f;
    }

    public void OnPlayerSpawned(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.NopSetArmourExpected = -1f;
    }

    public void OnPlayerDied(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.NopSetArmourExpected = -1f;
    }
}