// SafeServerExtensions.cs
#nullable enable
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using ProjectSMP.Plugins.Anticheat;
using ProjectSMP.Plugins.WeaponConfig;
using ProjectSMP.Plugins.EVF2;

namespace ProjectSMP.Extensions;

public static class SafeServerExtensions
{
    private static AnticheatPlugin? _anticheat;

    public static void Initialize(AnticheatPlugin anticheat)
    {
        _anticheat = anticheat;
    }

    public static void EnableStuntBonusForAllSafe(bool enable)
    {
        foreach (var p in BasePlayer.All)
        {
            if (p is Player player)
                player.EnableStuntBonus(enable);
        }
        _anticheat?.OnEnableStuntBonusForAll(enable);
    }

    public static int AddPlayerClassSafe(int skin, Vector3 position, float rotation,
        Weapon weapon1 = Weapon.None, int ammo1 = 0,
        Weapon weapon2 = Weapon.None, int ammo2 = 0,
        Weapon weapon3 = Weapon.None, int ammo3 = 0)
    {
        return WeaponConfigWrappers.AddPlayerClass(skin, position.X, position.Y, position.Z, rotation,
            weapon1, ammo1, weapon2, ammo2, weapon3, ammo3);
    }

    public static int AddPlayerClassExSafe(int team, int skin, Vector3 position, float rotation,
        Weapon weapon1 = Weapon.None, int ammo1 = 0,
        Weapon weapon2 = Weapon.None, int ammo2 = 0,
        Weapon weapon3 = Weapon.None, int ammo3 = 0)
    {
        return WeaponConfigWrappers.AddPlayerClassEx(team, skin, position.X, position.Y, position.Z, rotation,
            weapon1, ammo1, weapon2, ammo2, weapon3, ammo3);
    }

    public static BaseVehicle CreateVehicleSafe(VehicleModelType model, Vector3 position, float rotation, int color1, int color2, int respawnDelay = -1, bool addSiren = false)
    {
        var vehicle = SafeVehicleExtensions.CreateSafe(model, position, rotation, color1, color2, respawnDelay, addSiren);
        EVFService.RegisterVehicle(vehicle.Id, (int)model, position, rotation, color1, color2, 0, 0, false);
        EVFService.SetFuelEnabled(vehicle.Id, true);
        return vehicle;
    }

    public static void RegisterPickupSafe(int pickupId, float x, float y, float z, int type = 0, int weapon = 0, int amount = 0)
    {
        _anticheat?.OnRegisterPickup(pickupId, x, y, z, type, weapon, amount);
    }

    public static void DestroyPickupSafe(int pickupId)
    {
        _anticheat?.OnDestroyPickup(pickupId);
    }

    public static void SendDeathMessageSafe(BasePlayer? killer, BasePlayer victim, int weapon)
    {
        if (victim is Player p)
        {
            WeaponConfigWrappers.SendDeathMessage(killer as Player, p, weapon);
        }
        else
        {
            BasePlayer.SendDeathMessageToAll(killer, victim, (Weapon)weapon);
        }
    }

    public static void SetDisableSyncBugsSafe(bool toggle)
    {
        WeaponConfigWrappers.SetDisableSyncBugs(toggle);
    }

    public static void SetKnifeSyncSafe(bool toggle)
    {
        WeaponConfigWrappers.SetKnifeSync(toggle);
    }

    public static void SetCbugAllowedSafe(bool allowed)
    {
        WeaponConfigWrappers.SetCbugAllowed(allowed);
    }

    public static void SetCustomFallDamageSafe(bool toggle, float multiplier = 25f, float deathVel = -0.6f)
    {
        WeaponConfigWrappers.SetCustomFallDamage(toggle, multiplier, deathVel);
    }

    public static void SetCustomVendingMachinesSafe(bool enable)
    {
        WeaponConfigWrappers.SetCustomVendingMachines(enable);
    }

    public static void SetDamageFeedSafe(bool enable)
    {
        WeaponConfigWrappers.SetDamageFeed(enable);
    }

    public static void SetLagCompModeSafe(LagCompMode mode)
    {
        WeaponConfigWrappers.SetLagCompMode(mode);
    }

    public static void SetRespawnTimeSafe(int ms)
    {
        WeaponConfigWrappers.SetRespawnTime(ms);
    }

    public static void SetWeaponDamageSafe(int weaponId, float damage, DamageType type = DamageType.Static)
    {
        WeaponConfigWrappers.SetWeaponDamage(weaponId, damage, type);
    }

    public static void SetWeaponMaxRangeSafe(int weaponId, float range)
    {
        WeaponConfigWrappers.SetWeaponMaxRange(weaponId, range);
    }

    public static void SetWeaponShootRateSafe(int weaponId, int rate)
    {
        WeaponConfigWrappers.SetWeaponShootRate(weaponId, rate);
    }

    public static void SetWeaponNameSafe(int weaponId, string name)
    {
        WeaponConfigWrappers.SetWeaponName(weaponId, name);
    }

    public static void SetVehiclePassengerDamageSafe(bool toggle)
    {
        WeaponConfigWrappers.SetVehiclePassengerDamage(toggle);
    }

    public static void SetVehicleUnoccupiedDamageSafe(bool toggle)
    {
        WeaponConfigWrappers.SetVehicleUnoccupiedDamage(toggle);
    }
}