using ProjectSMP.Core;
using ProjectSMP.Entities.Players.Administrator.Commands;
using ProjectSMP.Extensions;
using SampSharp.GameMode;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;

namespace ProjectSMP.Features.Jobs.Side.Trashmaster.DynamicTrash.Commands
{
    public class TrashCommands : AdminCommandBase
    {
        [Command("createtrash")]
        public static async void CreateTrash(Player player)
        {
            if (!CheckAdmin(player, 5)) return;

            var pos = player.Position;
            var rot = new Vector3(0, 0, player.Angle);
            var id = await TrashService.CreateAsync(pos, rot, player.VirtualWorld, player.Interior);

            if (id == -1)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Trash sudah mencapai batas maksimal!");
                return;
            }

            player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Dynamic Trash berhasil dibuat dengan ID: {id}.");
        }

        [Command("gototrash")]
        public static void GotoTrash(Player player, int id)
        {
            if (!CheckAdmin(player, 1) || !ValidateCharLoaded(player)) return;

            var trash = TrashService.GetTrash(id);
            if (trash == null)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Trash ID {id} tidak ditemukan!");
                return;
            }

            player.SetPositionSafe(new Vector3(trash.PosX, trash.PosY, trash.PosZ));
            player.SetInteriorSafe(trash.Interior);
            player.SetVirtualWorldSafe(trash.VirtualWorld);
            player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Teleport ke Trash ID {id}.");
        }

        [Command("edittrash")]
        public static async void EditTrash(Player player, int id, string type)
        {
            if (!CheckAdmin(player, 5)) return;

            var trash = TrashService.GetTrash(id);
            if (trash == null)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Trash ID {id} tidak ditemukan!");
                return;
            }

            switch (type.ToLower())
            {
                case "location":
                    TrashService.StartEdit(player, id);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Gunakan editor untuk mengatur posisi Trash ID {id}.");
                    break;

                case "delete":
                    await TrashService.DeleteAsync(id);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Trash ID {id} berhasil dihapus.");
                    break;

                case "fill":
                    TrashService.SetAmount(id, 100);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Trash ID {id} diisi penuh (100/100).");
                    break;

                case "empty":
                    TrashService.SetAmount(id, 0);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Trash ID {id} dikosongkan (0/100).");
                    break;

                default:
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd_G} Gunakan /edittrash [ID] [Prefix]");
                    player.SendClientMessage(Color.White, "{FF6347}>> Prefix{888888}: location, delete, fill, empty");
                    break;
            }
        }
    }
}
