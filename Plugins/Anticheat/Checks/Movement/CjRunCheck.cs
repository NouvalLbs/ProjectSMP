using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Movement;

public class CjRunCheck
{
    private const float CjRunThreshold = 0.390f;
    private const int SA_Jetpack = 2;
    private const int SA_Skydive = 24;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public CjRunCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("CjRun").Enabled) return;
        if (player.State != PlayerState.OnFoot) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;
        if (now - st.PlayerVelocityTick < 2000) return;

        int sa = (int)player.SpecialAction;
        if (sa == SA_Jetpack || sa == SA_Skydive) return;

        var vel = player.Velocity;
        float spd = VectorMath.Speed(vel.X, vel.Y, 0f);

        if (spd > CjRunThreshold)
            _warnings.AddWarning(player.Id, "CjRun", $"spd={spd:F3}");
    }
}