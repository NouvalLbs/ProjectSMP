#nullable enable
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using System;

namespace ProjectSMP.Plugins.WeaponConfig
{
    public static class WeaponConfigExtensions
    {
        public static float GetWcHealth(this Player p)
            => WeaponConfigWrappers.GetPlayerHealth(p);

        public static float GetWcArmour(this Player p)
            => WeaponConfigWrappers.GetPlayerArmour(p);

        public static void SetWcHealth(this Player p, float health, float armour = -1f)
            => WeaponConfigWrappers.SetPlayerHealth(p, health, armour);

        public static void SetWcArmour(this Player p, float armour)
            => WeaponConfigWrappers.SetPlayerArmour(p, armour);

        public static void WcDamage(this Player p, float amount, Player? issuer = null,
            int weapon = 55, int bodypart = 0, bool ignoreArmour = false)
            => WeaponConfigWrappers.DamagePlayer(p, amount, issuer, weapon, bodypart, ignoreArmour);

        public static void WcHeal(this Player p, float amount)
            => WeaponConfigWrappers.HealPlayer(p, amount);

        public static void WcResync(this Player p)
            => WeaponConfigWrappers.ResyncPlayer(p);

        public static bool WcIsSpawned(this Player p)
            => WeaponConfigWrappers.IsPlayerSpawned(p);

        public static bool WcIsDying(this Player p)
            => WeaponConfigService.IsPlayerDying(p);

        public static bool WcIsPaused(this Player p)
            => WeaponConfigWrappers.IsPlayerPaused(p);

        public static PlayerState GetWcState(this Player p)
            => WeaponConfigWrappers.GetPlayerState(p);

        public static void SetWcMaxHealth(this Player p, float max)
            => WeaponConfigWrappers.SetPlayerMaxHealth(p, max);

        public static void SetWcMaxArmour(this Player p, float max)
            => WeaponConfigWrappers.SetPlayerMaxArmour(p, max);

        public static float GetWcMaxHealth(this Player p)
            => WeaponConfigWrappers.GetPlayerMaxHealth(p);

        public static float GetWcMaxArmour(this Player p)
            => WeaponConfigWrappers.GetPlayerMaxArmour(p);

        public static void WcApplyAnimation(this Player p, string animLib, string animName,
            float delta = 4.0f, bool loop = false, bool lockX = false, bool lockY = false,
            bool freeze = false, int time = 0, bool forceSync = false)
            => WeaponConfigWrappers.ApplyAnimation(p, animLib, animName, delta, loop, lockX, lockY, freeze, time, forceSync);

        public static void WcClearAnimations(this Player p, bool forceSync = true)
            => WeaponConfigWrappers.ClearAnimations(p, forceSync);

        public static void WcSetPos(this Player p, Vector3 pos)
            => WeaponConfigWrappers.SetPlayerPos(p, pos);

        public static void WcSetVelocity(this Player p, Vector3 velocity)
            => WeaponConfigWrappers.SetPlayerVelocity(p, velocity);

        public static void WcToggleControllable(this Player p, bool toggle)
            => WeaponConfigWrappers.TogglePlayerControllable(p, toggle);

        public static void WcSpawn(this Player p)
            => WeaponConfigWrappers.SpawnPlayer(p);

        public static void WcToggleSpectating(this Player p, bool toggle)
            => WeaponConfigWrappers.TogglePlayerSpectating(p, toggle);

        public static int GetWcVirtualWorld(this Player p)
            => WeaponConfigWrappers.GetPlayerVirtualWorld(p);

        public static void SetWcVirtualWorld(this Player p, int worldid)
            => WeaponConfigWrappers.SetPlayerVirtualWorld(p, worldid);

        public static void WcForceClassSelection(this Player p)
            => WeaponConfigWrappers.ForceClassSelection(p);

        public static bool WcIsInClassSelection(this Player p)
            => WeaponConfigWrappers.IsInClassSelection(p);

        public static string GetWcRejectedHit(this Player p, int index)
            => WeaponConfigWrappers.GetRejectedHit(p, index);

        public static string GetWcPreviousHit(this Player p, int index)
            => WeaponConfigWrappers.GetPreviousHit(p, index);

        public static void WcSetPosFindZ(this Player p, float x, float y, float z)
            => WeaponConfigWrappers.SetPlayerPosFindZ(p, x, y, z);

        public static string GetWcWeaponName(this int weaponId)
            => WeaponConfigWrappers.GetWeaponName(weaponId);

        public static void WcSetDisableSyncBugs(this bool toggle)
            => WeaponConfigWrappers.SetDisableSyncBugs(toggle);

        public static void WcSetKnifeSync(this bool toggle)
            => WeaponConfigWrappers.SetKnifeSync(toggle);

