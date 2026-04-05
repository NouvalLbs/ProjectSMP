using ProjectSMP.Core;
using ProjectSMP.Extensions;
using ProjectSMP.Plugins.ColAndreas;
using SampSharp.GameMode;
using SampSharp.GameMode.SAMP;

namespace ProjectSMP.Entities.Players.Administrator
{
    public static class WaypointTeleportService
    {
        private const float RaycastStartZ = 700f;
        private const float RaycastEndZ = -1000f;
        private const float FallbackOffsetZ = 3f;
        private const float GroundOffsetZ = 0.5f;

        public static void HandleWaypoint(Player player, Vector3 clickedPos)
        {
            if (!player.AdminOnDuty) return;

            var x = clickedPos.X;
            var y = clickedPos.Y;
            float z;

            var hit = ColAndreasService.RayCastLine(
                x, y, RaycastStartZ,
                x, y, RaycastEndZ,
                out _, out _, out var groundZ);

            z = hit != 0 ? groundZ + GroundOffsetZ : clickedPos.Z + FallbackOffsetZ;

            TeleportHelper.TeleportToLocation(player, x, y, z, 0, player.GetVirtualWorldSafe());
            player.PutCameraBehindPlayer();
            player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Teleport ke waypoint: {{00FFFF}}{x:F1}, {y:F1}, {z:F1}");
        }
    }
}