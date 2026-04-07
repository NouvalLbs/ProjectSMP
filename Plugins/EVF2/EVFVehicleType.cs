using SampSharp.GameMode.World;

namespace ProjectSMP.Plugins.EVF2
{
    public static class EVFVehicleType
    {
        // Motorcycles
        private static readonly int[] Bikes = { 461, 462, 463, 468, 471, 521, 522, 523, 581, 586 };
        // Bicycles
        private static readonly int[] Bicycles = { 481, 509, 510 };
        // Boats
        private static readonly int[] Boats = { 430, 446, 452, 453, 454, 472, 473, 484, 493, 595 };
        // Fixed-wing aircraft
        private static readonly int[] Planes = { 460, 476, 511, 512, 513, 519, 520, 553, 577, 592, 593 };
        // Helicopters
        private static readonly int[] Helis = { 417, 425, 447, 469, 487, 488, 497, 548, 563 };
        // Trains & trams
        private static readonly int[] Trains = { 449, 537, 538, 569, 570, 590 };
        // RC vehicles
        private static readonly int[] RCVehicles = { 441, 464, 465, 501, 564, 594 };

        private static bool Contains(int[] arr, int val)
        {
            foreach (var v in arr) if (v == val) return true;
            return false;
        }

        public static bool IsABike(int modelId) => Contains(Bikes, modelId);
        public static bool IsABicycle(int modelId) => Contains(Bicycles, modelId);
        public static bool IsABoat(int modelId) => Contains(Boats, modelId);
        public static bool IsAPlane(int modelId) => Contains(Planes, modelId);
        public static bool IsAHeli(int modelId) => Contains(Helis, modelId);
        public static bool IsATrain(int modelId) => Contains(Trains, modelId);
        public static bool IsARCVehicle(int modelId) => Contains(RCVehicles, modelId);

        public static bool IsACar(int modelId)
            => EVFService.IsValidVehicleModelId(modelId)
            && !IsABike(modelId) && !IsABicycle(modelId) && !IsABoat(modelId)
            && !IsAPlane(modelId) && !IsAHeli(modelId) && !IsATrain(modelId)
            && !IsARCVehicle(modelId);

        public static bool IsAirborne(int modelId) => IsAPlane(modelId) || IsAHeli(modelId);
        public static bool IsWater(int modelId) => IsABoat(modelId);
        public static bool IsGroundVehicle(int modelId) => IsACar(modelId) || IsABike(modelId) || IsABicycle(modelId);

        // Overloads by vehicleId
        public static bool IsABike(BaseVehicle v) => v != null && IsABike((int)v.Model);
        public static bool IsABicycle(BaseVehicle v) => v != null && IsABicycle((int)v.Model);
        public static bool IsABoat(BaseVehicle v) => v != null && IsABoat((int)v.Model);
        public static bool IsAPlane(BaseVehicle v) => v != null && IsAPlane((int)v.Model);
        public static bool IsAHeli(BaseVehicle v) => v != null && IsAHeli((int)v.Model);
        public static bool IsATrain(BaseVehicle v) => v != null && IsATrain((int)v.Model);
        public static bool IsARCVehicle(BaseVehicle v) => v != null && IsARCVehicle((int)v.Model);
        public static bool IsACar(BaseVehicle v) => v != null && IsACar((int)v.Model);
        public static bool IsAirborne(BaseVehicle v) => v != null && IsAirborne((int)v.Model);
        public static bool IsGroundVehicle(BaseVehicle v) => v != null && IsGroundVehicle((int)v.Model);
    }
}