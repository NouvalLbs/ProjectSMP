using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Combat;

public class QuickTurnCheck
{
    private const float MaxAngleDeltaPerUpdate = 170f;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public QuickTurnCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("QuickTurn").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;
        if (now - st.SetPosTick < 2000) return;

        if (player.State != SampSharp.GameMode.Definitions.PlayerState.OnFoot) return;

        float curFacing = player.Angle;
        float lastFacing = st.LastFacingAngle;
        float delta = AngleHelper.Diff(curFacing, lastFacing);

        if (lastFacing >= 0 && delta > MaxAngleDeltaPerUpdate)
            _warnings.AddWarning(player.Id, "QuickTurn",
                $"delta={delta:F1} cur={curFacing:F1}");

        st.LastFacingAngle = curFacing;
    }
}