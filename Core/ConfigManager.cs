using ProjectSMP.Plugins.SKY;
using SampSharp.GameMode.Definitions;
using System.IO;
using System.Text.Json;

namespace ProjectSMP.Core {
    public class GameConfig {
        public bool ManualVehicleEngineAndLights { get; set; } = true;
        public bool StuntBonusForAll { get; set; } = false;
        public bool InteriorWeapons { get; set; } = true;
        public bool InteriorEnterExits { get; set; } = false;

        public float NameTagDrawDistance { get; set; } = 8.0f;
        public float PlayerMarkerRadius { get; set; } = 1.0f;

        public bool ShowPlayerMarkers { get; set; } = false;
        public bool ShowNameTags { get; set; } = false;
        public bool NameTagLOS { get; set; } = false;

        public bool VehiclePassengerDamage { get; set; } = true;
        public bool DisableSyncBugs { get; set; } = true;
        public bool KnifeSync { get; set; } = true;

        public DatabaseConfig Database { get; set; } = new();
    }

    public class DatabaseConfig {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 3306;
        public string Name { get; set; } = "samp_db";
        public string User { get; set; } = "root";
        public string Password { get; set; } = "";
    }

    internal class ConfigManager {
        private const string Path = "scriptfiles/GameConfig.json";

        public static GameConfig Game { get; private set; } = new();

        public static void Load() {
            if (!File.Exists(Path)) {
                var json = JsonSerializer.Serialize(Game, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(Path, json);
                return;
            }

            Game = JsonSerializer.Deserialize<GameConfig>(File.ReadAllText(Path)) ?? new GameConfig();
        }

        public static void ApplyGameConfig(GameMode gameMode) {
            var cfg = Game;

            if (cfg.ManualVehicleEngineAndLights) gameMode.ManualVehicleEngineAndLights();

            gameMode.EnableStuntBonusForAll(cfg.StuntBonusForAll);
            gameMode.AllowInteriorWeapons(cfg.InteriorWeapons);

            if (!cfg.InteriorEnterExits) gameMode.DisableInteriorEnterExits();

            gameMode.SetNameTagDrawDistance(cfg.NameTagDrawDistance);
            gameMode.LimitPlayerMarkerRadius(cfg.PlayerMarkerRadius);

            gameMode.ShowPlayerMarkers(cfg.ShowPlayerMarkers
                ? PlayerMarkersMode.Global
                : PlayerMarkersMode.Off);

            gameMode.ShowNameTags(cfg.ShowNameTags);

            if (!cfg.NameTagLOS) gameMode.DisableNameTagLOS();

            try {
                SkyNatives.Instance.SetDisableSyncBugs(cfg.DisableSyncBugs ? 1 : 0);
                SkyNatives.Instance.SetKnifeSync(cfg.KnifeSync ? 1 : 0);
            }
            catch { }
        }
    }
}