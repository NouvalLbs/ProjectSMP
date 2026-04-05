using ProjectSMP.Entities.Players.Administrator.Data;
using System.Collections.Generic;
using System.Linq;

namespace ProjectSMP.Entities.Players.Administrator
{
    public static class VehicleService
    {
        private static readonly Dictionary<int, VehicleModelInfo> Vehicles = new();

        static VehicleService()
        {
            InitializeVehicles();
        }

        private static void InitializeVehicles()
        {
            var models = new[]
            {
                (400, "Landstalker", 0), (401, "Bravura", 0), (402, "Buffalo", 0), (411, "Infernus", 0),
                (415, "Cheetah", 0), (429, "Banshee", 0), (451, "Turismo", 0), (477, "ZR-350", 0),
                (506, "Super GT", 0), (541, "Bullet", 0), (559, "Jester", 0), (560, "Sultan", 0),
                (562, "Elegy", 0), (461, "PCJ-600", 1), (462, "Faggio", 1), (463, "Freeway", 1),
                (468, "Sanchez", 1), (471, "Quad", 1), (481, "BMX", 1), (509, "Bike", 1),
                (510, "Mountain Bike", 1), (521, "FCR-900", 1), (522, "NRG-500", 1), (586, "Wayfarer", 1),
                (417, "Leviathan", 2), (425, "Hunter", 2), (447, "Seasparrow", 2), (460, "Skimmer", 2),
                (469, "Sparrow", 2), (487, "Maverick", 2), (511, "Beagle", 2), (512, "Cropduster", 2),
                (513, "Stuntplane", 2), (519, "Shamal", 2), (520, "Hydra", 2), (430, "Predator", 3),
                (446, "Squalo", 3), (452, "Speeder", 3), (453, "Reefer", 3), (454, "Tropic", 3),
                (484, "Marquis", 3), (493, "Jetmax", 3), (595, "Launch", 3), (403, "Linerunner", 4),
                (414, "Mule", 4), (443, "Packer", 4), (515, "Roadtrain", 4), (524, "Cement Truck", 4),
                (531, "Tractor", 4), (408, "Trashmaster", 4), (420, "Taxi", 5), (431, "Bus", 5),
                (437, "Coach", 5), (438, "Cabbie", 5)
            };

            foreach (var (model, name, category) in models)
            {
                Vehicles[model] = new VehicleModelInfo { ModelId = model, Name = name, Category = category };
            }
        }

        public static bool IsValidModel(int modelId)
        {
            return modelId >= 400 && modelId <= 611 && Vehicles.ContainsKey(modelId);
        }

        public static string GetVehicleName(int modelId)
        {
            return Vehicles.TryGetValue(modelId, out var info) ? info.Name : "Unknown";
        }

        public static List<VehicleModelInfo> GetByCategory(int category)
        {
            if (category == -1)
                return Vehicles.Values.ToList();

            return Vehicles.Values.Where(v => v.Category == category).ToList();
        }
        public static List<VehicleModelInfo> Search(string query)
        {
            return Vehicles.Values
                .Where(v => v.Name.Contains(query, System.StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}