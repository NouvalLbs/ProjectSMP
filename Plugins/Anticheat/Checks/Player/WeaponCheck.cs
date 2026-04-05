using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Data;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Player;

public class WeaponCheck
{
    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public WeaponCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        var st = _players.Get(player.Id);
        if (st is null || st.IsDead || !_config.Enabled) return;
        if (!_config.GetCheck("WeaponHack").Enabled) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;
        if (now - st.ResetWeaponsTick < 1500) return;

        for (int slot = 0; slot < 13; slot++)
        {
            player.GetWeaponData(slot, out Weapon weapon, out _);
            int wid = (int)weapon;
            if (wid == 0) continue;

            if (!WeaponData.IsValid(wid))
            {
                _warnings.AddWarning(player.Id, "WeaponHack", $"slot={slot} wid={wid} (invalid id)");
                continue;
            }

            int trackedWid = st.Weapons[slot];
            if (trackedWid == wid) continue;

            if (now - st.SetWeaponTick[slot] < 1500) continue;

            var pos = player.Position;
            if (WeaponData.IsNearAmmuNation(pos.X, pos.Y, pos.Z))
            {
                st.Weapons[slot] = wid;
                st.SetWeaponTick[slot] = now;
                continue;
            }

            _warnings.AddWarning(player.Id, "WeaponHack", $"slot={slot} got={wid} expected={trackedWid}");
        }
    }

    public void OnWeaponGiven(int playerId, int weaponId, int ammo)
    {
        var st = _players.Get(playerId);
        if (st is null || !WeaponData.IsValid(weaponId)) return;
        int slot = WeaponData.Slot[weaponId];
        st.Weapons[slot] = weaponId;
        st.Ammo[slot] = ammo;
        st.SetWeaponTick[slot] = Environment.TickCount64;
    }

    public void OnWeaponsReset(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;
        Array.Clear(st.Weapons, 0, 13);
        Array.Clear(st.Ammo, 0, 13);
        st.ResetWeaponsTick = Environment.TickCount64;
    }

    public void OnPlayerSpawned(BasePlayer player) {
        var st = _players.Get(player.Id);
        if (st is null) return;

        for (int i = 0; i < 13; i++) {
            st.Weapons[i] = 0;
            st.Ammo[i] = 0;
        }

        OnWeaponGiven(player.Id, st.SpawnWeapon1, st.SpawnAmmo1);
        OnWeaponGiven(player.Id, st.SpawnWeapon2, st.SpawnAmmo2);
        OnWeaponGiven(player.Id, st.SpawnWeapon3, st.SpawnAmmo3);
    }
}