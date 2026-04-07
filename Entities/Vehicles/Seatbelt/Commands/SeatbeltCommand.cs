using ProjectSMP.Core;
using ProjectSMP.Entities.Vehicles.Seatbelt;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;

namespace ProjectSMP.Entities.Vehicles.Seatbelt.Commands
{
    public class SeatbeltCommand
    {
        [Command("seatbelt", Shortcut = "sb")]
        public static void OnSeatbeltCommand(Player player)
        {
            if (!player.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Kamu belum login.");
                return;
            }
            SeatbeltService.ToggleSeatbelt(player);
        }
    }
}