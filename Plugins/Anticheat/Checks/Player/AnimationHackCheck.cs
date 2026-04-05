using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Player;

public class AnimationHackCheck
{
    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public AnimationHackCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("AnimationHack").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;
        if (player.State != PlayerState.OnFoot) return;

        int anim = st.Anim;
        int specAct = (int)player.SpecialAction;

        if (specAct == (int)SpecialAction.Usejetpack)
        {
            bool validJetpackAnim = (1128 <= anim && anim <= 1134) || (1538 <= anim && anim <= 1544);
            if (!validJetpackAnim && anim != 0)
            {
                _warnings.AddWarning(player.Id, "AnimationHack",
                    $"jetpack anim={anim} expected=1128-1134|1538-1544");
            }
        }

        if (24 <= specAct && specAct <= 25)
        {
            bool validParachuteAnim = (958 <= anim && anim <= 979);
            if (!validParachuteAnim && anim != 0)
            {
                _warnings.AddWarning(player.Id, "AnimationHack",
                    $"parachute specAct={specAct} anim={anim} expected=958-979");
            }
        }

        if (specAct == (int)SpecialAction.Duck)
        {
            bool validDuckAnim = anim == 1130 || anim == 1131;
            if (!validDuckAnim && anim != 0 && anim != 1195)
            {
                _warnings.AddWarning(player.Id, "AnimationHack",
                    $"duck anim={anim} expected=1130|1131");
            }
        }

        /*
            if (specAct == (int)SpecialAction.EnterVehicle || specAct == (int)SpecialAction.ExitVehicle)
            {
                bool validVehAnim = (1543 <= anim && anim <= 1544) || anim == 0;
                if (!validVehAnim)
                {
                    _warnings.AddWarning(player.Id, "AnimationHack",
                        $"vehAction specAct={specAct} anim={anim}");
                }
            }
        */

        if (specAct == (int)SpecialAction.HandsUp)
        {
            bool validHandsupAnim = anim == 1195 || anim == 0;
            if (!validHandsupAnim)
            {
                _warnings.AddWarning(player.Id, "AnimationHack",
                    $"handsup anim={anim} expected=1195");
            }
        }

        if ((int)SpecialAction.Dance1 <= specAct && specAct <= (int)SpecialAction.Dance4)
        {
            bool validDanceAnim = (1013 <= anim && anim <= 1018) || anim == 0;
            if (!validDanceAnim)
            {
                _warnings.AddWarning(player.Id, "AnimationHack",
                    $"dance specAct={specAct} anim={anim} expected=1013-1018");
            }
        }

        if ((int)SpecialAction.DrinkBeer <= specAct && specAct <= (int)SpecialAction.DrinkSprunk)
        {
            bool validDrinkAnim = anim == 1017 || anim == 1018 || anim == 1019 || anim == 0;
            if (!validDrinkAnim)
            {
                _warnings.AddWarning(player.Id, "AnimationHack",
                    $"drink specAct={specAct} anim={anim}");
            }
        }

        if (specAct == (int)SpecialAction.UseCellphone)
        {
            bool validPhoneAnim = (1539 <= anim && anim <= 1542) || anim == 0;
            if (!validPhoneAnim)
            {
                _warnings.AddWarning(player.Id, "AnimationHack",
                    $"cellphone anim={anim} expected=1539-1542");
            }
        }
    }
}