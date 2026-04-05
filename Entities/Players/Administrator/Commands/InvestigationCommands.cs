using ProjectSMP.Core;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;
using System.Linq;

namespace ProjectSMP.Entities.Players.Administrator.Commands
{
    public class InvestigationCommands : AdminCommandBase
    {
        [Command("aka")]
        public static async void AKA(Player player, string username)
        {
            if (!CheckAdmin(player, 3)) return;

            var query = "SELECT `ip`, `username`, `last_login` FROM `players` WHERE `username` = @Username LIMIT 1";
            var result = await DatabaseManager.QueryFirstAsync<dynamic>(query, new { Username = username });

            if (result == null)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Player dengan username tersebut tidak ditemukan di database!");
                return;
            }

            string ip = result.ip;
            player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Data player {username}:");
            player.SendClientMessage(Color.White, $"{{FF6347}}>{{FFFFFF}} IP: {ip} | Last Login: {result.last_login}");

            var altQuery = "SELECT `username` FROM `players` WHERE `ip` = @IP AND `username` != @Username";
            var alts = await DatabaseManager.QueryAsync<dynamic>(altQuery, new { IP = ip, Username = username });

            if (!alts.Any())
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Tidak ada akun lain yang menggunakan IP yang sama dengan {username}");
                return;
            }

            player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Akun lain yang menggunakan IP yang sama dengan {username}:");
            foreach (var alt in alts)
            {
                player.SendClientMessage(Color.White, $"{{FF6347}}>{{FFFFFF}} {alt.username}");
            }
        }

        [Command("akaip")]
        public static async void AKAIP(Player player, string ip)
        {
            if (!CheckAdmin(player, 3)) return;

            if (ip.Length < 7)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Format IP tidak valid!");
                return;
            }

            var query = "SELECT username FROM players WHERE ip = @IP";
            var results = await DatabaseManager.QueryAsync<dynamic>(query, new { IP = ip });

            if (!results.Any())
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Tidak ada akun yang menggunakan IP {{00FFFF}}{ip}{{FFFFFF}}");
                return;
            }

            player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Daftar akun yang menggunakan IP {{00FFFF}}{ip}{{FFFFFF}}:");
            foreach (var result in results)
            {
                player.SendClientMessage(Color.White, $"{{FF6347}}>{{FFFFFF}} {result.username}");
            }
        }
    }
}