using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Player;

public class GodModeCheck
{
    private const float MinExpectedDecrease = 0.5f;
    private const long DamageWindowMs = 800;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public GodModeCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerTakeDamage(BasePlayer player, DamageEventArgs e)
    {
        var st = _players.Get(player.Id);
        if (st is null || !_config.Enabled) return;
        st.PendingDamageResult = true;
        st.PendingVehicleDamageResult = player.State == PlayerState.Driving;
        st.DamageTick = Environment.TickCount64;
    }

    public void OnPlayerUpdate(BasePlayer player)
    {
        var st = _players.Get(player.Id);
        if (st is null || !_config.Enabled || !st.PendingDamageResult) return;
        if (!_config.GetCheck("GodModeOnfoot").Enabled && !_config.GetCheck("GodModeVehicle").Enabled) {
            st.PendingDamageResult = false;
            return;
        }

        if (st.IsDead) { st.PendingDamageResult = false; return; }

        long elapsed = Environment.TickCount64 - st.DamageTick;
        if (elapsed < 100) return;
        if (elapsed > DamageWindowMs) { st.PendingDamageResult = false; return; }

        string name = st.PendingVehicleDamageResult ? "GodModeVehicle" : "GodModeOnfoot";
        if (!_config.GetCheck(name).Enabled) { st.PendingDamageResult = false; return; }

        if (player.Health >= st.Health - MinExpectedDecrease && player.Health > 0)
            _warnings.AddWarning(player.Id, name, $"hp={player.Health:F1} prev={st.Health:F1}");

        st.PendingDamageResult = false;
    }

    public void OnPlayerDied(BasePlayer player)
    {
        var st = _players.Get(player.Id);
        if (st is not null) st.PendingDamageResult = false;
    }
}