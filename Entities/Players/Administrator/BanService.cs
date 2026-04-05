using ProjectSMP.Core;
using System;
using System.Threading.Tasks;

namespace ProjectSMP.Entities.Players.Administrator
{
    internal static class BanService
    {
        private const string Table = "players";

        public static bool IsPlayerBanned(Player player)
        {
            if (player.BanInfo.Banned == 0)
                return false;

            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (player.BanInfo.Expire > 0 && currentTime > player.BanInfo.Expire)
            {
                UnbanPlayerAsync(player);
                return false;
            }

            ShowBanDialog(player);
            ScheduleKick(player);
            return true;
        }

        private static async void UnbanPlayerAsync(Player player)
        {
            player.BanInfo.Banned = 0;
            player.BanInfo.Time = 0;
            player.BanInfo.Expire = 0;
            player.BanInfo.Reason = "";
            player.BanInfo.Admin = "";

            await DatabaseManager.ExecuteAsync(
                $"UPDATE `{Table}` SET `ban` = 0, `bantime` = 0, `banexpire` = 0, `banreason` = '', `banadmin` = '' WHERE `citizenId` = @CitizenId",
                new { CitizenId = player.CitizenId });
        }

        private static void ShowBanDialog(Player player)
        {
            var message = FormatBanMessage(player);
            var header = $"{{ffffff}}Character {{34ebc0}}{player.CharInfo.Username}{{ffffff}} dalam status banned";

            player.ShowMessage(header, message)
                .WithButtons("Close")
                .Show();
        }

        private static string FormatBanMessage(Player player)
        {
            if (player.BanInfo.Expire > 0)
            {
                var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var timeLeft = player.BanInfo.Expire - currentTime;
                var days = timeLeft / 86400;
                var hours = (timeLeft % 86400) / 3600;

                return $"{{ffffff}}Charactermu sedang dalam status banned!\n\n" +
                       $"Character: {{34ebc0}}{player.CharInfo.Username}{{ffffff}}\n" +
                       $"Alasan: {{ffff00}}{player.BanInfo.Reason}{{ffffff}}\n" +
                       $"Admin: {{ff0000}}{player.BanInfo.Admin}{{ffffff}}\n" +
                       $"Waktu tersisa: {{ff0000}}{days} hari {hours} jam{{ffffff}}\n\n" +
                       $"Jika anda merasa ini adalah kesalahan, ajukan banding ke discord.";
            }

            return $"{{ffffff}}Charactermu sedang dalam status banned!\n\n" +
                   $"Character: {{34ebc0}}{player.CharInfo.Username}{{ffffff}}\n" +
                   $"Alasan: {{ffff00}}{player.BanInfo.Reason}{{ffffff}}\n" +
                   $"Admin: {{ff0000}}{player.BanInfo.Admin}{{ffffff}}\n" +
                   $"Waktu tersisa: {{ff0000}}Permanent{{ffffff}}\n\n" +
                   $"Jika anda merasa ini adalah kesalahan, ajukan banding ke discord.";
        }

        private static async void ScheduleKick(Player player)
        {
            await Task.Delay(1000);
            if (!player.IsDisposed)
                player.Kick();
        }
    }
}