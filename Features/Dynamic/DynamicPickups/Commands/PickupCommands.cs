using ProjectSMP.Entities.Players.Administrator.Commands;
using ProjectSMP.Extensions;
using ProjectSMP.Core;
using SampSharp.GameMode;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;

namespace ProjectSMP.Features.Dynamic.DynamicPickups.Commands
{
    public class PickupCommands : AdminCommandBase
    {
        [Command("createpickup")]
        public static async void CreatePickup(Player player, string title, string name, int modelId, int type, string callback)
        {
            if (!CheckAdmin(player, 5)) return;

            if (type < 1 || type > 23)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Type harus antara 1-23!");
                return;
            }

            var position = player.Position;
            var pickupId = await PickupService.CreateAsync(title, name, modelId, type, callback, position, player.VirtualWorld, player.Interior);

            if (pickupId == -1)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Dynamic Pickups sudah tidak bisa dibuat lagi!");
                return;
            }

            player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Kamu telah berhasil membuat Dynamic Pickup dengan PickupId: {pickupId}.");
        }

        [Command("gotopickup")]
        public static void GoToPickup(Player player, int pickupId)
        {
            if (!CheckAdmin(player, 1) || !ValidateCharLoaded(player)) return;

            var pickup = PickupService.GetPickup(pickupId);
            if (pickup == null)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Dynamic Pickup dengan PickupId: {pickupId} tidak tersedia!");
                return;
            }

            player.SetPositionSafe(new Vector3(pickup.PosX, pickup.PosY, pickup.PosZ));
            player.SetInteriorSafe(pickup.Interior);
            player.SetVirtualWorldSafe(pickup.VirtualWorld);

            player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Kamu berhasil di teleport ke PickupId: {pickupId}.");
        }

        [Command("editpickup")]
        public static async void EditPickup(Player player, int pickupId, string type, string value = "")
        {
            if (!CheckAdmin(player, 5)) return;

            var pickup = PickupService.GetPickup(pickupId);
            if (pickup == null)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Dynamic Pickup dengan PickupId: {pickupId} tidak tersedia!");
                return;
            }

            switch (type.ToLower())
            {
                case "location":
                    var pos = player.Position;
                    pickup.PosX = pos.X;
                    pickup.PosY = pos.Y;
                    pickup.PosZ = pos.Z;
                    pickup.VirtualWorld = player.VirtualWorld;
                    pickup.Interior = player.Interior;
                    await PickupService.SaveAsync(pickupId);
                    PickupService.UpdatePickup(pickupId);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Location PickupId {pickupId} berhasil disesuaikan.");
                    break;

                case "model":
                    if (!int.TryParse(value, out int modelId))
                    {
                        player.SendClientMessage(Color.White, $"{Msg.AdmCmd_G} Gunakan /editpickup [PickupId] [model] [Model ID]");
                        return;
                    }
                    pickup.ModelId = modelId;
                    await PickupService.SaveAsync(pickupId);
                    PickupService.UpdatePickup(pickupId);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Model PickupId {pickupId} diubah menjadi {modelId}.");
                    break;

                case "type":
                    if (!int.TryParse(value, out int typeId) || typeId < 1 || typeId > 23)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Type harus antara 1-23!");
                        return;
                    }
                    pickup.Type = typeId;
                    await PickupService.SaveAsync(pickupId);
                    PickupService.UpdatePickup(pickupId);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Type PickupId {pickupId} diubah menjadi {typeId}.");
                    break;

                case "title":
                    pickup.Title = Utilities.ColouredText(value);
                    await PickupService.SaveAsync(pickupId);
                    PickupService.UpdatePickup(pickupId);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Title PickupId {pickupId} diubah menjadi \"{pickup.Title}\".");
                    break;

                case "name":
                    pickup.Name = Utilities.ColouredText(value);
                    await PickupService.SaveAsync(pickupId);
                    PickupService.UpdatePickup(pickupId);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Name PickupId {pickupId} diubah menjadi \"{pickup.Name}\".");
                    break;

                case "callback":
                    pickup.Callback = value;
                    await PickupService.SaveAsync(pickupId);
                    PickupService.UpdatePickup(pickupId);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Callback PickupId {pickupId} diubah menjadi \"{value}\".");
                    break;

                case "delete":
                    await PickupService.DeleteAsync(pickupId);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} PickupId {pickupId} berhasil dihapus.");
                    break;

                default:
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd_G} Gunakan /editpickup [PickupId] [Prefix]");
                    player.SendClientMessage(Color.White, "{FF6347}>> Prefix{888888}: location, model, type, title, name, callback, delete");
                    break;
            }
        }
    }
}