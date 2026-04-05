using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Data;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.AntiNop;

public class NopGiveWeaponCheck
{
    private const long DeadlineMs = 2850;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public NopGiveWeaponCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnWeaponGiven(int playerId, int weaponId, int ammo)
    {
        if (!WeaponData.IsValid(weaponId)) return;
        var st = _players.Get(playerId);
        if (st is null) return;

        int slot = WeaponData.Slot[weaponId];
        st.NopSetWeapon[slot] = weaponId;
        st.NopSetWeaponDeadline[slot] = Environment.TickCount64 + DeadlineMs;
    }

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("NopGiveWeapon").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;

        for (int slot = 0; slot < 13; slot++)
        {
            int expected = st.NopSetWeapon[slot];
            if (expected == -1) continue;
            if (now < st.NopSetWeaponDeadline[slot]) continue;

            player.GetWeaponData(slot, out Weapon actual, out _);
            int actualId = (int)actual;

            if (actualId != expected)
                _warnings.AddWarning(player.Id, "NopGiveWeapon",
                    $"slot={slot} expected={expected} got={actualId}");

            st.NopSetWeapon[slot] = -1;
        }
    }

    public void OnWeaponsReset(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;
        for (int i = 0; i < 13; i++) st.NopSetWeapon[i] = -1;
    }

    public void OnPlayerSpawned(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is null) return;
        for (int i = 0; i < 13; i++) st.NopSetWeapon[i] = -1;
    }
}