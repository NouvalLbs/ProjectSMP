using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.State;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.Anticheat.Checks.Combat;

public class LagCompSpoofCheck
{
    private static readonly Dictionary<int, float> _maxRange = new()
    {
        {22,35f},{23,35f},{24,45f},
        {25,15f},{26,15f},{27,25f},
        {28,40f},{29,45f},{32,40f},
        {30,70f},{31,70f},
        {33,100f},{34,350f},
        {35,200f},{36,200f}
    };

    private static readonly Dictionary<int, float> _minRange = new()
    {
        {22,25f},{23,25f},{24,25f},{25,30f},{26,25f},{27,35f},
        {28,25f},{29,35f},{30,40f},{31,40f},{32,25f},{33,55f},
        {34,50f},{35,50f},{36,50f},{37,4f},{38,65f}
    };

    private const float LagcompTolerance = 50f;
    private const float MaxAngleDifference = 25f;
    private const float MaxVerticalAngleDiff = 30f;
    private const float MinDistanceForCheck = 5f;

    private static readonly HashSet<int> _validWeapons = new()
    {
        22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 38
    };

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public LagCompSpoofCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerTakeDamage(BasePlayer victim, DamageEventArgs e)
    {
        if (!_config.Enabled || !_config.GetCheck("LagCompSpoof").Enabled) return;
        if (e.OtherPlayer is not BasePlayer issuer) return;

        var ist = _players.Get(issuer.Id);
        var vst = _players.Get(victim.Id);
        if (ist is null || vst is null) return;

        int wid = (int)e.Weapon;

        CheckLagCompRange(issuer, victim, ist, vst, wid);

        if (issuer.State == PlayerState.OnFoot)
            CheckSilentAimOnFoot(issuer, victim, ist, vst, wid);
        else if (issuer.State == PlayerState.Driving)
            CheckSilentAimVehicle(issuer, victim, ist, vst, wid);
    }

    private void CheckLagCompRange(BasePlayer issuer, BasePlayer victim, PlayerAcState ist, PlayerAcState vst, int wid)
    {
        if (!_maxRange.TryGetValue(wid, out float maxRange)) return;

        float dist = VectorMath.Dist(ist.X, ist.Y, ist.Z, vst.X, vst.Y, vst.Z);
        float limit = maxRange + LagcompTolerance;

        if (dist > limit)
            _warnings.AddWarning(issuer.Id, "LagCompSpoof",
                $"dist={dist:F1} limit={limit:F1} wid={wid}");

        if (_minRange.TryGetValue(wid, out float minRange) && dist < minRange)
            _warnings.AddWarning(issuer.Id, "LagCompSpoof",
                $"dist={dist:F1} tooClose min={minRange:F1} wid={wid}");
    }

    private void CheckSilentAimOnFoot(BasePlayer issuer, BasePlayer victim, PlayerAcState ist, PlayerAcState vst, int wid)
    {
        if (!_validWeapons.Contains(wid)) return;

        long now = Environment.TickCount64;
        if (now - ist.SpawnTick < 3000) return;

        var issuerPos = issuer.Position;
        var victimPos = victim.Position;

        float distance = VectorMath.Dist(
            issuerPos.X, issuerPos.Y, issuerPos.Z,
            victimPos.X, victimPos.Y, victimPos.Z
        );

        float expectedAngle = MathF.Atan2(
            victimPos.Y - issuerPos.Y,
            victimPos.X - issuerPos.X
        ) * (180f / MathF.PI);

        float playerAngle = issuer.Angle;
        float angleDiff = AngleHelper.Diff(expectedAngle, playerAngle);

        if (angleDiff <= MaxAngleDifference) return;

        issuer.GetKeys(out Keys keys, out _, out _);
        bool isAiming = (keys & Keys.Aim) != 0 || (keys & Keys.SecondaryAttack) != 0;

        float verticalAngle = VectorMath.ElevationAngle(
            issuerPos.Z,
            victimPos.X - issuerPos.X,
            victimPos.Y - issuerPos.Y,
            victimPos.Z
        );

        float absVertical = MathF.Abs(verticalAngle);

        if (angleDiff > MaxAngleDifference) {
            if (!isAiming) {
                _warnings.AddWarning(issuer.Id, "LagCompSpoof",
                    $"silentAim noAim angle={angleDiff:F1}° dist={distance:F1} wid={wid}");
            } else if (absVertical > MaxVerticalAngleDiff) {
                _warnings.AddWarning(issuer.Id, "LagCompSpoof",
                    $"silentAim angle={angleDiff:F1}° vert={absVertical:F1}° dist={distance:F1} wid={wid}");
            } else {
                _warnings.AddWarning(issuer.Id, "LagCompSpoof",
                    $"silentAim horizontal angle={angleDiff:F1}° dist={distance:F1} wid={wid}");
            }
        }
    }

    private void CheckSilentAimVehicle(BasePlayer issuer, BasePlayer victim, PlayerAcState ist, PlayerAcState vst, int wid)
    {
        if (!_validWeapons.Contains(wid)) return;

        var issuerPos = issuer.Position;
        var victimPos = victim.Position;

        float distance = VectorMath.Dist(
            issuerPos.X, issuerPos.Y, issuerPos.Z,
            victimPos.X, victimPos.Y, victimPos.Z
        );

        float expectedAngle = MathF.Atan2(
            victimPos.Y - issuerPos.Y,
            victimPos.X - issuerPos.X
        ) * (180f / MathF.PI);

        float playerAngle = issuer.Angle;
        float angleDiff = AngleHelper.Diff(expectedAngle, playerAngle);

        if (angleDiff > MaxAngleDifference * 1.5f)
        {
            _warnings.AddWarning(issuer.Id, "LagCompSpoof",
                $"silentAim vehicle angle={angleDiff:F1}° dist={distance:F1} wid={wid}");
        }
    }
}