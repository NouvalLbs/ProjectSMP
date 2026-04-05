using ProjectSMP.Entities.Players.Needs;
using ProjectSMP.Plugins.CEF;
using ProjectSMP.Plugins.EVF2;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Display;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Entities.Vehicles.Speedo
{
    public static class SpeedometerService
    {
        private const int TdCount = 18;
        private static readonly Dictionary<int, PlayerTextDraw[]> _speedos = new();
        private static Timer _timer;

        public static void Initialize()
        {
            _timer = new Timer(100, true);
            _timer.Tick += OnTick;
        }

        public static void Dispose()
        {
            _timer?.Dispose();
            _speedos.Clear();
        }

        public static void OnPlayerStateChanged(Player player, PlayerState newState, PlayerState oldState)
        {
            if (newState == PlayerState.Driving)
            {
                if (player.Settings.HBEMode == 1)
                    Show(player);
                else
                    CefService.EmitEvent(player.Id, "setInVehicle", new { value = true });
            }
            else if (oldState == PlayerState.Driving)
            {
                if (player.Settings.HBEMode == 1)
                    Hide(player);
                else
                    CefService.EmitEvent(player.Id, "setInVehicle", new { value = false });
            }
        }

        public static void OnPlayerDisconnect(Player player)
        {
            Hide(player);
            if (player.Settings.HBEMode == 0 && player.State == PlayerState.Driving)
                CefService.EmitEvent(player.Id, "setInVehicle", new { value = false });
        }

        public static void Regenerate(Player player)
        {
            if (!_speedos.ContainsKey(player.Id)) return;
            Hide(player);
            Show(player);
        }

        private static void Show(Player player)
        {
            if (player.Settings.HBEMode != 1) return;

            Hide(player);

            var (count, coords) = NeedsHudManager.GetActiveHudInfo(player);
            var posXRight = count > 0 ? coords[0] - 35f : 320f - 35f;
            var posXLeft = count > 0 ? coords[count - 1] + 35f : 320f + 35f;

            var tds = new PlayerTextDraw[TdCount];
            GenerateRight(player, tds, posXRight, 424f);
            GenerateLeft(player, tds, posXLeft, 429f);
            _speedos[player.Id] = tds;

            var veh = player.Vehicle;
            if (veh != null)
                tds[13].Text = GetVehicleName(veh);

            foreach (var td in tds)
                td?.Show();
        }

        private static void Hide(Player player)
        {
            if (!_speedos.TryGetValue(player.Id, out var tds)) return;
            foreach (var td in tds) td?.Dispose();
            _speedos.Remove(player.Id);
        }

        private static void OnTick(object sender, EventArgs e)
        {
            foreach (var kvp in new Dictionary<int, PlayerTextDraw[]>(_speedos))
            {
                if (BasePlayer.Find(kvp.Key) is not Player p || p.IsDisposed || p.State != PlayerState.Driving) continue;
                var veh = p.Vehicle;
                if (veh == null) continue;

                var tds = kvp.Value;
                var vel = veh.Velocity;
                var raw = Math.Sqrt(vel.X * vel.X + vel.Y * vel.Y + vel.Z * vel.Z);
                var kmh = (int)(raw * 180);

                tds[9].Text = kmh.ToString();

                tds[7].Text = raw < 0.001 ? "N"
                    : kmh <= 20 ? "1"
                    : kmh <= 40 ? "2"
                    : kmh <= 60 ? "3"
                    : kmh <= 80 ? "4"
                    : "5";

                var angle = veh.Angle;
                tds[12].Text = (angle >= 337.5f || angle < 22.5f) ? "N"
                    : angle < 67.5f ? "NE"
                    : angle < 112.5f ? "E"
                    : angle < 157.5f ? "SE"
                    : angle < 202.5f ? "S"
                    : angle < 247.5f ? "SW"
                    : angle < 292.5f ? "W"
                    : "NW";
            }

            foreach (var bp in BasePlayer.All)
            {
                if (bp is not Player p || p.IsDisposed || p.State != PlayerState.Driving) continue;
                if (p.Settings.HBEMode != 0) continue;

                var veh = p.Vehicle;
                if (veh == null) continue;

                var vel = veh.Velocity;
                var raw = Math.Sqrt(vel.X * vel.X + vel.Y * vel.Y + vel.Z * vel.Z);
                var kmh = (int)(raw * 180);

                var gear = raw < 0.001 ? "N"
                    : kmh <= 20 ? "1"
                    : kmh <= 40 ? "2"
                    : kmh <= 60 ? "3"
                    : kmh <= 80 ? "4"
                    : "5";

                var angle = veh.Angle;
                var heading = (angle >= 337.5f || angle < 22.5f) ? "N"
                    : angle < 67.5f ? "NE"
                    : angle < 112.5f ? "E"
                    : angle < 157.5f ? "SE"
                    : angle < 202.5f ? "S"
                    : angle < 247.5f ? "SW"
                    : angle < 292.5f ? "W"
                    : "NW";

                // FIX: Gunakan EVF fuel system
                int evfFuel = EVFService.GetFuel(veh.Id);
                int fuelPct = EVFService.IsFuelEnabled(veh.Id)
                    ? (int)((evfFuel / (float)EVFConstants.MaxVehicleFuel) * 100)
                    : 100;

                CefService.EmitEvent(p.Id, "updateSpeedo", new
                {
                    VehicleSpeed = kmh,
                    VehicleGear = gear,
                    VehicleHeading = heading,
                    VehicleName = GetVehicleName(veh),
                    VehicleFuel = fuelPct
                });
            }
        }

        private static void GenerateRight(Player p, PlayerTextDraw[] tds, float x, float y)
        {
            tds[0] = TD(p, x, y, "_", TextDrawFont.Normal, 0.6f, 2.2f, 400f, 40.5f, TextDrawAlignment.Center, -1, 255, 100, true, 1);
            tds[1] = TD(p, x - 30f, y, "_", TextDrawFont.Normal, 0.6f, 2.2f, 400f, 12.5f, TextDrawAlignment.Center, -1, 255, -1006681089, true, 1);
            tds[2] = TD(p, x - 34f, y + 12f, "_", TextDrawFont.Normal, 0.6f, 0.250001f, 400f, -2f, TextDrawAlignment.Center, -1, 255, 255, true, 1);
            tds[3] = TD(p, x - 27f, y + 14f, "_", TextDrawFont.Normal, 0.6f, 0.250001f, 400f, -2f, TextDrawAlignment.Center, -1, 255, 255, true, 1);
            tds[4] = TD(p, x - 29f, y + 14f, "_", TextDrawFont.Normal, 0.6f, 0.250001f, 400f, -2f, TextDrawAlignment.Center, -1, 255, 255, true, 1);
            tds[5] = TD(p, x - 31f, y + 14f, "_", TextDrawFont.Normal, 0.6f, 0.250001f, 400f, -2f, TextDrawAlignment.Center, -1, 255, 255, true, 1);
            tds[6] = TD(p, x - 31f, y + 16f, "_", TextDrawFont.Normal, 0.6f, -0.249998f, 400f, 4.5f, TextDrawAlignment.Center, -1, 255, 255, true, 1);
            tds[7] = TD(p, x - 30f, y - 1f, "N", TextDrawFont.Slim, 0.187497f, 1.399997f, 400f, 17f, TextDrawAlignment.Center, 255, 255, 50, false, 0);
            tds[8] = TD(p, x + 15f, y - 2f, "KMH", TextDrawFont.Slim, 0.141664f, 0.599997f, 400f, 17f, TextDrawAlignment.Center, -1, 255, 50, false, 0);
            tds[9] = TD(p, x - 1f, y - 2f, "0", TextDrawFont.Slim, 0.345830f, 2.599996f, 400f, 17f, TextDrawAlignment.Center, -1, 255, 50, false, 0);
        }

        private static void GenerateLeft(Player p, PlayerTextDraw[] tds, float x, float y)
        {
            tds[10] = TD(p, x, y, "_", TextDrawFont.Normal, 0.6f, 1.6f, 400f, 40.5f, TextDrawAlignment.Center, -1, 255, 100, true, 1);
            tds[11] = TD(p, x + 30f, y, "_", TextDrawFont.Normal, 0.6f, 1.55f, 400f, 12.5f, TextDrawAlignment.Center, -1, 255, -2686721, true, 1);
            tds[12] = TD(p, x + 30f, y + 2f, "N", TextDrawFont.Normal, 0.149997f, 1.149997f, 400f, 17f, TextDrawAlignment.Center, 255, 255, 50, false, 0);
            tds[13] = TD(p, x, y, "UNKNOWN", TextDrawFont.Slim, 0.154164f, 1.399997f, 400f, 17f, TextDrawAlignment.Center, -1, 255, 50, false, 0);
            tds[14] = new PlayerTextDraw(p, new Vector2(x - 22f, y - 10f), "HUD:radar_centre")
            {
                Font = TextDrawFont.DrawSprite,
                LetterSize = new Vector2(0.6f, 2.0f),
                Width = 6.5f,
                Height = 7f,
                Alignment = TextDrawAlignment.Center,
                ForeColor = new Color(-764862721),
                BackColor = new Color(255),
                BoxColor = new Color(50),
                UseBox = true,
                Outline = 1,
                Shadow = 0,
                Proportional = true,
                Selectable = false
            };
            tds[15] = TD(p, x - 13f, y - 6f, "_", TextDrawFont.Normal, 0.6f, 0.05f, 407f, 17f, TextDrawAlignment.Left, -1, 255, -2023876609, true, 1);
            tds[16] = TD(p, x - 13f, y - 6f, "_", TextDrawFont.Normal, 0.6f, 0.05f, 407f, 17f, TextDrawAlignment.Left, -1, 255, -764862721, true, 1);
            tds[17] = TD(p, x + 2f, y - 15f, "100%", TextDrawFont.Slim, 0.133331f, 0.699998f, 400f, 17f, TextDrawAlignment.Center, -1, 255, 50, false, 1);
        }

        private static PlayerTextDraw TD(Player p, float x, float y, string text,
            TextDrawFont font, float lx, float ly, float w, float h,
            TextDrawAlignment align, int color, int bg, int box, bool useBox, int outline)
        {
            return new PlayerTextDraw(p, new Vector2(x, y), text)
            {
                Font = font,
                LetterSize = new Vector2(lx, ly),
                Width = w,
                Height = h,
                Alignment = align,
                ForeColor = new Color(color),
                BackColor = new Color(bg),
                BoxColor = new Color(box),
                UseBox = useBox,
                Outline = outline,
                Shadow = 0,
                Proportional = true,
                Selectable = false
            };
        }

        private static string GetVehicleName(BaseVehicle vehicle)
        {
            var id = (int)vehicle.Model;
            return id >= 400 && id <= 611 ? VehicleNames[id - 400] : "Unknown";
        }

        private static readonly string[] VehicleNames =
        {
            "Landstalker","Bravura","Buffalo","Linerunner","Perennial","Sentinel",
            "Dumper","Firetruck","Trashmaster","Stretch","Manana","Infernus",
            "Voodoo","Pony","Mule","Cheetah","Ambulance","Leviathan","Moonbeam",
            "Esperanto","Taxi","Washington","Bobcat","Mr Whoopee","BF Injection",
            "Hunter","Premier","Enforcer","Securicar","Banshee","Predator","Bus",
            "Rhino","Barracks","Hotknife","Trailer","Previon","Coach","Cabbie",
            "Stallion","Rumpo","RC Bandit","Romero","Packer","Monster","Admiral",
            "Squalo","Seasparrow","Pizzaboy","Tram","Trailer","Turismo","Speeder",
            "Reefer","Tropic","Flatbed","Yankee","Caddy","Solair","Berkley's RC Van",
            "Skimmer","PCJ-600","Faggio","Freeway","RC Baron","RC Raider","Glendale",
            "Oceanic","Sanchez","Sparrow","Patriot","Quad","Coastguard","Dinghy",
            "Hermes","Sabre","Rustler","ZR-350","Walton","Regina","Comet","BMX",
            "Burrito","Camper","Marquis","Baggage","Dozer","Maverick","News Chopper",
            "Rancher","FBI Rancher","Virgo","Greenwood","Jetmax","Hotring","Sandking",
            "Blista Compact","Police Maverick","Boxville","Benson","Mesa","RC Goblin",
            "Hotring Racer","Hotring Racer","Bloodring Banger","Rancher","Super GT",
            "Elegant","Journey","Bike","Mountain Bike","Beagle","Cropdust","Stunt",
            "Tanker","RoadTrain","Nebula","Majestic","Buccaneer","Shamal","Hydra",
            "FCR-900","NRG-500","HPV1000","Cement Truck","Tow Truck","Fortune","Cadrona",
            "FBI Truck","Willard","Forklift","Tractor","Combine","Feltzer","Remington",
            "Slamvan","Blade","Freight","Streak","Vortex","Vincent","Bullet","Clover",
            "Sadler","Firetruck","Hustler","Intruder","Primo","Cargobob","Tampa",
            "Sunrise","Merit","Utility Truck","Nevada","Yosemite","Windsor","Monster",
            "Monster","Uranus","Jester","Sultan","Stratum","Elegy","Raindance","RC Tiger",
            "Flash","Tahoma","Savanna","Bandito","Freight","Trailer","Kart","Mower",
            "Dune","Sweeper","Broadway","Tornado","AT-400","DFT-30","Huntley",
            "Stafford","BF-400","Newsvan","Tug","Trailer","Emperor","Wayfarer","Euros",
            "Hotdog","Club","Trailer","Trailer","Andromada","Dodo","RC Cam","Launch",
            "Police Car (LSPD)","Police Car (SFPD)","Police Car (LVPD)","Police Ranger",
            "Picador","S.W.A.T. Van","Alpha","Phoenix","Glendale","Sadler",
            "Luggage Trailer","Luggage Trailer","Stair Trailer","Boxville","Farm Plow",
            "Utility Trailer"
        };
    }
}