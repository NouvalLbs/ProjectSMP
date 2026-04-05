using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Movement;

public class AirBreakCheck
{
    private const float MaxZGainNoVelocity = 2.0f;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public AirBreakCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        if (!_config.Enabled) return;
        var st = _players.Get(player.Id);
        if (st is null || st.IsDead || st.IsSpectating) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;
        if (now - st.SetPosTick < 2000) return;
        if (now - st.PlayerVelocityTick < 2000) return;

        var pState = player.State;
        var vel = player.Velocity;
        var pos = player.Position;

        if (player.SpecialAction == SpecialAction.Usejetpack) return;

        float zDiff = pos.Z - st.Z;

        if (pState == PlayerState.OnFoot)
        {
            if (!_config.GetCheck("AirBreakOnfoot").Enabled) return;

            bool isJetpacking = player.SpecialAction == SpecialAction.Usejetpack;
            int anim = st.Anim;

            if (isJetpacking)
            {
                bool validJetpackAnim = (1128 <= anim && anim <= 1134) || (1538 <= anim && anim <= 1544);
                if (!validJetpackAnim && anim != 0)
                {
                    _warnings.AddWarning(player.Id, "AirBreakOnfoot", $"fakeJetpack anim={anim}");
                }
                st.WasJetpacking = true;
                return;
            }

            if (st.WasJetpacking && !isJetpacking)
            {
                st.DropJpX = pos.X;
                st.DropJpY = pos.Y;
                st.DropJpTick = now;
                st.WasJetpacking = false;
            }

            if (st.DropJpTick > 0 && now - st.DropJpTick < 3000)
            {
                float dropDist = VectorMath.Dist2D(pos.X, pos.Y, st.DropJpX, st.DropJpY);
                if (dropDist > 50.0f) st.DropJpTick = 0;
                else return;
            }

            if (st.IsParachuting) return;
            if (player.SurfingVehicle is not null) return;
            if (zDiff > MaxZGainNoVelocity && vel.Z is >= 0f and < 0.05f)
                _warnings.AddWarning(player.Id, "AirBreakOnfoot", $"dz={zDiff:F2} vz={vel.Z:F3}");
        }
        else if (pState == PlayerState.Driving)
        {
            if (!_config.GetCheck("AirBreakVehicle").Enabled) return;
            if (now - st.VehicleVelocityTick < 2000) return;
            if (zDiff > MaxZGainNoVelocity * 1.5f && MathF.Abs(vel.Z) < 0.05f)
                _warnings.AddWarning(player.Id, "AirBreakVehicle", $"dz={zDiff:F2}");
        }
    }
}