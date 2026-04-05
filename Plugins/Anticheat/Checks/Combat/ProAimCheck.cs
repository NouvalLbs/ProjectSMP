using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.Anticheat.Checks.Combat;

public class ProAimCheck
{
    private const float MaxAngleDiff = 15f;

    private readonly PlayerStateManager _players;
    private readonly WarningManager _warnings;
    private readonly AnticheatConfig _config;

    public ProAimCheck(PlayerStateManager p, WarningManager w, AnticheatConfig c)
        => (_players, _warnings, _config) = (p, w, c);

    public void OnPlayerWeaponShot(BasePlayer player, WeaponShotEventArgs e)
    {
        if (!_config.Enabled || !_config.GetCheck("ProAim").Enabled) return;
        if (e.BulletHitType != BulletHitType.Player) return;

        var st = _players.Get(player.Id);
        if (st is null) return;

        long now = Environment.TickCount64;
        if (now - st.SpawnTick < 3000) return;

        var hitPos = e.Position;
        float dx = hitPos.X - st.X;
        float dy = hitPos.Y - st.Y;

        float aimAngle = MathF.Atan2(dx, dy) * (180f / MathF.PI);
        float facing = player.Angle;
        float diff = AngleHelper.Diff(aimAngle, facing);

        if (diff > MaxAngleDiff)
            _warnings.AddWarning(player.Id, "ProAim",
                $"facing={facing:F1} aim={aimAngle:F1} diff={diff:F1}");
    }
}