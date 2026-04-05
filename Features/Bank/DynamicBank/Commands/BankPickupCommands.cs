using ProjectSMP.Core;
using ProjectSMP.Entities.Players.Administrator.Commands;
using ProjectSMP.Extensions;
using SampSharp.GameMode;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;

namespace ProjectSMP.Features.Bank.DynamicBank.Commands
{
    public class BankPickupCommands : AdminCommandBase
    {
        [Command("createbank")]
        public static async void CreateBank(Player player, string name)
        {
            if (!CheckAdmin(player, 5)) return;

            var pos = player.Position;
            var id = await BankPickupService.CreateAsync(name, pos, player.VirtualWorld, player.Interior);

            if (id == -1)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Bank location sudah mencapai batas maksimal!");
                return;
            }

            player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Bank location '{name}' berhasil dibuat dengan ID: {id}.");
        }

        [Command("gotobank")]
        public static void GotoBank(Player player, int id)
        {
            if (!CheckAdmin(player, 1) || !ValidateCharLoaded(player)) return;

            var bank = BankPickupService.GetBank(id);
            if (bank == null)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Bank location ID {id} tidak ditemukan!");
                return;
            }

            player.SetPositionSafe(new Vector3(bank.PosX, bank.PosY, bank.PosZ));
            player.SetInteriorSafe(bank.Interior);
            player.SetVirtualWorldSafe(bank.VirtualWorld);
            player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Teleport ke Bank location ID {id}.");
        }

        [Command("editbank")]
        public static async void EditBank(Player player, int id, string type, string value = "")
        {
            if (!CheckAdmin(player, 5)) return;

            var bank = BankPickupService.GetBank(id);
            if (bank == null)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Bank location ID {id} tidak ditemukan!");
                return;
            }

            switch (type.ToLower())
            {
                case "location":
                    var pos = player.Position;
                    bank.PosX = pos.X;
                    bank.PosY = pos.Y;
                    bank.PosZ = pos.Z;
                    bank.VirtualWorld = player.VirtualWorld;
                    bank.Interior = player.Interior;
                    await BankPickupService.SaveAsync(id);
                    BankPickupService.Rebuild(id);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Location Bank ID {id} diperbarui.");
                    break;

                case "name":
                    bank.Name = Utilities.ColouredText(value);
                    await BankPickupService.SaveAsync(id);
                    BankPickupService.Rebuild(id);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Nama Bank ID {id} diubah menjadi '{bank.Name}'.");
                    break;

                case "delete":
                    await BankPickupService.DeleteAsync(id);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Bank location ID {id} berhasil dihapus.");
                    break;

                default:
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd_G} Gunakan /editbank [ID] [Prefix]");
                    player.SendClientMessage(Color.White, "{FF6347}>> Prefix{888888}: location, name, delete");
                    break;
            }
        }
    }
}