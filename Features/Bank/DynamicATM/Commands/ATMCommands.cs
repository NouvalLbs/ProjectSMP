using ProjectSMP.Core;
using ProjectSMP.Entities.Players.Administrator.Commands;
using ProjectSMP.Extensions;
using SampSharp.GameMode;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;

namespace ProjectSMP.Features.Bank.DynamicATM.Commands
{
    public class ATMCommands : AdminCommandBase
    {
        [Command("createatm")]
        public static async void CreateATM(Player player)
        {
            if (!CheckAdmin(player, 5)) return;

            var pos = player.Position;
            var rot = new Vector3(0, 0, player.Angle);
            var id = await ATMService.CreateAsync(pos, rot, player.VirtualWorld, player.Interior);

            if (id == -1)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} ATM sudah mencapai batas maksimal!");
                return;
            }

            player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Dynamic ATM berhasil dibuat dengan ID: {id}.");
        }

        [Command("gotoatm")]
        public static void GotoATM(Player player, int id)
        {
            if (!CheckAdmin(player, 1) || !ValidateCharLoaded(player)) return;

            var atm = ATMService.GetATM(id);
            if (atm == null)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} ATM ID {id} tidak ditemukan!");
                return;
            }

            player.SetPositionSafe(new Vector3(atm.PosX, atm.PosY, atm.PosZ));
            player.SetInteriorSafe(atm.Interior);
            player.SetVirtualWorldSafe(atm.VirtualWorld);
            player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Teleport ke ATM ID {id}.");
        }

        [Command("editatm")]
        public static async void EditATM(Player player, int id, string type)
        {
            if (!CheckAdmin(player, 5)) return;

            var atm = ATMService.GetATM(id);
            if (atm == null)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} ATM ID {id} tidak ditemukan!");
                return;
            }

            switch (type.ToLower())
            {
                case "location":
                    ATMService.StartEdit(player, id);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Gunakan editor untuk mengatur posisi ATM ID {id}.");
                    break;

                case "delete":
                    await ATMService.DeleteAsync(id);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} ATM ID {id} berhasil dihapus.");
                    break;

                default:
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd_G} Gunakan /editatm [ID] [Prefix]");
                    player.SendClientMessage(Color.White, "{FF6347}>> Prefix{888888}: location, delete");
                    break;
            }
        }
    }
}