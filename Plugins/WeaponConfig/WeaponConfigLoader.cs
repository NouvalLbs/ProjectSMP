using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProjectSMP.Plugins.WeaponConfig
{
    internal static class WeaponConfigLoader
    {
        private const string FilePath = "scriptfiles/Weapons.json";
        private static readonly JsonSerializerOptions Opts = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public static (WeaponConfig cfg, WeaponEntry[] weapons) Load()
        {
            var def = BuildDefaultWeapons();

            if (!File.Exists(FilePath))
            {
                var root = new WeaponConfigRoot { Config = new WeaponConfig(), Weapons = new List<WeaponEntry>(def) };
                File.WriteAllText(FilePath, JsonSerializer.Serialize(root, Opts));
                return (root.Config, def);
            }

            try
            {
                var parsed = JsonSerializer.Deserialize<WeaponConfigRoot>(File.ReadAllText(FilePath), Opts) ?? new WeaponConfigRoot();
                var weapons = BuildDefaultWeapons();
                foreach (var w in parsed.Weapons)
                    if (w.Id >= 0 && w.Id < weapons.Length)
                    {
                        weapons[w.Id] = w;
                        if (string.IsNullOrEmpty(w.Name))
                            w.Name = weapons[w.Id].Name;
                    }
                return (parsed.Config ?? new WeaponConfig(), weapons);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WeaponConfig] Load error: {ex.Message}");
                return (new WeaponConfig(), def);
            }
        }

        private static WeaponEntry[] BuildDefaultWeapons()
        {
            var d = new (string name, float dmg, DamageType t, float r, int rate, bool arm, bool torso)[]
            {
                ("Fist",                1.0f,   DamageType.Multiplier, 1.76f, 250,  false, false),
                ("Brass Knuckles",      1.0f,   DamageType.Multiplier, 1.76f, 250,  false, false),
                ("Golf Club",           1.0f,   DamageType.Multiplier, 1.76f, 250,  false, false),
                ("Nightstick",          1.0f,   DamageType.Multiplier, 1.76f, 250,  false, false),
                ("Knife",               1.0f,   DamageType.Multiplier, 1.76f, 250,  false, false),
                ("Bat",                 1.0f,   DamageType.Multiplier, 1.76f, 250,  false, false),
                ("Shovel",              1.0f,   DamageType.Multiplier, 1.60f, 250,  false, false),
                ("Pool Cue",            1.0f,   DamageType.Multiplier, 1.76f, 250,  false, false),
                ("Katana",              1.0f,   DamageType.Multiplier, 1.76f, 250,  false, false),
                ("Chainsaw",            1.0f,   DamageType.Multiplier, 1.76f,  30,  false, false),
                ("Purple Dildo",        1.0f,   DamageType.Multiplier, 1.76f, 250,  false, false),
                ("Dildo",               1.0f,   DamageType.Multiplier, 1.76f, 250,  false, false),
                ("Vibrator",            1.0f,   DamageType.Multiplier, 1.76f, 250,  false, false),
                ("Vibrator 2",          1.0f,   DamageType.Multiplier, 1.76f, 250,  false, false),
                ("Flowers",             1.0f,   DamageType.Multiplier, 1.76f, 250,  false, false),
                ("Cane",                1.0f,   DamageType.Multiplier, 1.76f, 250,  false, false),
                ("Grenade",             82.5f,  DamageType.Multiplier,  40f,    0,  false, false),
                ("Teargas",             0.0f,   DamageType.Static,      40f,    0,  false, false),
                ("Molotov",             1.0f,   DamageType.Multiplier,  40f,    0,  false, false),
                ("Vehicle M4",          9.9f,   DamageType.Static,      90f,   90,  true,  true ),
                ("Vehicle Minigun",     46.2f,  DamageType.Static,      75f,   20,  true,  true ),
                ("Vehicle Rocket",      82.5f,  DamageType.Multiplier,   0f,    0,  false, false),
                ("Colt 45",             8.25f,  DamageType.Static,      35f,  160,  true,  true ),
                ("Silenced Pistol",     13.2f,  DamageType.Static,      35f,  120,  true,  true ),
                ("Deagle",              46.2f,  DamageType.Static,      35f,  120,  true,  true ),
                ("Shotgun",             3.3f,   DamageType.Static,      40f,  800,  true,  true ),
                ("Sawn-off Shotgun",    3.3f,   DamageType.Static,      35f,  120,  true,  true ),
                ("Combat Shotgun",      4.95f,  DamageType.Static,      40f,  120,  true,  true ),
                ("Mac-10",              6.6f,   DamageType.Static,      35f,   50,  true,  true ),
                ("MP5",                 8.25f,  DamageType.Static,      45f,   90,  true,  true ),
                ("AK-47",               9.9f,   DamageType.Static,      70f,   90,  true,  true ),
                ("M4",                  9.9f,   DamageType.Static,      90f,   90,  true,  true ),
                ("Tec-9",               6.6f,   DamageType.Static,      35f,   50,  true,  true ),
                ("Cuntgun",             24.75f, DamageType.Static,     100f,  800,  true,  true ),
                ("Sniper",              41.25f, DamageType.Static,     320f,  800,  true,  true ),
                ("Rocket Launcher",     82.5f,  DamageType.Multiplier,  55f,    0,  false, false),
                ("Heat Seeker",         82.5f,  DamageType.Multiplier,  55f,    0,  false, false),
                ("Flamethrower",        1.0f,   DamageType.Multiplier,  5.1f,   0,  false, false),
                ("Minigun",             46.2f,  DamageType.Static,      75f,   20,  true,  true ),
                ("Satchel",             82.5f,  DamageType.Multiplier,  40f,    0,  false, false),
                ("Detonator",           0.0f,   DamageType.Multiplier,  25f,    0,  false, false),
                ("Spraycan",            0.33f,  DamageType.Static,      6.1f,  10,  false, false),
                ("Fire Extinguisher",   0.33f,  DamageType.Static,     10.1f,  10,  false, false),
                ("Camera",              0.0f,   DamageType.Multiplier, 100f,    0,  true,  false),
                ("NV Goggles",          0.0f,   DamageType.Multiplier, 100f,    0,  true,  false),
                ("IR Goggles",          0.0f,   DamageType.Multiplier, 100f,    0,  true,  false),
                ("Parachute",           0.0f,   DamageType.Multiplier, 1.76f,   0,  true,  false),
                ("Fake Pistol",         0.0f,   DamageType.Multiplier,  0f,     0,  true,  false),
                ("Pistol Whip",         2.64f,  DamageType.Static,      0f,   400,  false, false),
                ("Vehicle",             9.9f,   DamageType.Static,      0f,     0,  false, false),
                ("Helicopter Blades",   330f,   DamageType.Static,      0f,     0,  false, false),
                ("Explosion",           82.5f,  DamageType.Multiplier,  0f,     0,  false, false),
                ("Car Parking",         1.0f,   DamageType.Multiplier,  0f,     0,  false, false),
                ("Drowning",            1.0f,   DamageType.Multiplier,  0f,     0,  false, false),
                ("Splat",               5.0f,   DamageType.Multiplier,   0f,     0,  false, false),
            };

            var arr = new WeaponEntry[d.Length];
            for (var i = 0; i < d.Length; i++)
            {
                var (name, dmg, t, r, rate, arm, torso) = d[i];
                arr[i] = new WeaponEntry
                {
                    Id = i,
                    Name = name,
                    Damage = dmg,
                    Type = t,
                    Range = r,
                    MaxShootRate = rate,
                    AffectsArmour = arm,
                    TorsoOnly = torso
                };
            }
            return arr;
        }
    }
}