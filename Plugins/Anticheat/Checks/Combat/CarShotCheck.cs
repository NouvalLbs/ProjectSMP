using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Data;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.Checks.Combat;

public class CarShotCheck
{
    private static readonly HashSet<int> _driveByWeapons = new() { 28, 29, 32, 22, 24 };

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public CarShotCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerWeaponShot(BasePlayer player, WeaponShotEventArgs e)
    {
        if (!_config.Enabled || !_config.GetCheck("CarShot").Enabled) return;
        if (player.State != PlayerState.Driving) return;

        int wid = (int)e.Weapon;
        int model = player.Vehicle is not null ? (int)player.Vehicle.Model : -1;
        if (model < 400) return;

        if (_driveByWeapons.Contains(wid)) return;
        if (VehicleData.IsHelicopter(model) || VehicleData.IsAircraft(model)) return;

        var st = _players.Get(player.Id);
        if (st is null) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;

        _warnings.AddWarning(player.Id, "CarShot", $"wid={wid} model={model}");
    }
}