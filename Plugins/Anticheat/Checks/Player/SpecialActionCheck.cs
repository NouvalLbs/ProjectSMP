using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Player;

public class SpecialActionCheck
{
    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public SpecialActionCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("SpecialActionHack").Enabled) return;
        if (player.State != PlayerState.OnFoot) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;
        if (now - st.SetSpecialActionTick < 2000) return;

        int sa = (int)player.SpecialAction;
        if (sa == 0) return;

        bool allowed = st.SetSpecialActionId == sa
            || sa == (int)SpecialAction.Duck
            || sa == (int)SpecialAction.HandsUp
            || sa == (int)SpecialAction.EnterVehicle
            || sa == (int)SpecialAction.ExitVehicle;

        if (!allowed)
            _warnings.AddWarning(player.Id, "SpecialActionHack", $"sa={sa}");
        else
        {
            int anim = st.Anim;

            if (sa == (int)SpecialAction.Usejetpack)
            {
                bool validJetpackAnim = (1128 <= anim && anim <= 1134) || (1538 <= anim && anim <= 1544);
                if (!validJetpackAnim && anim != 0)
                {
                    _warnings.AddWarning(player.Id, "SpecialActionHack",
                        $"jetpack invalidAnim={anim}");
                }
            }
        }
    }

    public void OnSpecialActionSet(int playerId, int action)
    {
        var st = _players.Get(playerId);
        if (st is null) return;
        st.SetSpecialActionId = action;
        st.SetSpecialActionTick = Environment.TickCount64;
    }
}