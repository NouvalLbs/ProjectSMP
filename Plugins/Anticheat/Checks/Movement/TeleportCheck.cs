using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Movement;

public class TeleportCheck
{
    private const float MaxOnfootDist = 25f;
    private const float MaxVehicleDist = 40f;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public TeleportCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerUpdate(BasePlayer player)
    {
        var st = _players.Get(player.Id);
        if (st is null || st.IsDead || st.IsSpectating) return;

        long now = Environment.TickCount64;
        var pos = player.Position;

        bool graced = now - st.SetPosTick < 1500
                   || now - st.SpawnTick < 2500
                   || now - st.SpectateTick < 2000
                   || now - st.PutInVehicleTick < 2000
                   || now - st.EnterVehicleTick < 2500
                   || now - st.RemoveFromVehicleTick < 2000;

        if (!graced && _config.Enabled)
        {
            float dist = VectorMath.Dist(pos.X, pos.Y, pos.Z, st.X, st.Y, st.Z);
            var pState = player.State;

            if (pState == PlayerState.OnFoot && _config.GetCheck("TeleportOnfoot").Enabled)
            {
                if (dist > MaxOnfootDist)
                    _warnings.AddWarning(player.Id, "TeleportOnfoot", $"d={dist:F1}");
            }
            else if (pState == PlayerState.Driving && _config.GetCheck("TeleportVehicle").Enabled)
            {
                if (dist > MaxVehicleDist)
                    _warnings.AddWarning(player.Id, "TeleportVehicle", $"d={dist:F1}");
            }
        }

        // Selalu update posisi terakhir
        st.X = pos.X;
        st.Y = pos.Y;
        st.Z = pos.Z;
        st.UpdateTick = now;
    }
}