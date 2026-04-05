using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.AntiNop;

public class NopSetSpecialActionCheck
{
    private const long DeadlineMs = 3250;

    // Transitional actions the client resolves on its own — mismatch is expected
    private static readonly System.Collections.Generic.HashSet<int> _transitional = new()
    {
        (int)SpecialAction.EnterVehicle,
        (int)SpecialAction.ExitVehicle,
        (int)SpecialAction.Usejetpack,
        24, // skydive entry
        25, // skydive freefall
    };

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public NopSetSpecialActionCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnSetPlayerSpecialAction(int playerId, int actionId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;
        if (_transitional.Contains(actionId)) return;

        st.NopSetSpecialActionExpected = actionId;
        st.NopSetSpecialActionDeadline = Environment.TickCount64 + DeadlineMs;
    }

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("NopSetSpecialAction").Enabled) return;
        if (player.State != PlayerState.OnFoot) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (st.NopSetSpecialActionExpected == -1) return;
        if (now < st.NopSetSpecialActionDeadline) return;
        if (now - st.SpawnTick < 3000) { st.NopSetSpecialActionExpected = -1; return; }
        if (now - st.EnterVehicleTick < 2000) { st.NopSetSpecialActionExpected = -1; return; }
        if (now - st.RemoveFromVehicleTick < 2000) { st.NopSetSpecialActionExpected = -1; return; }

        int actual = (int)player.SpecialAction;

        if (actual != st.NopSetSpecialActionExpected)
            _warnings.AddWarning(player.Id, "NopSetSpecialAction",
                $"expected={st.NopSetSpecialActionExpected} got={actual}");

        st.NopSetSpecialActionExpected = -1;
    }

    public void OnPlayerSpawned(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.NopSetSpecialActionExpected = -1;
    }

    public void OnPlayerDied(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.NopSetSpecialActionExpected = -1;
    }
}