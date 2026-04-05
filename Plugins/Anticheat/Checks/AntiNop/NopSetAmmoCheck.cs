using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Data;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.AntiNop;

public class NopSetAmmoCheck
{
    private const long DeadlineMs = 2850;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public NopSetAmmoCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnSetPlayerAmmo(int playerId, int weaponId, int ammo)
    {
        if (!WeaponData.IsValid(weaponId)) return;
        var st = _players.Get(playerId);
        if (st is null) return;

        int slot = WeaponData.Slot[weaponId];
        if (!WeaponData.SlotHasAmmo(slot)) return;

        st.NopSetAmmoWeapon[slot] = weaponId;
        st.NopSetAmmoExpected[slot] = ammo;
        st.NopSetAmmoDeadline[slot] = Environment.TickCount64 + DeadlineMs;
    }

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("NopSetAmmo").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;

        for (int slot = 0; slot < 13; slot++)
        {
            int expectedWeapon = st.NopSetAmmoWeapon[slot];
            if (expectedWeapon == -1) continue;
            if (now < st.NopSetAmmoDeadline[slot]) continue;

            player.GetWeaponData(slot, out Weapon actual, out int actualAmmo);

            if ((int)actual == expectedWeapon && actualAmmo != st.NopSetAmmoExpected[slot])
                _warnings.AddWarning(player.Id, "NopSetAmmo",
                    $"slot={slot} wid={expectedWeapon} expected={st.NopSetAmmoExpected[slot]} got={actualAmmo}");

            st.NopSetAmmoWeapon[slot] = -1;
        }
    }

    public void OnWeaponsReset(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;
        for (int i = 0; i < 13; i++) st.NopSetAmmoWeapon[i] = -1;
    }

    public void OnPlayerSpawned(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;
        for (int i = 0; i < 13; i++) st.NopSetAmmoWeapon[i] = -1;
    }
}