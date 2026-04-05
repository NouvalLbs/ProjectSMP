#nullable enable
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.WeaponConfig
{
    public static class WeaponConfigWrappers
    {
        public static PlayerState GetPlayerState(Player p)
        {
            if (WeaponConfigService.IsPlayerDying(p))
                return PlayerState.Wasted;
            return p.State;
        }

        public static float GetPlayerHealth(Player p)
            => WeaponConfigService.GetWcHealth(p);

        public static float GetPlayerArmour(Player p)
            => WeaponConfigService.GetWcArmour(p);

        public static void SetPlayerHealth(Player p, float health, float armour = -1f)
            => WeaponConfigService.SetPlayerHealth(p, health, armour);

        public static void SetPlayerArmour(Player p, float armour)
            => WeaponConfigService.SetPlayerArmour(p, armour);

        public static int GetPlayerVirtualWorld(Player p)
            => WeaponConfigService.GetIntendedVirtualWorld(p);

        public static void SetPlayerVirtualWorld(Player p, int world)
        {
            var state = WeaponConfigService.GetPlayerState(p);
            if (state != null) state.IntendedWorld = world;
            if (!WeaponConfigService.IsPlayerDying(p))
                p.VirtualWorld = world;
        }

        public static void SendDeathMessage(Player? killer, Player killee, int weapon)
            => WeaponConfigService.SendWcDeathMessage(killer, killee, weapon);

        public static string GetWeaponName(int weaponId)
            => WeaponConfigService.GetWeaponName(weaponId);

        public static void SetPlayerTeam(Player p, int team)
            => WeaponConfigService.SetPlayerTeam(p, team);

        public static int GetPlayerTeam(Player p)
        {
            if (!p.IsConnected) return 255;
            return p.Team;
        }

        public static void ApplyAnimation(Player p, string animLib, string animName,
            float delta = 4.0f, bool loop = false, bool lockX = false, bool lockY = false,
            bool freeze = false, int time = 0, bool forceSync = false)
        {
            if (WeaponConfigService.IsPlayerDying(p)) return;
            p.ApplyAnimation(animLib, animName, delta, loop, lockX, lockY, freeze, time, forceSync);
        }

        public static void ClearAnimations(Player p, bool forceSync = true)
        {
            if (WeaponConfigService.IsPlayerDying(p)) return;

            var state = WeaponConfigService.GetPlayerState(p);
            if (state != null)
                state.LastStopTick = Environment.TickCount;

            p.ClearAnimations(forceSync);
        }

        public static void TogglePlayerControllable(Player p, bool toggle) {
            if (WeaponConfigService.IsPlayerDying(p)) return;
            var state = WeaponConfigService.GetPlayerState(p);
            if (state != null) state.LastStopTick = Environment.TickCount;
            p.ToggleControllable(toggle);
        }

        public static void SetPlayerPos(Player p, Vector3 pos)
        {
            if (WeaponConfigService.IsPlayerDying(p)) return;

            var state = WeaponConfigService.GetPlayerState(p);
            if (state != null)
                state.LastStopTick = Environment.TickCount;

            p.Position = pos;
        }

        public static void SetPlayerVelocity(Player p, Vector3 velocity)
        {
            if (WeaponConfigService.IsPlayerDying(p)) return;

            if (velocity.X == 0 && velocity.Y == 0 && velocity.Z == 0)
            {
                var state = WeaponConfigService.GetPlayerState(p);
                if (state != null)
                    state.LastStopTick = Environment.TickCount;
            }

            p.Velocity = velocity;
        }

        public static bool IsPlayerSpawned(Player p)
            => WeaponConfigService.IsPlayerSpawned(p);

        public static bool IsPlayerPaused(Player p)
            => WeaponConfigService.IsPlayerPaused(p);

        public static void DamagePlayer(Player target, float amount,
            Player? issuer = null, int weapon = 55, int bodypart = 0,
            bool ignoreArmour = false)
            => WeaponConfigService.DamagePlayer(target, amount, issuer,
                weapon, bodypart, ignoreArmour);

        public static void HealPlayer(Player p, float amount)
            => WeaponConfigService.HealPlayer(p, amount);

        public static void ResyncPlayer(Player p)
            => WeaponConfigService.ResyncPlayer(p);

        public static void SetPlayerMaxHealth(Player p, float max)
            => WeaponConfigService.SetPlayerMaxHealth(p, max);

        public static void SetPlayerMaxArmour(Player p, float max)
            => WeaponConfigService.SetPlayerMaxArmour(p, max);

        public static float GetPlayerMaxHealth(Player p)
            => WeaponConfigService.GetPlayerMaxHealth(p);

        public static float GetPlayerMaxArmour(Player p)
            => WeaponConfigService.GetPlayerMaxArmour(p);

        public static string GetRejectedHit(Player p, int index)
            => WeaponConfigService.GetRejectedHitFormatted(p, index);

        public static string GetPreviousHit(Player p, int index)
            => WeaponConfigService.GetPreviousHitFormatted(p, index);

        public static void SetMaxShootRateSamples(int samples)
            => WeaponConfigService.SetMaxShootRateSamples(samples);

        public static int GetMaxShootRateSamples()
            => WeaponConfigService.GetMaxShootRateSamples();

        public static void SetMaxHitRateSamples(int samples)
            => WeaponConfigService.SetMaxHitRateSamples(samples);

        public static int GetMaxHitRateSamples()
            => WeaponConfigService.GetMaxHitRateSamples();

        // Fixed: Added nullable return type
        public static PreviousHitInfo? GetPreviousHitRaw(Player p, int index)
            => WeaponConfigService.GetPreviousHit(p, index);

        // Fixed: Added nullable return type
        public static RejectedHit? GetRejectedHitRaw(Player p, int index)
            => WeaponConfigService.GetRejectedHit(p, index);

        public static void SetPlayerStreamDistance(float distance)
            => WeaponConfigService.SetPlayerStreamDistance(distance);

        public static float GetPlayerStreamDistance()
            => WeaponConfigService.GetPlayerStreamDistance();

        public static void SetMaxDistFromShot(float distance)
            => WeaponConfigService.SetMaxDistFromShot(distance);

        public static float GetMaxDistFromShot()
            => WeaponConfigService.GetMaxDistFromShot();

        public static void SetMaxDistFromOrigin(float distance)
            => WeaponConfigService.SetMaxDistFromOrigin(distance);

        public static float GetMaxDistFromOrigin()
            => WeaponConfigService.GetMaxDistFromOrigin();

        public static void SetShotTimeout(int ms)
            => WeaponConfigService.SetShotTimeout(ms);

        public static int GetShotTimeout()
            => WeaponConfigService.GetShotTimeout();

        public static void SetDeathSkipTimeout(int ms)
            => WeaponConfigService.SetDeathSkipTimeout(ms);

        public static int GetDeathSkipTimeout()
            => WeaponConfigService.GetDeathSkipTimeout();

        public static void SetMaxPreviousHits(int count)
            => WeaponConfigService.SetMaxPreviousHits(count);

        public static int GetMaxPreviousHits()
            => WeaponConfigService.GetMaxPreviousHits();

        public static bool IsBulletWeapon(int weaponId)
            => WeaponConfigService.IsBulletWeapon(weaponId);

        public static bool IsMeleeWeapon(int weaponId)
            => WeaponConfigService.IsMeleeWeapon(weaponId);

        public static bool IsShotgunWeapon(int weaponId)
            => WeaponConfigService.IsShotgunWeapon(weaponId);

        public static bool IsHighRateWeapon(int weaponId)
            => WeaponConfigService.IsHighRateWeapon(weaponId);

        public static void SetWeaponRangeDamage(int weaponId, DamageType damageType, params float[] values)
            => WeaponConfigService.SetWeaponRangeDamage(weaponId, damageType, values);

        public static void TogglePlayerSpectating(Player p, bool toggle)
        {
            if (toggle)
            {
                WeaponConfigService.CancelDeathForSpectating(p);
                WeaponConfigVendingMachines.OnStartSpectating(p);
                p.ToggleSpectating(true);
            }
            else
            {
                WeaponConfigService.OnStopSpectating(p);
                p.ToggleSpectating(false);
            }
        }

        public static void SpawnPlayer(Player p)
        {
            if (WeaponConfigService.IsPlayerDying(p)) return;
            p.Spawn();
        }

        public static void ForceClassSelection(Player p)
            => WeaponConfigService.ForcePlayerClassSelection(p);

        public static bool IsInClassSelection(Player p)
            => WeaponConfigService.IsPlayerInClassSelection(p);

        public static void SetPlayerPosFindZ(Player p, float x, float y, float z) {
            if (p.IsDisposed || WeaponConfigService.IsPlayerDying(p)) return;
            var state = WeaponConfigService.GetPlayerState(p);
            if (state != null) state.LastStopTick = Environment.TickCount;
            var currentZ = p.Position.Z;
            if (currentZ > z)
                p.Position = new Vector3(x, y, currentZ);
            else
                p.Position = new Vector3(x, y, z);
        }

        public static void SetWeaponName(int weaponId, string name)
            => WeaponConfigService.SetWeaponName(weaponId, name);

        public static void SetDisableSyncBugs(bool toggle)
            => WeaponConfigService.SetDisableSyncBugs(toggle);

        public static void SetKnifeSync(bool toggle)
            => WeaponConfigService.SetKnifeSync(toggle);

        public static void SetSpawnInfo(Player p, int team, int skin, float x, float y, float z,
            float rotation, Weapon weapon1 = Weapon.None, int ammo1 = 0,
            Weapon weapon2 = Weapon.None, int ammo2 = 0,
            Weapon weapon3 = Weapon.None, int ammo3 = 0)
        {
            var info = new SpawnInfo
            {
                Skin = skin,
                Team = team,
                Position = new Vector3(x, y, z),
                Rotation = rotation,
                Weapon1 = weapon1,
                Ammo1 = ammo1,
                Weapon2 = weapon2,
                Ammo2 = ammo2,
                Weapon3 = weapon3,
                Ammo3 = ammo3
            };

            WeaponConfigService.SetPlayerSpawnInfo(p, info);

            p.SetSpawnInfo(team, skin, new Vector3(x, y, z), rotation,
                weapon1, ammo1, weapon2, ammo2, weapon3, ammo3);
        }

        public static void PlayerSpectatePlayer(Player spectator, Player target, SpectateMode mode = SpectateMode.Normal)
        {
            spectator.SpectatePlayer(target, mode);
            WeaponConfigService.OnStartSpectating(spectator, target);
        }

        public static void StopSpectating(Player p)
        {
            WeaponConfigService.OnStopSpectating(p);
            p.ToggleSpectating(false);
        }

        public static bool IsPlayerInCheckpoint(Player p)
        {
            if (!WeaponConfigService.IsPlayerSpawned(p)) return false;
            return p.InCheckpoint;
        }

        public static bool IsPlayerInRaceCheckpoint(Player p)
        {
            if (!WeaponConfigService.IsPlayerSpawned(p)) return false;
            return p.InRaceCheckpoint;
        }

        public static void SetPlayerSpecialAction(Player p, SpecialAction action)
        {
            if (!WeaponConfigService.IsPlayerSpawned(p)) return;
            p.SpecialAction = action;
        }

        public static int GetLastAnimation(Player p) {
            var state = WeaponConfigService.GetPlayerState(p);
            return state?.LastAnim ?? -1;
        }

        public static int GetLastStopTick(Player p)
        {
            if (p.Id >= 0 && p.Id < 500)
            {
                var state = WeaponConfigService.GetPlayerState(p);
                return state?.LastStopTick ?? 0;
            }
            return 0;
        }

        public static Weapon GetLastExplosive(Player p)
        {
            var state = WeaponConfigService.GetPlayerState(p);
            return state?.LastExplosive ?? Weapon.None;
        }

        public static void SetCbugDeathDelay(bool toggle)
            => WeaponConfigService.SetCbugDeathDelay(toggle);

        public static int AddPlayerClass(int modelid, float spawnX, float spawnY, float spawnZ,
            float zAngle, Weapon weapon1 = Weapon.None, int weapon1Ammo = 0,
            Weapon weapon2 = Weapon.None, int weapon2Ammo = 0,
            Weapon weapon3 = Weapon.None, int weapon3Ammo = 0)
        {
            return WeaponConfigService.AddPlayerClass(modelid,
                new SampSharp.GameMode.Vector3(spawnX, spawnY, spawnZ), zAngle,
                weapon1, weapon1Ammo, weapon2, weapon2Ammo, weapon3, weapon3Ammo);
        }

        public static int AddPlayerClassEx(int teamid, int modelid, float spawnX, float spawnY,
            float spawnZ, float zAngle, Weapon weapon1 = Weapon.None, int weapon1Ammo = 0,
            Weapon weapon2 = Weapon.None, int weapon2Ammo = 0,
            Weapon weapon3 = Weapon.None, int weapon3Ammo = 0)
        {
            return WeaponConfigService.AddPlayerClassEx(teamid, modelid,
                new SampSharp.GameMode.Vector3(spawnX, spawnY, spawnZ), zAngle,
                weapon1, weapon1Ammo, weapon2, weapon2Ammo, weapon3, weapon3Ammo);
        }

        // Fixed: Added nullable return type
        public static SpawnClassInfo? GetSpawnClass(int classid)
            => WeaponConfigService.GetSpawnClass(classid);

        public static bool GetSpawnClassInfo(int classid, out SpawnClassInfo? info)
        {
            info = WeaponConfigService.GetSpawnClass(classid);
            return info != null;
        }

        public static int AverageShootRate(Player p, int shots)
            => WeaponConfigService.AverageShootRate(p, shots);

        public static int AverageShootRate(Player p, int shots, out bool multipleWeapons)
            => WeaponConfigService.AverageShootRate(p, shots, out multipleWeapons);

        public static int AverageHitRate(Player p, int hits)
            => WeaponConfigService.AverageHitRate(p, hits);

        public static int AverageHitRate(Player p, int hits, out bool multipleWeapons)
            => WeaponConfigService.AverageHitRate(p, hits, out multipleWeapons);

        public static float GetLastDamageHealth(Player p)
            => WeaponConfigService.GetLastDamageHealth(p);

        public static float GetLastDamageArmour(Player p)
            => WeaponConfigService.GetLastDamageArmour(p);

        public static void SetLagCompMode(LagCompMode mode)
            => WeaponConfigService.SetLagCompMode(mode);

        public static LagCompMode GetLagCompMode()
            => WeaponConfigService.GetLagCompMode();

        public static int CreateVehicle(int modelid, float x, float y, float z, float rotation, int color1, int color2, int respawnDelay, bool addSiren = false)
        {
            var vehicle = BaseVehicle.Create(
                (VehicleModelType)modelid,
                new Vector3(x, y, z), rotation, color1, color2, respawnDelay, addSiren);

            return vehicle?.Id ?? -1;
        }

        public static void DestroyVehicle(int vehicleid)
        {
            var vehicle = BaseVehicle.Find(vehicleid);
            if (vehicle == null) return;
            WeaponConfigService.OnVehicleDestroy(vehicleid);
            vehicle.Dispose();
        }

        public static int AddStaticVehicle(int modelid, float x, float y, float z, float rotation, int color1, int color2)
        {
            var vehicle = BaseVehicle.Create(
                (VehicleModelType)modelid,
                new Vector3(x, y, z), rotation, color1, color2);

            return vehicle?.Id ?? -1;
        }

        public static int AddStaticVehicleEx(int modelid, float x, float y, float z, float rotation, int color1, int color2, int respawnDelay, bool addSiren = false)
        {
            var vehicle = BaseVehicle.Create(
                (VehicleModelType)modelid,
                new Vector3(x, y, z), rotation, color1, color2, respawnDelay, addSiren);

            return vehicle?.Id ?? -1;
        }

        public static void EnableHealthBarForPlayer(Player p, bool enable)
            => WeaponConfigHealthBar.SetEnabled(p, enable);

        public static WeaponEntry? GetWeaponEntry(int weaponId)
            => WeaponConfigService.GetWeaponEntryPublic(weaponId);

        public static void ModifyWeaponEntry(int weaponId, Action<WeaponEntry> modifier)
            => WeaponConfigService.ModifyWeaponEntry(weaponId, modifier);

        public static string GetBodypartName(int bodypart)
            => WeaponConfigService.GetBodypartName(bodypart);

        public static void SetVehiclePassengerDamage(bool toggle)
            => WeaponConfigService.SetVehiclePassengerDamage(toggle);

        public static void SetVehicleUnoccupiedDamage(bool toggle)
            => WeaponConfigService.SetVehicleUnoccupiedDamage(toggle);

        public static void SetCustomArmourRules(bool armourRules, bool torsoRules = false)
            => WeaponConfigService.SetCustomArmourRules(armourRules, torsoRules);

        public static void SetWeaponArmourRule(int weaponId, bool affectsArmour, bool torsoOnly = false)
            => WeaponConfigService.SetWeaponArmourRule(weaponId, affectsArmour, torsoOnly);

        public static void SetRespawnTime(int ms)
            => WeaponConfigService.SetRespawnTime(ms);

        public static int GetRespawnTime()
            => WeaponConfigService.GetRespawnTime();

        public static void SetDamageSounds(int taken, int given)
            => WeaponConfigService.SetDamageSounds(taken, given);

        // Fixed: Made player parameter nullable with default value
        public static void SetCbugAllowed(bool enabled, Player? p = null)
            => WeaponConfigService.SetCbugAllowed(enabled, p);

        // Fixed: Made player parameter nullable with default value
        public static bool GetCbugAllowed(Player? p = null)
            => WeaponConfigService.GetCbugAllowed(p);

        public static void SetCustomFallDamage(bool toggle, float multiplier = 25f, float deathVel = -0.6f)
            => WeaponConfigService.SetCustomFallDamage(toggle, multiplier, deathVel);

        public static void SetCustomVendingMachines(bool enable)
            => WeaponConfigService.SetCustomVendingMachines(enable);

        public static void SetDamageFeed(bool enable)
            => WeaponConfigService.SetDamageFeed(enable);

        public static void SetDamageFeedForPlayer(Player p, bool enable)
            => WeaponConfigDamageFeed.SetEnabled(p, enable);

        // Fixed: Made player parameter nullable with default value
        public static bool IsDamageFeedActive(Player? p = null)
            => WeaponConfigService.IsDamageFeedActive(p);

        public static void SetWeaponDamage(int id, float dmg, DamageType type = DamageType.Static)
            => WeaponConfigService.SetWeaponDamage(id, dmg, type);

        public static float GetWeaponDamage(int id)
            => WeaponConfigService.GetWeaponDamage(id);

        public static void SetWeaponMaxRange(int id, float range)
            => WeaponConfigService.SetWeaponMaxRange(id, range);

        public static float GetWeaponMaxRange(int id)
            => WeaponConfigService.GetWeaponMaxRange(id);

        public static void SetWeaponShootRate(int id, int rate)
            => WeaponConfigService.SetWeaponShootRate(id, rate);

        public static int GetWeaponShootRate(int id)
            => WeaponConfigService.GetWeaponShootRate(id);

        public static string ReturnWeaponName(int weaponId)
            => GetWeaponName(weaponId);

        public static void ResumeDeath(Player p, string animLib = "PED", string animName = "FLOOR_HIT")
            => WeaponConfigService.ResumeDeath(p, animLib, animName);

        public static void ForceRespawnFromDeath(Player p)
            => WeaponConfigService.ForceRespawnFromDeath(p);
    }
}