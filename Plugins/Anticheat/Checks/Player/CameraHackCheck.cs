using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Player;

public class CameraHackCheck
{
    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public CameraHackCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("CameraHack").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;

        int camMode = st.CamMode;
        int weapon = (int)player.Weapon;

        if (camMode < 0 || camMode > 55)
        {
            _warnings.AddWarning(player.Id, "CameraHack", $"invalid cam={camMode}");
            return;
        }

        if (player.State == PlayerState.OnFoot)
        {
            bool isSniperWeapon = weapon is 34 or 35 or 36;
            bool isAimingMode = camMode is 7 or 8 or 53 or 55;

            player.GetKeys(out Keys keys, out _, out _);
            bool isAiming = (keys & Keys.Aim) != 0 || (keys & Keys.SecondaryAttack) != 0;

            if (isSniperWeapon && isAiming && !isAimingMode && camMode != 4)
            {
                _warnings.AddWarning(player.Id, "CameraHack",
                    $"sniper wid={weapon} cam={camMode} notAiming");
            }

            if (!isSniperWeapon && weapon > 0 && isAiming && (camMode == 7 || camMode == 8))
            {
                _warnings.AddWarning(player.Id, "CameraHack",
                    $"nonSniper wid={weapon} cam={camMode} fakeSniper");
            }
        }
        else if (player.State == PlayerState.Driving)
        {
            if (camMode == 7 || camMode == 8 || camMode == 53 || camMode == 55)
            {
                _warnings.AddWarning(player.Id, "CameraHack",
                    $"inVehicle cam={camMode} invalidMode");
            }
        }

        int specAct = (int)player.SpecialAction;
        if (specAct == 2 || (24 <= specAct && specAct <= 25))
        {
            if (camMode != 3 && camMode != 55 && camMode != 53)
            {
                _warnings.AddWarning(player.Id, "CameraHack",
                    $"jetpack specAct={specAct} cam={camMode}");
            }
        }
    }
}