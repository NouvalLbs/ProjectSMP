using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.AntiNop;

public class NopSetPlayerPosCheck
{
    private const long DeadlineMs = 3850;
    private const float MaxAllowedDist = 10f;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public NopSetPlayerPosCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnSetPlayerPos(int playerId, float x, float y, float z)
    {
        var st = _players.Get(playerId);
        if (st is null) return;

        st.NopSetPosX = x;
        st.NopSetPosY = y;
        st.NopSetPosZ = z;
        st.NopSetPosDeadline = Environment.TickCount64 + DeadlineMs;
        st.NopSetPosPending = true;
    }

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled || !_config.GetCheck("NopSetPos").Enabled) return;

        var st = _players.Get(player.Id);
        if (st is null || st.IsDead || !st.NopSetPosPending) return;
        if (st.TpToZ) { st.NopSetPosPending = false; st.TpToZ = false; return; }

        long now = Environment.TickCount64;
        if (now < st.NopSetPosDeadline) return;

        var pState = player.State;
        if (pState == PlayerState.Spectating) { st.NopSetPosPending = false; return; }
        if (pState == PlayerState.Driving ||
            pState == PlayerState.Passenger) { st.NopSetPosPending = false; return; }

        var pos = player.Position;
        float dist = VectorMath.Dist(pos.X, pos.Y, pos.Z,
                                     st.NopSetPosX, st.NopSetPosY, st.NopSetPosZ);

        if (dist > MaxAllowedDist)
            _warnings.AddWarning(player.Id, "NopSetPos",
                $"expected=({st.NopSetPosX:F1},{st.NopSetPosY:F1},{st.NopSetPosZ:F1}) " +
                $"got=({pos.X:F1},{pos.Y:F1},{pos.Z:F1}) dist={dist:F1}");

        st.NopSetPosPending = false;
    }

    public void OnPlayerSpawned(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.NopSetPosPending = false;
    }

    public void OnPlayerDied(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.NopSetPosPending = false;
    }

    public void OnPlayerEnterVehicle(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.NopSetPosPending = false;
    }

    public void OnPlayerExitVehicle(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.NopSetPosPending = false;
    }
}