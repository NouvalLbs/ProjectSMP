// SafeNativeExtensions.cs
#nullable enable
using ProjectSMP.Plugins.Anticheat;
using ProjectSMP.Plugins.WeaponConfig;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Extensions;

public static class SafeNativeExtensions
{
    private static AnticheatPlugin? _anticheat;

    public static void Initialize(AnticheatPlugin anticheat)
    {
        _anticheat = anticheat;
    }

    public static float GetHealthSafe(this BasePlayer player)
    {
        if (player is Player p)
            return WeaponConfigWrappers.GetPlayerHealth(p);
        return player.Health;
    }

    public static float GetArmourSafe(this BasePlayer player)
    {
        if (player is Player p)
            return WeaponConfigWrappers.GetPlayerArmour(p);
        return player.Armour;
    }

    public static void SetHealthSafe(this BasePlayer player, float health, float armour = -1f)
    {
        if (player is Player p)
        {
            WeaponConfigWrappers.SetPlayerHealth(p, health, armour);
            _anticheat?.OnSetPlayerHealth(player.Id, health);
        }
        else
        {
            player.Health = health;
        }
    }

    public static void SetArmourSafe(this BasePlayer player, float armour)
    {
        if (player is Player p)
        {
            WeaponConfigWrappers.SetPlayerArmour(p, armour);
            _anticheat?.OnSetPlayerArmour(player.Id, armour);
        }
        else
        {
            player.Armour = armour;
        }
    }

    public static void GivePlayerMoneySafe(this BasePlayer player, int amount)
    {
        player.Money += amount;
        _anticheat?.OnGivePlayerMoney(player.Id, amount);
    }

    public static void ResetPlayerMoneySafe(this BasePlayer player)
    {
        player.Money = 0;
        _anticheat?.OnResetPlayerMoney(player.Id);
    }

    public static void GiveWeaponSafe(this BasePlayer player, Weapon weapon, int ammo)
    {
        player.GiveWeapon(weapon, ammo);
        _anticheat?.OnGivePlayerWeapon(player.Id, (int)weapon, ammo);
    }

    public static void SetAmmoSafe(this BasePlayer player, Weapon weapon, int ammo)
    {
        player.SetAmmo(weapon, ammo);
        _anticheat?.OnSetPlayerAmmo(player.Id, (int)weapon, ammo);
    }

    public static void ResetWeaponsSafe(this BasePlayer player)
    {
        player.ResetWeapons();
        _anticheat?.OnResetPlayerWeapons(player.Id);
    }

    public static void SetInteriorSafe(this BasePlayer player, int interiorId)
    {
        player.Interior = interiorId;
        _anticheat?.OnSetPlayerInterior(player.Id, interiorId);
    }

    public static void SetPositionSafe(this BasePlayer player, Vector3 position)
    {
        if (player is Player p)
        {
            WeaponConfigWrappers.SetPlayerPos(p, position);
            _anticheat?.OnSetPlayerPos(player.Id, position.X, position.Y, position.Z);
        }
        else
        {
            player.Position = position;
            _anticheat?.OnSetPlayerPos(player.Id, position.X, position.Y, position.Z);
        }
    }

    public static void SetPositionSafe(this BasePlayer player, float x, float y, float z)
    {
        player.SetPositionSafe(new Vector3(x, y, z));
    }

    public static void SetPosFindZSafe(this BasePlayer player, float x, float y, float z)
    {
        if (player is Player p)
        {
            WeaponConfigWrappers.SetPlayerPosFindZ(p, x, y, z);
            _anticheat?.OnSetPlayerPosFindZ(player.Id, x, y, z);
        }
        else
        {
            player.Position = new Vector3(x, y, z);
            _anticheat?.OnSetPlayerPosFindZ(player.Id, x, y, z);
        }
    }

    public static void SetVelocitySafe(this BasePlayer player, Vector3 velocity)
    {
        if (player is Player p)
        {
            WeaponConfigWrappers.SetPlayerVelocity(p, velocity);
            _anticheat?.OnPlayerVelocitySet(player.Id);
        }
        else
        {
            player.Velocity = velocity;
            _anticheat?.OnPlayerVelocitySet(player.Id);
        }
    }

    public static void SetVelocitySafe(this BasePlayer player, float x, float y, float z)
    {
        player.SetVelocitySafe(new Vector3(x, y, z));
    }

    public static void ToggleControllableSafe(this BasePlayer player, bool toggle)
    {
        if (player is Player p)
        {
            WeaponConfigWrappers.TogglePlayerControllable(p, toggle);
            _anticheat?.OnTogglePlayerControllable(player.Id, toggle);
        }
        else
        {
            player.ToggleControllable(toggle);
            _anticheat?.OnTogglePlayerControllable(player.Id, toggle);
        }
    }

