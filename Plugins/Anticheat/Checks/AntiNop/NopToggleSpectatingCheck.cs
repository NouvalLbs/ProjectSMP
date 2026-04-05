using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.AntiNop;

public class NopToggleSpectatingCheck
{
    private const long DeadlineMs = 2650;

    // Sentinel values stored in NopToggleSpectatingExpected
    private const int None = -1;
    private const int ExpectOff = 0;
    private const int ExpectOn = 1;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public NopToggleSpectatingCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnTogglePlayerSpectating(int playerId, bool toggle)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        st.NopToggleSpectatingExpected = toggle ? ExpectOn : ExpectOff;
        st.NopToggleSpectatingDeadline = Environment.TickCount64 + DeadlineMs;
    }

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("NopToggleSpectating").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (st.NopToggleSpectatingExpected == None) return;
        if (now < st.NopToggleSpectatingDeadline) return;
        if (now - st.SpawnTick < 3000) { st.NopToggleSpectatingExpected = None; return; }

        bool isSpectating = player.State == PlayerState.Spectating;
        bool expectedSpectating = st.NopToggleSpectatingExpected == ExpectOn;

        if (isSpectating != expectedSpectating)
            _warnings.AddWarning(player.Id, "NopToggleSpectating",
                $"expected={(expectedSpectating ? "on" : "off")} got={(isSpectating ? "on" : "off")}");

        st.NopToggleSpectatingExpected = None;
    }

    public void OnPlayerSpawned(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.NopToggleSpectatingExpected = None;
    }

    public void OnPlayerDied(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.NopToggleSpectatingExpected = None;
    }
}