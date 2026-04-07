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
                // Cars (0)
                (400, "Landstalker", 0),    (401, "Bravura", 0),          (402, "Buffalo", 0),
                (404, "Perennial", 0),      (405, "Sentinel", 0),          (409, "Stretch", 0),
                (410, "Manana", 0),         (411, "Infernus", 0),          (412, "Voodoo", 0),
                (415, "Cheetah", 0),        (418, "Moonbeam", 0),          (419, "Esperanto", 0),
                (421, "Washington", 0),     (423, "Mr. Whoopee", 0),       (424, "BF Injection", 0),
                (426, "Premier", 0),        (428, "Securicar", 0),         (429, "Banshee", 0),
                (434, "Hotknife", 0),       (436, "Previon", 0),           (439, "Stallion", 0),
                (442, "Romero", 0),         (444, "Monster", 0),           (445, "Admiral", 0),
                (451, "Turismo", 0),        (457, "Caddy", 0),             (458, "Solair", 0),
                (466, "Glendale", 0),       (467, "Oceanic", 0),           (470, "Patriot", 0),
                (474, "Hermes", 0),         (475, "Sabre", 0),             (477, "ZR-350", 0),
                (479, "Regina", 0),         (480, "Comet", 0),             (483, "Camper", 0),
                (489, "Rancher", 0),        (491, "Virgo", 0),             (492, "Greenwood", 0),
                (494, "Hotring Racer", 0),  (495, "Sandking", 0),          (496, "Blista Compact", 0),
                (500, "Mesa", 0),           (502, "Hotring Racer A", 0),   (503, "Hotring Racer B", 0),
                (504, "Bloodring Banger", 0),(505, "Rancher Lure", 0),     (506, "Super GT", 0),
                (507, "Elegant", 0),        (508, "Journey", 0),           (516, "Nebula", 0),
                (517, "Majestic", 0),       (518, "Buccaneer", 0),         (526, "Fortune", 0),
                (527, "Cadrona", 0),        (529, "Willard", 0),           (533, "Feltzer", 0),
                (534, "Remington", 0),      (535, "Slamvan", 0),           (536, "Blade", 0),
                (539, "Vortex", 0),         (540, "Vincent", 0),           (541, "Bullet", 0),
                (542, "Clover", 0),         (545, "Hustler", 0),           (546, "Intruder", 0),
                (547, "Primo", 0),          (549, "Tampa", 0),             (550, "Sunrise", 0),
                (551, "Merit", 0),          (555, "Windsor", 0),           (556, "Monster A", 0),
                (557, "Monster B", 0),      (558, "Uranus", 0),            (559, "Jester", 0),
                (560, "Sultan", 0),         (561, "Stratum", 0),           (562, "Elegy", 0),
                (565, "Flash", 0),          (566, "Tahoma", 0),            (567, "Savanna", 0),
                (568, "Bandito", 0),        (571, "Kart", 0),              (573, "Dune", 0),
                (575, "Broadway", 0),       (576, "Tornado", 0),           (579, "Huntley", 0),
                (580, "Stafford", 0),       (585, "Emperor", 0),           (587, "Euros", 0),
                (588, "Hotdog", 0),         (589, "Club", 0),              (602, "Alpha", 0),
                (603, "Phoenix", 0),        (604, "Glendale Shit", 0),

                // Bikes (1)
                (448, "Pizzaboy", 1),       (461, "PCJ-600", 1),           (462, "Faggio", 1),
                (463, "Freeway", 1),        (468, "Sanchez", 1),           (471, "Quad", 1),
                (481, "BMX", 1),            (509, "Bike", 1),              (510, "Mountain Bike", 1),
                (521, "FCR-900", 1),        (522, "NRG-500", 1),           (581, "BF-400", 1),
                (586, "Wayfarer", 1),

                // Aircraft (2)
                (417, "Leviathan", 2),      (425, "Hunter", 2),            (447, "Seasparrow", 2),
                (460, "Skimmer", 2),        (469, "Sparrow", 2),           (476, "Rustler", 2),
                (487, "Maverick", 2),       (488, "SAN News Maverick", 2), (497, "Police Maverick", 2),
                (511, "Beagle", 2),         (512, "Cropduster", 2),        (513, "Stuntplane", 2),
                (519, "Shamal", 2),         (520, "Hydra", 2),             (548, "Cargobob", 2),
                (553, "Nevada", 2),         (563, "Raindance", 2),         (577, "AT400", 2),
                (592, "Andromada", 2),      (593, "Dodo", 2),

                // Boats (3)
                (430, "Predator", 3),       (446, "Squallo", 3),           (452, "Speeder", 3),
                (453, "Reefer", 3),         (454, "Tropic", 3),            (472, "Coastguard", 3),
                (473, "Dinghy", 3),         (484, "Marquis", 3),           (493, "Jetmax", 3),
                (595, "Launch", 3),

                // Heavy / Industrial (4)
                (403, "Linerunner", 4),     (406, "Dumper", 4),            (408, "Trashmaster", 4),
                (413, "Pony", 4),           (414, "Mule", 4),              (422, "Bobcat", 4),
                (435, "Article Trailer", 4),(440, "Rumpo", 4),             (441, "RC Bandit", 4),
                (443, "Packer", 4),         (450, "Article Trailer 2", 4), (455, "Flatbed", 4),
                (456, "Yankee", 4),         (459, "Topfun Van", 4),        (464, "RC Baron", 4),
                (465, "RC Raider", 4),      (478, "Walton", 4),            (482, "Burrito", 4),
                (485, "Baggage", 4),        (486, "Dozer", 4),             (498, "Boxville", 4),
                (499, "Benson", 4),         (501, "RC Goblin", 4),         (514, "Tanker", 4),
                (515, "Roadtrain", 4),      (524, "Cement Truck", 4),      (525, "Towtruck", 4),
                (530, "Forklift", 4),       (531, "Tractor", 4),           (532, "Combine Harvester", 4),
                (543, "Sadler", 4),         (552, "Utility Van", 4),       (554, "Yosemite", 4),
                (564, "RC Tiger", 4),       (569, "Freight Flat Trailer", 4),(570, "Streak Trailer", 4),
                (572, "Mower", 4),          (574, "Sweeper", 4),           (578, "DFT-30", 4),
                (582, "Newsvan", 4),        (583, "Tug", 4),               (584, "Petrol Trailer", 4),
                (590, "Freight Box Trailer", 4),(591, "Article Trailer 3", 4),(594, "RC Cam", 4),
                (600, "Picador", 4),        (605, "Sadler Shit", 4),       (606, "Baggage Trailer A", 4),
                (607, "Baggage Trailer B", 4),(608, "Tug Stairs Trailer", 4),(609, "Boxville 2", 4),
                (610, "Farm Trailer", 4),   (611, "Utility Trailer", 4),

                // Public Service (5)
                (407, "Firetruck", 5),      (416, "Ambulance", 5),         (420, "Taxi", 5),
                (427, "Enforcer", 5),       (431, "Bus", 5),               (432, "Rhino", 5),
                (433, "Barracks", 5),       (437, "Coach", 5),             (438, "Cabbie", 5),
                (449, "Tram", 5),           (490, "FBI Rancher", 5),       (497, "Police Maverick", 5),
                (523, "HPV1000", 5),        (528, "FBI Truck", 5),         (537, "Freight Train", 5),
                (538, "Brownstreak Train", 5),(544, "Firetruck LA", 5),    (596, "Police Car LSPD", 5),
                (597, "Police Car SFPD", 5),(598, "Police Car LVPD", 5),  (599, "Police Ranger", 5),
                (601, "S.W.A.T.", 5),
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