    public static void SpawnPlayerSafe(this BasePlayer player)
    {
        if (player is Player p)
        {
            WeaponConfigWrappers.SpawnPlayer(p);
            _anticheat?.OnSpawnPlayer(player.Id);
        }
        else
        {
            player.Spawn();
            _anticheat?.OnSpawnPlayer(player.Id);
        }
    }

    public static void ToggleSpectatingSafe(this BasePlayer player, bool toggle)
    {
        if (player is Player p)
        {
            WeaponConfigWrappers.TogglePlayerSpectating(p, toggle);
            _anticheat?.OnTogglePlayerSpectating(player.Id, toggle);
        }
        else
        {
            player.ToggleSpectating(toggle);
            _anticheat?.OnTogglePlayerSpectating(player.Id, toggle);
        }
    }

    public static void SetSpecialActionSafe(this BasePlayer player, SpecialAction action)
    {
        if (player is Player p)
        {
            WeaponConfigWrappers.SetPlayerSpecialAction(p, action);
            _anticheat?.OnSetPlayerSpecialAction(player.Id, (int)action);
        }
        else
        {
            player.SpecialAction = action;
            _anticheat?.OnSetPlayerSpecialAction(player.Id, (int)action);
        }
    }

    public static void ApplyAnimationSafe(this BasePlayer player, string animLib, string animName,
        float delta = 4.0f, bool loop = false, bool lockX = false, bool lockY = false,
        bool freeze = false, int time = 0, bool forceSync = false)
    {
        if (player is Player p)
        {
            WeaponConfigWrappers.ApplyAnimation(p, animLib, animName, delta, loop, lockX, lockY, freeze, time, forceSync);
        }
        else
        {
            player.ApplyAnimation(animLib, animName, delta, loop, lockX, lockY, freeze, time, forceSync);
        }
    }

    public static void ClearAnimationsSafe(this BasePlayer player, bool forceSync = true)
    {
        if (player is Player p)
        {
            WeaponConfigWrappers.ClearAnimations(p, forceSync);
        }
        else
        {
            player.ClearAnimations(forceSync);
        }
    }

    public static void PutInVehicleSafe(this BasePlayer player, BaseVehicle vehicle, int seatId = 0)
    {
        player.PutInVehicle(vehicle, seatId);
        _anticheat?.OnPutPlayerInVehicle(player.Id, vehicle.Id, seatId);
    }

    public static void RemoveFromVehicleSafe(this BasePlayer player)
    {
        player.RemoveFromVehicle();
        _anticheat?.OnRemovePlayerFromVehicle(player.Id);
    }

    public static void SetVirtualWorldSafe(this BasePlayer player, int worldId)
    {
        if (player is Player p)
        {
            WeaponConfigWrappers.SetPlayerVirtualWorld(p, worldId);
        }
        else
        {
            player.VirtualWorld = worldId;
        }
    }

    public static int GetVirtualWorldSafe(this BasePlayer player)
    {
        if (player is Player p)
            return WeaponConfigWrappers.GetPlayerVirtualWorld(p);
        return player.VirtualWorld;
    }

    public static PlayerState GetStateSafe(this BasePlayer player)
    {
        if (player is Player p)
            return WeaponConfigWrappers.GetPlayerState(p);
        return player.State;
    }

    public static void SetTeamSafe(this BasePlayer player, int team)
    {
        if (player is Player p)
        {
            WeaponConfigWrappers.SetPlayerTeam(p, team);
        }
        else
        {
            player.Team = team;
        }
    }

    public static int GetTeamSafe(this BasePlayer player)
    {
        if (player is Player p)
            return WeaponConfigWrappers.GetPlayerTeam(p);
        return player.Team;
    }

    public static void SetSpawnInfoSafe(this BasePlayer player, int team, int skin, float x, float y, float z,
        float rotation, Weapon weapon1 = Weapon.None, int ammo1 = 0,
        Weapon weapon2 = Weapon.None, int ammo2 = 0,
        Weapon weapon3 = Weapon.None, int ammo3 = 0)
    {
        if (player is Player p)
        {
            WeaponConfigWrappers.SetSpawnInfo(p, team, skin, x, y, z, rotation, weapon1, ammo1, weapon2, ammo2, weapon3, ammo3);
            _anticheat?.OnSetPlayerSpawnInfo(player.Id, x, y, z, (int)weapon1, ammo1, (int)weapon2, ammo2, (int)weapon3, ammo3);
        }
        else
        {
            player.SetSpawnInfo(team, skin, new Vector3(x, y, z), rotation, weapon1, ammo1, weapon2, ammo2, weapon3, ammo3);
            _anticheat?.OnSetPlayerSpawnInfo(player.Id, x, y, z, (int)weapon1, ammo1, (int)weapon2, ammo2, (int)weapon3, ammo3);
        }
    }

