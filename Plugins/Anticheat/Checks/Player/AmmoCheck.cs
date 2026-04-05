using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Data;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;
using System.Numerics;

namespace ProjectSMP.Plugins.Anticheat.Checks.Player;

public class AmmoCheck
{
    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public AmmoCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        var st = _players.Get(player.Id);
        if (st is null || st.IsDead || !_config.Enabled) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;
        if (now - st.ResetWeaponsTick < 1500) return;

        for (int slot = 1; slot < 13; slot++)
        {
            if (!WeaponData.SlotHasAmmo(slot)) continue;

            player.GetWeaponData(slot, out Weapon _, out int ammo);
            int stored = st.Ammo[slot];

            if (_config.GetCheck("AmmoHackInfinite").Enabled)
            {
                long shotTick = st.ShotAmmoTick[slot];
                if (shotTick > 0 && now - shotTick >= 100 && now - shotTick < 800) {
                    bool serverGave = now - st.GiveAmmoTick[slot] < 1500;
                    if (!serverGave && ammo >= st.PreShotAmmo[slot] && st.PreShotAmmo[slot] > 0)
                        _warnings.AddWarning(player.Id, "AmmoHackInfinite", $"slot={slot} ammo={ammo}");
                    st.ShotAmmoTick[slot] = 0;
                }
            }

            if (ammo == stored) continue;

            int gain = ammo - stored;
            bool serverGaveAmmo = now - st.GiveAmmoTick[slot] < 1500;

            if (gain > 0 && !serverGaveAmmo && _config.GetCheck("AmmoHackAdd").Enabled)
            {
                var pos = player.Position;
                if (!WeaponData.IsNearAmmuNation(pos.X, pos.Y, pos.Z))
                    _warnings.AddWarning(player.Id, "AmmoHackAdd", $"slot={slot} +{gain} total={ammo}");
            }

            st.Ammo[slot] = ammo;
        }
    }

    public void OnPlayerWeaponShot(BasePlayer player, int weaponId)
    {
        var st = _players.Get(player.Id);
        if (st is null || !WeaponData.IsValid(weaponId)) return;
        int slot = WeaponData.Slot[weaponId];
        if (!WeaponData.SlotHasAmmo(slot)) return;
        player.GetWeaponData(slot, out _, out int ammo);
        st.PreShotAmmo[slot] = ammo;
        st.ShotAmmoTick[slot] = Environment.TickCount64;
        if (ammo <= 1) st.ReloadTick = Environment.TickCount64;
    }

    public void OnAmmoGiven(int playerId, int weaponId, int ammo)
    {
        var st = _players.Get(playerId);
        if (st is null || !WeaponData.IsValid(weaponId)) return;
        int slot = WeaponData.Slot[weaponId];
        st.GiveAmmoTick[slot] = Environment.TickCount64;

        if (WeaponData.SlotSharesAmmo(slot)) {
            st.Ammo[slot] += ammo;
        } else {
            var player = BasePlayer.Find(playerId);
            if (player is null) return;

            player.GetWeaponData(slot, out _, out int currentAmmo);
            int currentWeapon = st.Ammo[slot] > 0 ? st.Weapons[slot] : 0;

            if (currentWeapon == weaponId || currentWeapon == 0)
            {
                st.Ammo[slot] += ammo;
            }
            else
            {
                st.Ammo[slot] = ammo;
            }
        }
    }
}