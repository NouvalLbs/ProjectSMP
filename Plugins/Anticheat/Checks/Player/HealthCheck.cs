using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Data;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Player;

public class HealthCheck
{
    private const float AllowedGain = 0.5f;
    private const float MaxHealth = 100f;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;
    private readonly AnticheatPlugin _plugin;

    public HealthCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c, AnticheatPlugin plugin) {
        _players = p;
        _warnings = w;
        _config = c;
        _plugin = plugin;
    }

    public void OnPlayerUpdate(BasePlayer player)
    {
        var st = _players.Get(player.Id);
        if (st is null || st.IsDead || !_config.Enabled) return;
        if (!_config.GetCheck("HealthHackOnfoot").Enabled && !_config.GetCheck("HealthHackVehicle").Enabled) return;

        long now = Environment.TickCount64;
        float hp = player.Health;

        if (now - st.SetHealthTick < 1500) { st.Health = hp; return; }
        if (now - st.SpawnTick < 2500) { st.Health = hp; return; }

        float gain = hp - st.Health;
        if (gain <= AllowedGain) { st.Health = hp; return; }

        var pos = player.Position;
        if (VehicleData.IsInPayNSpray(pos.X, pos.Y, pos.Z)) { st.Health = hp; return; }
        if (VehicleData.IsNearCasino(pos.X, pos.Y, pos.Z)) { st.Health = hp; return; }
        if (TuningData.IsNearVendingMachine(pos.X, pos.Y, pos.Z)) { st.Health = hp; return; }
        if (VehicleData.IsNearRestaurant(pos.X, pos.Y, pos.Z)) { st.Health = hp; return; }
        if (VehicleData.IsNearHospital(pos.X, pos.Y, pos.Z)) { st.Health = hp; return; }
        if (st.StuntBonusEnabled) { st.Health = hp; return; }

        string name = player.State == PlayerState.Driving ? "HealthHackVehicle" : "HealthHackOnfoot";
        if (!_config.GetCheck(name).Enabled) { st.Health = hp; return; }

        if (hp > MaxHealth + 0.5f)
            _warnings.AddWarning(player.Id, name, $"hp={hp:F1} (>100)");
        else
            _warnings.AddWarning(player.Id, name, $"gain={gain:F1} hp={hp:F1}");

        st.Health = hp;
    }

    public void OnPlayerSpawned(BasePlayer player)
    {
        var st = _players.Get(player.Id);
        if (st is null) return;
        st.Health = player.Health;
        st.SetHealth = -1;
    }
}