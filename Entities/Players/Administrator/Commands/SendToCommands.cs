using ProjectSMP.Core;
using ProjectSMP.Entities.Players.Administrator.Data;
using ProjectSMP.Extensions;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;
using SampSharp.GameMode.World;
using System.Linq;

namespace ProjectSMP.Entities.Players.Administrator.Commands
{
    public class SendToCommands : AdminCommandBase
    {
        [Command("sendto")]
        public static void SendTo(Player player, string targetInput, string cityName = "")
        {
            if (!CheckAdmin(player, 1) || !ValidateCharLoaded(player)) return;

            var target = GetTargetPlayer(player, targetInput);
            if (!ValidateTarget(player, target)) return;

            if (target.JailInfo.Jailed > 0)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Player target sedang berada di jail!");
                return;
            }

            player.SetData("SendTo_Target", target.Id);

            if (string.IsNullOrEmpty(cityName))
            {
                ShowCityDialog(player);
                return;
            }

            var cityId = cityName.ToLower() switch
            {
                "ls" => 1,
                "sf" => 2,
                "lv" => 3,
                _ => 0
            };

            if (cityId == 0)
            {
                ShowCityDialog(player);
                return;
            }

            player.SetData("SendTo_City", cityId);
            ShowLocationDialog(player);
        }

        private static void ShowCityDialog(Player player)
        {
            player.ShowList("Select City", "Los Santos (LS)", "San Fierro (SF)", "Las Venturas (LV)")
                .WithButtons("Select", "Cancel")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        player.SetData("SendTo_Target", -1);
                        player.SetData("SendTo_City", 0);
                        return;
                    }

                    var cityId = e.ListItem + 1;
                    player.SetData("SendTo_City", cityId);
                    ShowLocationDialog(player);
                });
        }

        private static void ShowLocationDialog(Player player)
        {
            var cityId = player.GetData("SendTo_City", 0);
            var targetId = player.GetData("SendTo_Target", -1);

            if (cityId == 0 || targetId == -1) return;

            var target = BasePlayer.Find(targetId) as Player;
            if (target == null || !target.IsConnected)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Player target telah disconnect.");
                player.SetData("SendTo_Target", -1);
                player.SetData("SendTo_City", 0);
                return;
            }

            var locations = SendToData.All.Where(loc => loc.City == cityId).ToArray();
            var items = new string[locations.Length];
            for (var i = 0; i < locations.Length; i++)
                items[i] = locations[i].Name;

            player.ShowList($"Locations - City {cityId}", items)
                .WithButtons("Select", "Back")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        ShowCityDialog(player);
                        return;
                    }

                    var loc = locations[e.ListItem];
                    var tgt = BasePlayer.Find(targetId) as Player;
                    if (tgt == null || !tgt.IsConnected)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Error} Player target telah disconnect.");
                        player.SetData("SendTo_Target", -1);
                        player.SetData("SendTo_City", 0);
                        return;
                    }

                    TeleportHelper.TeleportToLocation(tgt, loc.X, loc.Y, loc.Z, loc.Interior, loc.VirtualWorld);
                    tgt.PutCameraBehindPlayer();

                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Kamu telah mengirim {{00FFFF}}{tgt.CharInfo.Username} (ID:{tgt.Id}){{FFFFFF}} ke {{00FFFF}}{loc.Name}{{FFFFFF}}!");
                    tgt.SendClientMessage(Color.White, $"{Msg.AdmCmd} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah mengirim kamu ke {{00FFFF}}{loc.Name}{{FFFFFF}}");

                    player.SetData("SendTo_Target", -1);
                    player.SetData("SendTo_City", 0);
                });
        }
    }
}