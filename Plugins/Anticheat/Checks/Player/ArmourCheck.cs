using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Player;

/// <summary>Check 13</summary>
public class ArmourCheck
{
    private const float AllowedGain = 0.5f;
    private const float MaxArmour = 100f;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public ArmourCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        var st = _players.Get(player.Id);
        if (st is null || st.IsDead || !_config.Enabled) return;
        if (!_config.GetCheck("ArmourHack").Enabled) return;

        long now = Environment.TickCount64;
        if (now - st.SetArmourTick < 1500) { st.Armour = player.Armour; return; }
        if (now - st.SpawnTick < 2500) { st.Armour = player.Armour; return; }

        float arm = player.Armour;
        float gain = arm - st.Armour;

        if (gain <= AllowedGain) { st.Armour = arm; return; }
        if (st.SetArmour >= 0) { st.SetArmour = -1; st.Armour = arm; return; }

        if (arm > MaxArmour + 0.5f)
            _warnings.AddWarning(player.Id, "ArmourHack", $"arm={arm:F1} (>100)");
        else
            _warnings.AddWarning(player.Id, "ArmourHack", $"gain={gain:F1}");

        st.Armour = arm;
    }

    public void OnPlayerSpawned(BasePlayer player)
    {
        var st = _players.Get(player.Id);
        if (st is null) return;
        st.Armour = player.Armour;
        st.SetArmour = -1;
    }
}