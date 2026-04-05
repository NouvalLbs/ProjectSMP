using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.AntiNop;

public class NopSetInteriorCheck
{
    private const long DeadlineMs = 2850;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public NopSetInteriorCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnSetPlayerInterior(int playerId, int interiorId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        st.NopSetInteriorExpected = interiorId & 0xFF;
        st.NopSetInteriorDeadline = Environment.TickCount64 + DeadlineMs;
    }

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("NopSetInterior").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (st.NopSetInteriorExpected == -1) return;
        if (now < st.NopSetInteriorDeadline) return;
        if (now - st.SpawnTick < 3000) { st.NopSetInteriorExpected = -1; return; }

        int actual = player.Interior;
        if (actual != st.NopSetInteriorExpected)
            _warnings.AddWarning(player.Id, "NopSetInterior",
                $"expected={st.NopSetInteriorExpected} got={actual}");

        st.NopSetInteriorExpected = -1;
    }

    public void OnPlayerSpawned(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.NopSetInteriorExpected = -1;
    }
}