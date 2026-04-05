using ProjectSMP.Core;
using ProjectSMP.Entities.Players.Administrator.Commands;
using ProjectSMP.Extensions;
using ProjectSMP.Features.Dynamic.DynamicDoor;
using SampSharp.GameMode;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;

namespace ProjectSMP.Features.Dynamic.DynamicDoor.Commands
{
    public class DoorCommands : AdminCommandBase
    {
        [Command("createdoor")]
        public static async void CreateDoor(Player player, string name)
        {
            if (!CheckAdmin(player, 5)) return;

            var position = player.Position;
            var doorId = await DoorService.CreateAsync(name, position, player.Angle, player.VirtualWorld, player.Interior);

            if (doorId == -1)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Dynamic Doors sudah tidak bisa dibuat lagi!");
                return;
            }

            player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Kamu telah berhasil membuat Dynamic Door dengan DoorId: {doorId}.");
        }

        [Command("gotodoor")]
        public static void GoToDoor(Player player, int doorId)
        {
            if (!CheckAdmin(player, 1) || !ValidateCharLoaded(player)) return;

            var door = DoorService.GetDoor(doorId);
            if (door == null)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Dynamic Doors dengan DoorId: {doorId} tidak tersedia!");
                return;
            }

            player.SetPositionSafe(new Vector3(door.ExtPosX, door.ExtPosY, door.ExtPosZ));
            player.Angle = door.ExtAngle;
            player.SetInteriorSafe(door.ExtInterior);
            player.SetVirtualWorldSafe(door.ExtVirtualWorld);

            player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Kamu berhasil di teleport ke DoorId: {doorId}.");
        }

        [Command("editdoor")]
        public static async void EditDoor(Player player, int doorId, string type, string value = "")
        {
            if (!CheckAdmin(player, 5)) return;

            var door = DoorService.GetDoor(doorId);
            if (door == null)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Dynamic Doors dengan DoorId: {doorId} tidak tersedia!");
                return;
            }

            switch (type.ToLower())
            {
                case "location":
                    var pos = player.Position;
                    door.ExtPosX = pos.X;
                    door.ExtPosY = pos.Y;
                    door.ExtPosZ = pos.Z;
                    door.ExtAngle = player.Angle;
                    door.ExtVirtualWorld = player.VirtualWorld;
                    door.ExtInterior = player.Interior;
                    await DoorService.SaveAsync(doorId);
                    DoorService.UpdateDoor(doorId);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Location DoorId {doorId} berhasil disesuaikan.");
                    break;

                case "interior":
                    var intPos = player.Position;
                    door.IntPosX = intPos.X;
                    door.IntPosY = intPos.Y;
                    door.IntPosZ = intPos.Z;
                    door.IntAngle = player.Angle;
                    door.IntVirtualWorld = player.VirtualWorld;
                    door.IntInterior = player.Interior;
                    await DoorService.SaveAsync(doorId);
                    DoorService.UpdateDoor(doorId);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Interior spawn DoorId {doorId} berhasil disesuaikan.");
                    break;

                case "password":
                    door.Password = value.Equals("none", System.StringComparison.OrdinalIgnoreCase) ? "" : value;
                    await DoorService.SaveAsync(doorId);
                    DoorService.UpdateDoor(doorId);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Password DoorId {doorId} diubah.");
                    break;

                case "name":
                    door.Name = Utilities.ColouredText(value);
                    await DoorService.SaveAsync(doorId);
                    DoorService.UpdateDoor(doorId);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Name DoorId {doorId} diubah menjadi \"{door.Name}\".");
                    break;

                case "locked":
                    if (!int.TryParse(value, out int locked) || locked < 0 || locked > 1)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Input 0 untuk Unlock dan 1 untuk Lock!");
                        return;
                    }
                    door.Locked = locked == 1;
                    await DoorService.SaveAsync(doorId);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} DoorId {doorId} {(door.Locked ? "Locked" : "Unlocked")}.");
                    break;

                case "admin":
                    if (!int.TryParse(value, out int adminLevel) || adminLevel < 0 || adminLevel > 5)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Input Level Admin [0 - 5]!");
                        return;
                    }
                    door.AdminLevel = adminLevel;
                    await DoorService.SaveAsync(doorId);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} DoorId {doorId} untuk admin Level {adminLevel}.");
                    break;

                case "vip":
                    if (!int.TryParse(value, out int vipLevel) || vipLevel < 0 || vipLevel > 3)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Input Level VIP [0 - 3]!");
                        return;
                    }
                    door.VipLevel = vipLevel;
                    await DoorService.SaveAsync(doorId);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} DoorId {doorId} untuk VIP Level {vipLevel}.");
                    break;

                case "faction":
                    if (!int.TryParse(value, out int factionId) || factionId < 0 || factionId > 4)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Input FactionId [0 - 4]!");
                        return;
                    }
                    door.FactionId = factionId;
                    await DoorService.SaveAsync(doorId);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} DoorId {doorId} untuk Faction Id {factionId}.");
                    break;

                case "family":
                    if (!int.TryParse(value, out int familyId) || familyId < -1 || familyId > 9)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Input FamilyId [-1 - 9]!");
                        return;
                    }
                    door.FamilyId = familyId;
                    await DoorService.SaveAsync(doorId);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} DoorId {doorId} untuk Family Id {familyId}.");
                    break;

                case "garage":
                    if (!int.TryParse(value, out int garage) || garage < 0 || garage > 1)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Input 0 untuk Disable dan 1 untuk Enable!");
                        return;
                    }
                    door.IsGarage = garage == 1;
                    await DoorService.SaveAsync(doorId);
                    DoorService.UpdateDoor(doorId);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Garasi DoorId {doorId} {(door.IsGarage ? "Enabled" : "Disabled")}.");
                    break;

                case "custom":
                    if (!int.TryParse(value, out int custom) || custom < 0 || custom > 1)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Input 0 atau 1!");
                        return;
                    }
                    door.CustomInterior = custom == 1;
                    await DoorService.SaveAsync(doorId);
                    DoorService.UpdateDoor(doorId);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Interior mode DoorId {doorId} {(door.CustomInterior ? "Enabled" : "Disabled")}.");
                    break;

                case "virtual":
                    door.ExtVirtualWorld = player.VirtualWorld;
                    await DoorService.SaveAsync(doorId);
                    DoorService.UpdateDoor(doorId);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Virtual world DoorId {doorId} disesuaikan.");
                    break;

                case "mapicon":
                    if (!int.TryParse(value, out int mapIconId) || mapIconId < -1)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Input MapIcon ID [-1 untuk disable]!");
                        return;
                    }
                    door.MapIconId = mapIconId;
                    await DoorService.SaveAsync(doorId);
                    DoorService.UpdateDoor(doorId);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} MapIcon DoorId {doorId} diubah menjadi {mapIconId}.");
                    break;

                case "delete":
                    await DoorService.DeleteAsync(doorId);
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} DoorId {doorId} berhasil dihapus.");
                    break;

                default:
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd_G} Gunakan /editdoor [DoorId] [Prefix]");
                    player.SendClientMessage(Color.White, "{FF6347}>> Prefix{888888}: location, interior, password, name, locked, admin, vip, faction, family, custom, virtual, garage, mapicon, delete");
                    break;
            }
        }
    }
}