    public static void SpectatePlayerSafe(this BasePlayer spectator, BasePlayer target, SpectateMode mode = SpectateMode.Normal)
    {
        if (spectator is Player specPlayer && target is Player targetPlayer)
        {
            WeaponConfigWrappers.PlayerSpectatePlayer(specPlayer, targetPlayer, mode);
            _anticheat?.OnPlayerSpectatePlayerOrVehicle(spectator.Id);
        }
        else
        {
            spectator.SpectatePlayer(target, mode);
            _anticheat?.OnPlayerSpectatePlayerOrVehicle(spectator.Id);
        }
    }

    public static void SpectateVehicleSafe(this BasePlayer player, BaseVehicle vehicle, SpectateMode mode = SpectateMode.Normal)
    {
        player.SpectateVehicle(vehicle, mode);
        _anticheat?.OnPlayerSpectatePlayerOrVehicle(player.Id);
    }

    public static void ForceClassSelectionSafe(this BasePlayer player)
    {
        if (player is Player p)
        {
            WeaponConfigWrappers.ForceClassSelection(p);
        }
        else
        {
            player.ToggleSpectating(true);
        }
    }

    public static bool IsInCheckpointSafe(this BasePlayer player)
    {
        if (player is Player p)
            return WeaponConfigWrappers.IsPlayerInCheckpoint(p);
        return player.InCheckpoint;
    }

    public static bool IsInRaceCheckpointSafe(this BasePlayer player)
    {
        if (player is Player p)
            return WeaponConfigWrappers.IsPlayerInRaceCheckpoint(p);
        return player.InRaceCheckpoint;
    }

    public static bool IsSpawnedSafe(this BasePlayer player)
    {
        if (player is Player p)
            return WeaponConfigWrappers.IsPlayerSpawned(p);
        return player.State >= PlayerState.OnFoot && player.State <= PlayerState.Passenger;
    }

    public static bool IsDyingSafe(this BasePlayer player)
    {
        if (player is Player p)
            return WeaponConfigService.IsPlayerDying(p);
        return false;
    }

    public static bool IsPausedSafe(this BasePlayer player)
    {
        if (player is Player p)
            return WeaponConfigWrappers.IsPlayerPaused(p);
        return false;
    }

    public static void DamagePlayerSafe(this BasePlayer player, float amount, BasePlayer? issuer = null, int weapon = 55, int bodypart = 0, bool ignoreArmour = false)
    {
        if (player is not Player p) return;

        Player? issuerPlayer = issuer as Player;
        WeaponConfigWrappers.DamagePlayer(p, amount, issuerPlayer, weapon, bodypart, ignoreArmour);
    }

    public static void HealPlayerSafe(this BasePlayer player, float amount)
    {
        if (player is Player p)
        {
            WeaponConfigWrappers.HealPlayer(p, amount);
        }
        else
        {
            player.Health = Math.Min(player.Health + amount, 100f);
        }
    }

    public static void ResyncPlayerSafe(this BasePlayer player)
    {
        if (player is Player p)
        {
            WeaponConfigWrappers.ResyncPlayer(p);
        }
    }

    public static void SetMaxHealthSafe(this BasePlayer player, float max)
    {
        if (player is Player p)
        {
            WeaponConfigWrappers.SetPlayerMaxHealth(p, max);
        }
    }

    public static void SetMaxArmourSafe(this BasePlayer player, float max)
    {
        if (player is Player p)
        {
            WeaponConfigWrappers.SetPlayerMaxArmour(p, max);
        }
    }

    public static float GetMaxHealthSafe(this BasePlayer player)
    {
        if (player is Player p)
            return WeaponConfigWrappers.GetPlayerMaxHealth(p);
        return 100f;
    }

    public static float GetMaxArmourSafe(this BasePlayer player)
    {
        if (player is Player p)
            return WeaponConfigWrappers.GetPlayerMaxArmour(p);
        return 100f;
    }

    public static void EnableHealthBarSafe(this BasePlayer player, bool enable)
    {
        if (player is Player p)
        {
            WeaponConfigWrappers.EnableHealthBarForPlayer(p, enable);
        }
    }

    public static void SetDamageFeedSafe(this BasePlayer player, bool enable)
    {
        if (player is Player p)
        {
            WeaponConfigWrappers.SetDamageFeedForPlayer(p, enable);
        }
    }

    public static void SetAttachedObjectSafe(this BasePlayer player, int slot, int modelId, Bone bone,
        Vector3 offset = default, Vector3 rotation = default, Vector3 scale = default,
        int materialColor1 = 0, int materialColor2 = 0)
    {
        player.SetAttachedObject(slot, modelId, bone, offset, rotation, scale, materialColor1, materialColor2);
        _anticheat?.OnSetAttachedObject(player.Id, slot, modelId);
    }

    public static void EnableStuntBonusSafe(this BasePlayer player, bool enable)
    {
        player.EnableStuntBonus(enable);
        _anticheat?.OnEnableStuntBonusForPlayer(player.Id, enable);
    }
}