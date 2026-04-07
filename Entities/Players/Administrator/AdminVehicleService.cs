using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using System.Collections.Generic;

namespace ProjectSMP.Entities.Players.Administrator
{
    public static class AdminVehicleService
    {
        private static readonly Dictionary<int, Vehicle> _vehicles = new();

        public static void Spawn(Player player, int modelId)
        {
            Cleanup(player);

            var pos = player.Position;
            var vehicle = Vehicle.CreateVehicle(
                (VehicleModelType)modelId,
                new Vector3(pos.X + 2f, pos.Y, pos.Z),
                player.Angle,
                -1, -1);

            vehicle.VehicleType = VehicleType.Admin;
            vehicle.VirtualWorld = player.VirtualWorld;
            vehicle.LinkToInterior(player.Interior);

            _vehicles[player.Id] = vehicle;
            player.PutInVehicle(vehicle, 0);
        }

        public static void Cleanup(Player player)
        {
            if (_vehicles.TryGetValue(player.Id, out var v) && !v.IsDisposed)
                v.Destroy();
            _vehicles.Remove(player.Id);
        }
    }
}