using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Combat;

public class FullAimingCheck
{
    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public FullAimingCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("FullAiming").Enabled) return;
        if (player.State != PlayerState.Driving) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;
        if (now - st.EnterVehicleTick < 2000) return;

        player.GetKeys(out Keys keys, out _, out _);
        bool aiming = (keys & Keys.SecondaryAttack) != 0;
        if (!aiming) return;

        int wid = (int)player.Weapon;
        int model = player.Vehicle is not null ? (int)player.Vehicle.Model : -1;
        if (wid is 0 or 1 or 2) return;

        _warnings.AddWarning(player.Id, "FullAiming", $"wid={wid} model={model}");
    }
}