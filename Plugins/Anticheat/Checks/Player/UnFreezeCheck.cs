using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Player;

public class UnFreezeCheck
{
    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public UnFreezeCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("UnFreeze").Enabled) return;
        if (player.State != PlayerState.OnFoot) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead || !st.IsFrozen) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;

        var vel = player.Velocity;
        float spd = VectorMath.Speed(vel.X, vel.Y, vel.Z);
        if (spd > 0.05f)
            _warnings.AddWarning(player.Id, "UnFreeze", $"spd={spd:F3}");
    }

    public void OnPlayerFrozen(int playerId, bool frozen)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.IsFrozen = frozen;
    }
}