        public static void WcSetSpawnInfo(this Player p, int team, int skin,
            float x, float y, float z, float rotation,
            Weapon weapon1 = Weapon.None, int ammo1 = 0,
            Weapon weapon2 = Weapon.None, int ammo2 = 0,
            Weapon weapon3 = Weapon.None, int ammo3 = 0)
            => WeaponConfigWrappers.SetSpawnInfo(p, team, skin, x, y, z, rotation, weapon1, ammo1, weapon2, ammo2, weapon3, ammo3);

        public static void WcSpectatePlayer(this Player p, Player target,
            SpectateMode mode = SampSharp.GameMode.Definitions.SpectateMode.Normal)
            => WeaponConfigWrappers.PlayerSpectatePlayer(p, target, mode);

        public static void WcStopSpectating(this Player p)
            => WeaponConfigWrappers.StopSpectating(p);

        public static bool WcIsInCheckpoint(this Player p)
            => WeaponConfigWrappers.IsPlayerInCheckpoint(p);

        public static bool WcIsInRaceCheckpoint(this Player p)
            => WeaponConfigWrappers.IsPlayerInRaceCheckpoint(p);

        public static void WcSetSpecialAction(this Player p, SampSharp.GameMode.Definitions.SpecialAction action)
            => WeaponConfigWrappers.SetPlayerSpecialAction(p, action);

        public static int WcGetTeam(this Player p)
            => WeaponConfigWrappers.GetPlayerTeam(p);

        public static void WcSendDeathMessage(this Player victim, Player? killer, int weaponid)
            => WeaponConfigWrappers.SendDeathMessage(killer, victim, weaponid);

        public static int WcAverageShootRate(this Player p, int shots)
            => WeaponConfigWrappers.AverageShootRate(p, shots);

        public static int WcAverageShootRate(this Player p, int shots, out bool multipleWeapons)
            => WeaponConfigWrappers.AverageShootRate(p, shots, out multipleWeapons);

        public static int WcAverageHitRate(this Player p, int hits)
            => WeaponConfigWrappers.AverageHitRate(p, hits);

        public static int WcAverageHitRate(this Player p, int hits, out bool multipleWeapons)
            => WeaponConfigWrappers.AverageHitRate(p, hits, out multipleWeapons);

        public static float WcGetLastDamageHealth(this Player p)
            => WeaponConfigWrappers.GetLastDamageHealth(p);

        public static float WcGetLastDamageArmour(this Player p)
            => WeaponConfigWrappers.GetLastDamageArmour(p);
        public static void WcEnableHealthBar(this Player p, bool enable)
            => WeaponConfigWrappers.EnableHealthBarForPlayer(p, enable);

        public static void WcSetDamageFeed(this Player p, bool enable)
            => WeaponConfigWrappers.SetDamageFeedForPlayer(p, enable);

        public static string WcGetBodypartName(this int bodypart)
            => WeaponConfigWrappers.GetBodypartName(bodypart);

        public static string WcGetWeaponNameStatic(this int weaponId)
            => WeaponConfigWrappers.GetWeaponName(weaponId);

        public static bool WcIsBullet(this int weaponId)
            => WeaponConfigWrappers.IsBulletWeapon(weaponId);

        public static bool WcIsMelee(this int weaponId)
            => WeaponConfigWrappers.IsMeleeWeapon(weaponId);

        public static bool WcIsShotgun(this int weaponId)
            => WeaponConfigWrappers.IsShotgunWeapon(weaponId);

        public static bool WcIsHighRate(this int weaponId)
            => WeaponConfigWrappers.IsHighRateWeapon(weaponId);

        public static WeaponEntry? WcGetEntry(this int weaponId)
            => WeaponConfigWrappers.GetWeaponEntry(weaponId);

        public static void WcModifyEntry(this int weaponId, Action<WeaponEntry> modifier)
            => WeaponConfigWrappers.ModifyWeaponEntry(weaponId, modifier);

        public static string WcGetRejectedHitRaw(this Player p, int index)
            => WeaponConfigWrappers.GetRejectedHit(p, index);

        public static string WcGetPreviousHitRaw(this Player p, int index)
            => WeaponConfigWrappers.GetPreviousHit(p, index);

        public static int WcGetLastAnimation(this Player p)
            => WeaponConfigWrappers.GetLastAnimation(p);

        public static int WcGetLastStopTick(this Player p)
            => WeaponConfigWrappers.GetLastStopTick(p);

        public static Weapon WcGetLastExplosive(this Player p)
            => WeaponConfigWrappers.GetLastExplosive(p);

        public static SpawnInfo? WcGetSpawnInfo(this Player p)
            => WeaponConfigService.GetPlayerSpawnInfo(p);

        public static void WcSetSpawnInfo(this Player p, SpawnInfo info)
            => WeaponConfigService.SetPlayerSpawnInfo(p, info);
    }
}