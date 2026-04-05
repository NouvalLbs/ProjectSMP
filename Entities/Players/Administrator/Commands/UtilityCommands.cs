using ProjectSMP.Extensions;
using ProjectSMP.Core;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;
using SampSharp.GameMode.World;
using System.Linq;

namespace ProjectSMP.Entities.Players.Administrator.Commands
{
    public class UtilityCommands : AdminCommandBase
    {
        [Command("staff")]
        public static void Staff(Player player, string text)
        {
            if (player.Admin < 1)
            {
                player.SendClientMessage(Color.White, "{b9b9b9}Command '/staff' tidak ada, gunakan '/help'.");
                return;
            }

            var adminTier = Utilities.GetAdminStringChat(player);
            foreach (var p in BasePlayer.All.OfType<Player>().Where(p => p.Admin > 0))
            {
                p.SendClientMessage(0x15D4EDFF, $"{adminTier} {player.Ucp}(Id:{player.Id}): {text}");
            }
        }

        [Command("asay")]
        public static void ASay(Player player, string text)
        {
            if (!CheckAdmin(player, 1)) return;

            var adminTier = Utilities.GetAdminStringChat(player);
            BasePlayer.SendClientMessageToAll(0xFF6347AA, $"(( {{FFFF00}}{adminTier} {player.Ucp}: {{00FF00}}{Utilities.ColouredText(text)} {{FF6347}}))");
        }

        [Command("coords")]
        public static void Coords(Player player, string name, int devMode = 0)
        {
            if (!CheckAdmin(player, 1)) return;

            var pos = player.Position;
            var coords = $"{name} | {pos.X}, {pos.Y}, {pos.Z}, {player.Angle}";

            if (devMode == 1)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} {pos.X}, {pos.Y}, {pos.Z}, {player.Angle} disalin ke console dengan nama {name}");
                System.Console.WriteLine(coords);
                return;
            }

            System.IO.File.AppendAllText("coords.txt", coords + "\n");
            player.SendClientMessage(Color.White, $"{Msg.AdmCmd} {pos.X}, {pos.Y}, {pos.Z}, {player.Angle} disimpan dengan nama {name}");
        }

        [Command("tp")]
        public static void Teleport(Player player, float x, float y, float z, int interior = 0, int vw = 0)
        {
            if (!CheckAdmin(player, 1)) return;

            player.SetInteriorSafe(interior);
            player.SetVirtualWorldSafe(vw);
            player.SetPositionSafe(x, y, z);
        }

        [Command("setweather")]
        public static void SetWeather(Player player, int weatherId)
        {
            if (!CheckAdmin(player, 2)) return;

            Server.SetWeather(weatherId);
            foreach (var p in BasePlayer.All)
                p.SetWeather(weatherId);

            BasePlayer.SendClientMessageToAll(Color.White, $"{Msg.AdmCmd} Weather telah dirubah oleh {{ff0000}}{player.Ucp}{{FFFFFF}}");
        }

        [Command("jetpack")]
        public static void Jetpack(Player player)
        {
            if (!CheckAdmin(player, 2) || !ValidateCharLoaded(player)) return;

            if (player.SpecialAction == SpecialAction.Usejetpack)
            {
                player.SetSpecialActionSafe(SpecialAction.None);
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Kamu telah melepas jetpack.");
            }
            else
            {
                player.SetSpecialActionSafe(SpecialAction.Usejetpack);
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Kamu telah memakai jetpack.");
            }
        }

        [Command("noclip", Shortcut = "nc")]
        public static void NoClip(Player player)
        {
            if (!CheckAdmin(player, 1)) return;

            if (player.GetData("NoClipActive", false))
            {
                NoClipService.Stop(player);
                player.SetData("NoClipActive", false);
            }
            else
            {
                NoClipService.Start(player);
                player.SetData("NoClipActive", true);
            }
        }

        [Command("setint")]
        public static void SetInt(Player player, string targetInput, int interiorId)
        {
            if (!CheckAdmin(player, 3)) return;

            var target = GetTargetPlayer(player, targetInput);
            if (!ValidateTarget(player, target)) return;

            target.SetInteriorSafe(interiorId);
            player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Kamu telah mengubah interior {{00FFFF}}{target.CharInfo.Username} (ID:{target.Id}){{FFFFFF}} menjadi {{00FFFF}}{interiorId}{{FFFFFF}}!");
            target.SendClientMessage(Color.White, $"{Msg.AdmCmd} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah mengubah interior kamu menjadi {{00FFFF}}{interiorId}{{FFFFFF}}");
        }

        [Command("setvw")]
        public static void SetVW(Player player, string targetInput, int vwId)
        {
            if (!CheckAdmin(player, 3)) return;

            var target = GetTargetPlayer(player, targetInput);
            if (!ValidateTarget(player, target)) return;

            target.SetVirtualWorldSafe(vwId);
            player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Kamu telah mengubah virtual world {{00FFFF}}{target.CharInfo.Username} (ID:{target.Id}){{FFFFFF}} menjadi {{00FFFF}}{vwId}{{FFFFFF}}!");
            target.SendClientMessage(Color.White, $"{Msg.AdmCmd} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah mengubah virtual world kamu menjadi {{00FFFF}}{vwId}{{FFFFFF}}");
        }

        [Command("getip")]
        public static void GetIP(Player player, string targetInput)
        {
            if (!CheckAdmin(player, 3)) return;

            var target = GetTargetPlayer(player, targetInput);
            if (!ValidateTarget(player, target)) return;

            var ip = target.IP;
            player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Informasi IP {{00FFFF}}{target.CharInfo.Username} (ID:{target.Id} | UCP: {target.Ucp}){{FFFFFF}}");
            player.SendClientMessage(Color.White, $"{{FF6347}}>{{FFFFFF}} IP Address: {{00FFFF}}{ip}");
        }
    }
}