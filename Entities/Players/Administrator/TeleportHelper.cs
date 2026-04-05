using ProjectSMP.Extensions;
using SampSharp.GameMode.World;

namespace ProjectSMP.Entities.Players.Administrator
{
    internal static class TeleportHelper
    {
        public static void TeleportToPlayer(Player source, Player target, bool reverse = false)
        {
            var actualTarget = reverse ? source : target;
            var actualSource = reverse ? target : source;

            var pos = actualTarget.Position;

            if (actualSource.InAnyVehicle)
            {
                var vehicle = actualSource.Vehicle;
                vehicle.SetPositionSafe(pos.X + 2, pos.Y, pos.Z);
                vehicle.LinkToInteriorSafe(actualTarget.Interior);
            }
            else
            {
                actualSource.SetPositionSafe(pos.X + 1, pos.Y, pos.Z);
            }

            actualSource.SetInteriorSafe(actualTarget.Interior);
            actualSource.SetVirtualWorldSafe(actualTarget.VirtualWorld);

            // actualSource.InHouse = actualTarget.InHouse;
            // actualSource.InBiz = actualTarget.InBiz;
            // actualSource.InDoor = actualTarget.InDoor;
        }

        public static void TeleportToLocation(Player player, float x, float y, float z, int interior, int virtualWorld)
        {
            if (player.InAnyVehicle)
            {
                var vehicle = player.Vehicle;
                vehicle.SetPositionSafe(x, y, z);
                vehicle.LinkToInteriorSafe(interior);
            }
            else
            {
                player.SetPositionSafe(x, y, z);
            }

            player.SetInteriorSafe(interior);
            player.SetVirtualWorldSafe(virtualWorld);

            // player.InHouse = 0;
            // player.InBiz = 0;
            // player.InDoor = 0;
        }
    }
}