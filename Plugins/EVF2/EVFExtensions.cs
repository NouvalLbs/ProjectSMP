#nullable enable
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;

namespace ProjectSMP.Plugins.EVF2
{
    public static class EVFExtensions
    {
        public const int DoorDriver = 1;
        public const int DoorPassenger = 2;
        public const int DoorBackLeft = 3;
        public const int DoorBackRight = 4;

        public static bool GetWindowState(int vehicleId, int doorId)
        {
            var v = BaseVehicle.Find(vehicleId);
            if (v == null) return false;
            v.GetWindowsParameters(out bool d, out bool p, out bool bl, out bool br);
            return doorId switch { DoorDriver => d, DoorPassenger => p, DoorBackLeft => bl, DoorBackRight => br, _ => false };
        }

        public static bool SetWindowState(int vehicleId, int doorId, bool state)
        {
            if (!EVFService.IsValidVehicleDoor(vehicleId, doorId)) return false;
            var v = BaseVehicle.Find(vehicleId);
            if (v == null) return false;
            v.GetWindowsParameters(out bool d, out bool p, out bool bl, out bool br);
            switch (doorId)
            {
                case DoorDriver: d = state; break;
                case DoorPassenger: p = state; break;
                case DoorBackLeft: bl = state; break;
                case DoorBackRight: br = state; break;
            }
            v.SetWindowsParameters(d, p, bl, br);
            return true;
        }

        public static bool GetCarDoorState(int vehicleId, int doorId)
        {
            var v = BaseVehicle.Find(vehicleId);
            if (v == null) return false;
            v.GetDoorsParameters(out bool d, out bool p, out bool bl, out bool br);
            return doorId switch { DoorDriver => d, DoorPassenger => p, DoorBackLeft => bl, DoorBackRight => br, _ => false };
        }

        public static bool SetCarDoorState(int vehicleId, int doorId, bool state)
        {
            if (!EVFService.IsValidVehicleDoor(vehicleId, doorId)) return false;
            var v = BaseVehicle.Find(vehicleId);
            if (v == null) return false;
            v.GetDoorsParameters(out bool d, out bool p, out bool bl, out bool br);
            switch (doorId)
            {
                case DoorDriver: d = state; break;
                case DoorPassenger: p = state; break;
                case DoorBackLeft: bl = state; break;
                case DoorBackRight: br = state; break;
            }
            v.SetDoorsParameters(d, p, bl, br);
            return true;
        }

        public static bool IsVehicleSeatOccupied(int vehicleId, int seatId)
            => EVFService.IsVehicleSeatOccupied(vehicleId, seatId);

        public static int GetVehicleNextSeat(int vehicleId, int startSeat = 1)
            => EVFService.GetVehicleNextSeat(vehicleId, startSeat);

        public static int GetVehiclePassenger(int vehicleId)
            => EVFService.GetVehiclePassenger(vehicleId);

        public static void ImportVehicle(int vehicleId, Vector3 pos, float angle,
            int color1, int color2, int worldId, int interiorId, bool unoccupiedDamage)
        {
            var v = BaseVehicle.Find(vehicleId);
            if (v == null) return;
            EVFService.RegisterVehicle(vehicleId, (int)v.Model, pos, angle, color1, color2, worldId, interiorId, unoccupiedDamage);
        }

        public static void HandleHorn(BasePlayer player)
        {
            if (player.State != PlayerState.Driving) return;
            int vId = player.Vehicle?.Id ?? -1;
            if (vId < 0) return;
            int horn = EVFService.GetHorn(vId);
            if (horn == 0) return;
            var pos = player.Position;
            foreach (var p in BasePlayer.All)
                if (p.Position.DistanceTo(pos) <= 35f)
                    p.PlaySound(horn, pos);
        }

        public static void SetVehicleNeonLights(int vehicleId, bool enable = true, int colorModel = 18647, int slotId = 0)
            => EVFService.SetVehicleNeonLights(vehicleId, enable, colorModel, slotId);

        public static bool GetVehicleNeonLightsState(int vehicleId, int slotId = 0)
            => EVFService.GetVehicleNeonLightsState(vehicleId, slotId);

        public static bool IsCarBlinking(int vehicleId)
            => EVFService.IsCarBlinking(vehicleId);

        public static void SetCarBlinking(int vehicleId, EVFBlinkSide side, bool skip = false)
            => EVFService.SetCarBlinking(vehicleId, side, skip);

        public static int DisableCarBlinking(int vehicleId)
            => EVFService.DisableCarBlinking(vehicleId);

        public static void ToggleVehicleBlinking(bool toggle)
            => EVFService.VehicleBlinking = toggle;

        public static bool IsToggledVehicleBlinking()
            => EVFService.VehicleBlinking;
    }
}