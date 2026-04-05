using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.Configuration;

public class AnticheatConfig
{
    public bool Enabled { get; set; } = true;
    public string LogPath { get; set; } = "logs/anticheat.log";
    public int MaxPing { get; set; } = 500;
    public int MaxConnectsPerIp { get; set; } = 1;
    public int MinReconnectSeconds { get; set; } = 12;
    public int SpeedHackVehResetDelay { get; set; } = 3;

    public Dictionary<string, CheckConfig> Checks { get; set; } = BuildDefaults();

    public CheckConfig GetCheck(string name) =>
        Checks.TryGetValue(name, out var c) ? c : Checks[name] = new CheckConfig();

    private static Dictionary<string, CheckConfig> BuildDefaults() => new()
    {
        ["AirBreakOnfoot"] = new() { MaxWarnings = 4 },
        ["AirBreakVehicle"] = new() { MaxWarnings = 4 },
        ["TeleportOnfoot"] = new() { MaxWarnings = 1 },
        ["TeleportVehicle"] = new() { MaxWarnings = 1 },
        ["TeleportVehicleEnter"] = new() { MaxWarnings = 1 },
        ["TeleportPickup"] = new() { MaxWarnings = 1 },
        ["TeleportVehicleToPlayer"] = new() { MaxWarnings = 1 },
        ["FlyHackOnfoot"] = new() { MaxWarnings = 2 },
        ["FlyHackVehicle"] = new() { MaxWarnings = 5 },
        ["SpeedHackOnfoot"] = new() { MaxWarnings = 6 },
        ["SpeedHackVehicle"] = new() { MaxWarnings = 3 },
        ["HealthHackOnfoot"] = new() { MaxWarnings = 3 },
        ["HealthHackVehicle"] = new() { MaxWarnings = 3 },
        ["ArmourHack"] = new() { MaxWarnings = 3 },
        ["MoneyHack"] = new() { MaxWarnings = 2 },
        ["WeaponHack"] = new() { MaxWarnings = 2 },
        ["AmmoHackAdd"] = new() { MaxWarnings = 3 },
        ["AmmoHackInfinite"] = new() { MaxWarnings = 3 },
        ["SpecialActionHack"] = new() { MaxWarnings = 3 },
        ["GodModeOnfoot"] = new() { MaxWarnings = 3 },
        ["GodModeVehicle"] = new() { MaxWarnings = 3 },
        ["InvisibleHack"] = new() { MaxWarnings = 3 },
        ["CameraHack"] = new() { MaxWarnings = 3 },
        ["AnimationHack"] = new() { MaxWarnings = 3 },
        ["CjRun"] = new() { MaxWarnings = 8 },
        ["LagCompSpoof"] = new() { MaxWarnings = 2 },
        ["QuickTurn"] = new() { MaxWarnings = 3 },
        ["RapidFire"] = new() { MaxWarnings = 16 },
        ["ProAim"] = new() { MaxWarnings = 2 },
        ["CarShot"] = new() { MaxWarnings = 4 },
        ["FullAiming"] = new() { MaxWarnings = 3 },
        ["FakeSpawn"] = new() { MaxWarnings = 3 },
        ["FakeKill"] = new() { MaxWarnings = 3 },
        ["AfkGhost"] = new() { MaxWarnings = 2 },
        ["CarJack"] = new() { MaxWarnings = 3 },
        ["TuningHack"] = new() { MaxWarnings = 3 },
        ["TuningCrasher"] = new() { MaxWarnings = 3 },
        ["Reconnect"] = new() { MaxWarnings = 3 },
        ["HighPing"] = new() { MaxWarnings = 8 },
        ["Sandbox"] = new() { MaxWarnings = 3 },
        ["InvalidVersion"] = new() { MaxWarnings = 3 },
        ["RconHack"] = new() { MaxWarnings = 3 },
        ["InvalidSeatCrasher"] = new() { MaxWarnings = 3 },
        ["DialogCrasher"] = new() { MaxWarnings = 3 },
        ["AttachedObjectCrasher"] = new() { MaxWarnings = 3 },
        ["WeaponCrasher"] = new() { MaxWarnings = 3 },
        ["ConnectionFlood"] = new() { MaxWarnings = 3 },
        ["CallbackFlood"] = new() { MaxWarnings = 3 },
        ["SeatFlood"] = new() { MaxWarnings = 3 },
        ["DoS"] = new() { MaxWarnings = 3 },
        ["ParkourMod"] = new() { Enabled = false, MaxWarnings = 3 },
        ["UnFreeze"] = new() { Enabled = false, MaxWarnings = 3 },
        ["FakeNpc"] = new() { Enabled = false, MaxWarnings = 3 },

        ["NopGiveWeapon"] = new() { MaxWarnings = 8 },
        ["NopSetAmmo"] = new() { MaxWarnings = 8 },
        ["NopSetInterior"] = new() { MaxWarnings = 8 },
        ["NopSetHealth"] = new() { MaxWarnings = 8 },
        ["NopSetVehicleHealth"] = new() { MaxWarnings = 8 },
        ["NopSetArmour"] = new() { MaxWarnings = 8 },
        ["NopSetSpecialAction"] = new() { MaxWarnings = 8 },
        ["NopPutInVehicle"] = new() { MaxWarnings = 8 },
        ["NopToggleSpectating"] = new() { MaxWarnings = 8 },
        ["NopSpawnPlayer"] = new() { MaxWarnings = 8 },
        ["NopSetPos"] = new() { MaxWarnings = 8 },
        ["NopRemoveFromVehicle"] = new() { MaxWarnings = 8 },
    };
}