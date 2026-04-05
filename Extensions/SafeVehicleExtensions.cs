// SafeVehicleExtensions.cs
#nullable enable
using ProjectSMP.Entities;
using ProjectSMP.Plugins.Anticheat;
using ProjectSMP.Plugins.WeaponConfig;
using ProjectSMP.Plugins.EVF2;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;

namespace ProjectSMP.Extensions;

public static class SafeVehicleExtensions
{
    private static AnticheatPlugin? _anticheat;

    public static void Initialize(AnticheatPlugin anticheat)
    {
        _anticheat = anticheat;
    }

    public static void SetHealthSafe(this BaseVehicle vehicle, float health)
    {
        vehicle.Health = health;
        EVFService.SetVehicleHealth(vehicle.Id, health); // sync EVF2 internal state
        _anticheat?.OnSetVehicleHealth(vehicle.Id, health);
    }

    public static void RepairSafe(this BaseVehicle vehicle)
    {
        vehicle.Repair();
        EVFService.SetVehicleHealth(vehicle.Id, 1000f);
        _anticheat?.OnRepairVehicle(vehicle.Id);
    }

    public static void SetPositionSafe(this BaseVehicle vehicle, Vector3 position)
    {
        vehicle.Position = position;
        EVFService.SetVehiclePosition(vehicle.Id, position);
        _anticheat?.OnSetVehiclePos(vehicle.Id, position.X, position.Y, position.Z);
    }

    public static void SetPositionSafe(this BaseVehicle vehicle, float x, float y, float z)
    {
        vehicle.SetPositionSafe(new Vector3(x, y, z));
    }

    public static void SetVelocitySafe(this BaseVehicle vehicle, Vector3 velocity)
    {
        vehicle.Velocity = velocity;
        _anticheat?.OnVehicleVelocitySet(vehicle.Id);
    }

    public static void SetVelocitySafe(this BaseVehicle vehicle, float x, float y, float z)
    {
        vehicle.SetVelocitySafe(new Vector3(x, y, z));
    }

    public static void UpdateDamageStatusSafe(this BaseVehicle vehicle, int panels, int doors, int lights, int tires)
    {
        vehicle.UpdateDamageStatus(panels, doors, lights, tires);
        _anticheat?.OnUpdateVehicleDamageStatus(vehicle.Id, panels, doors, lights, tires);
    }

    public static void SetPaintjobSafe(this BaseVehicle vehicle, int paintjobId)
    {
        vehicle.ChangePaintjob(paintjobId);
        _anticheat?.OnChangeVehiclePaintjob(vehicle.Id, paintjobId);
    }

    public static void LinkToInteriorSafe(this BaseVehicle vehicle, int interiorId)
    {
        vehicle.LinkToInterior(interiorId);
        _anticheat?.OnLinkVehicleToInterior(vehicle.Id, interiorId);
    }

    public static void SetParamsExSafe(this BaseVehicle vehicle, bool engine, bool lights, bool alarm, bool doorsLocked, bool bonnet, bool boot, bool objective)
    {
        vehicle.SetParameters(engine, lights, alarm, doorsLocked, bonnet, boot, objective);
        _anticheat?.OnSetVehicleParamsEx(vehicle.Id, doorsLocked);
    }

    public static void SetParamsForPlayerSafe(this BaseVehicle vehicle, BasePlayer player, bool objective, bool doorsLocked)
    {
        _anticheat?.OnSetVehicleParamsForPlayer(vehicle.Id, player.Id, doorsLocked);
    }

    public static void RespawnSafe(this BaseVehicle vehicle)
    {
        vehicle.Respawn();
        _anticheat?.OnSetVehicleToRespawn(vehicle.Id);
    }

    public static Vehicle CreateSafe(VehicleModelType model, Vector3 position, float rotation, int color1, int color2, int respawnDelay = -1, bool addSiren = false)
    {
        return Vehicle.CreateVehicle(model, position, rotation, color1, color2, respawnDelay, addSiren);
    }

    public static void DisposeSafe(this BaseVehicle vehicle)
    {
        WeaponConfigService.OnVehicleDestroy(vehicle.Id);
        vehicle.Dispose();
    }
}