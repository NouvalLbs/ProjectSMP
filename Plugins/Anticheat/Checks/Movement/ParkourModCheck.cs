using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Movement;

public class ParkourModCheck
{
    private const float MaxWallClimbVZ = 0.35f;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public ParkourModCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("ParkourMod").Enabled) return;
        if (player.State != PlayerState.OnFoot) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;
        if (now - st.PlayerVelocityTick < 2000) return;
        if (player.SpecialAction == SpecialAction.Usejetpack) return;

        var vel = player.Velocity;
        if (vel.Z > MaxWallClimbVZ && MathF.Abs(vel.X) < 0.05f && MathF.Abs(vel.Y) < 0.05f)
            _warnings.AddWarning(player.Id, "ParkourMod", $"vz={vel.Z:F3}");
    }
}