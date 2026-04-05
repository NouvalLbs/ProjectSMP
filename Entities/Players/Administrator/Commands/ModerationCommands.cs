using ProjectSMP.Core;
using ProjectSMP.Entities.Players.Condition;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;
using SampSharp.GameMode.World;
using System;
using System.Linq;

namespace ProjectSMP.Entities.Players.Administrator.Commands
{
    public class ModerationCommands : AdminCommandBase
    {
        [Command("kick")]
        public static void Kick(Player player, string targetInput, string reason)
        {
            if (!CheckAdmin(player, 1)) return;

            var target = GetTargetPlayer(player, targetInput);
            if (!ValidateTarget(player, target) || !CheckAdminRank(player, target)) return;

            BasePlayer.SendClientMessageToAll(Color.White, $"{{992712}}<AdmCmd> {target.CharInfo.Username} telah dikeluarkan dari server oleh {player.Ucp}.");
            BasePlayer.SendClientMessageToAll(Color.White, $"{{992712}}Alasan: {reason}");
            Utilities.KickEx(target, 500);
        }

        [Command("jail")]
        public static void Jail(Player player, string targetInput, int seconds, string reason)
        {
            if (!CheckAdmin(player, 2)) return;

            var target = GetTargetPlayer(player, targetInput);
            if (!ValidateTarget(player, target) || !CheckAdminRank(player, target)) return;

            if (target.JailInfo.Time > 0)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Player tersebut sudah berada di jail!");
                return;
            }

            if (!JailService.JailPlayer(target, seconds, reason))
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Waktu jail harus antara 1 - 3600 detik!");
                return;
            }

            BasePlayer.SendClientMessageToAll(Color.White, $"{{992712}}<AdmCmd> {target.CharInfo.Username} telah dijail oleh {player.Ucp} selama {seconds} detik.");
            BasePlayer.SendClientMessageToAll(Color.White, $"{{992712}}Alasan: {reason}");
        }

        [Command("unjail")]
        public static void Unjail(Player player, string targetInput)
        {
            if (!CheckAdmin(player, 2)) return;

            var target = GetTargetPlayer(player, targetInput);
            if (!ValidateTarget(player, target)) return;

            if (target.JailInfo.Time <= 0)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Player tersebut tidak berada di jail!");
                return;
            }

            JailService.UnjailPlayer(target);
            player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Kamu telah mengeluarkan {{00FFFF}}{target.CharInfo.Username} (ID:{target.Id}){{FFFFFF}} dari jail!");
            target.SendClientMessage(Color.White, $"{Msg.AdmCmd} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah mengeluarkan kamu dari jail");
        }

        [Command("ban")]
        public static async void Ban(Player player, string targetInput, int days, string reason)
        {
            if (!CheckAdmin(player, 3)) return;

            var target = GetTargetPlayer(player, targetInput);
            if (!ValidateTarget(player, target) || !CheckAdminRank(player, target)) return;

            var banTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var banExpire = days > 0 ? banTime + (days * 86400) : 0;

            target.BanInfo.Time = (int)banTime;
            target.BanInfo.Expire = (int)banExpire;
            target.BanInfo.Reason = reason;
            target.BanInfo.Admin = player.Ucp;

            var query = @"UPDATE `players` SET `ban` = 1, `bantime` = @BanTime, `banexpire` = @BanExpire, 
                         `banreason` = @Reason, `banadmin` = @Admin WHERE `citizenId` = @CitizenId";
            await DatabaseManager.ExecuteAsync(query, new
            {
                BanTime = banTime,
                BanExpire = banExpire,
                Reason = reason,
                Admin = player.Ucp,
                CitizenId = target.CitizenId
            });

            var msg = days > 0
                ? $"{{992712}}<AdmCmd> {target.CharInfo.Username} telah di-ban dari server oleh {player.Ucp} selama {days} hari."
                : $"{{992712}}<AdmCmd> {target.CharInfo.Username} telah di-ban secara permanent dari server oleh {player.Ucp}.";

            BasePlayer.SendClientMessageToAll(Color.White, msg);
            BasePlayer.SendClientMessageToAll(Color.White, $"{{992712}}Alasan: {reason}");
            Utilities.KickEx(target, 500);
        }

        [Command("oban")]
        public static async void OBan(Player player, string username, int days, string reason)
        {
            if (!CheckAdmin(player, 4)) return;

            var checkQuery = "SELECT * FROM `players` WHERE JSON_UNQUOTE(JSON_EXTRACT(char_info, '$.Username')) = @Username LIMIT 1";
            var result = await DatabaseManager.QueryFirstAsync<dynamic>(checkQuery, new { Username = username });

            if (result == null)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Player dengan nama {username} tidak ditemukan di database!");
                return;
            }

            string citizenId = result.citizenId;
            foreach (var p in BasePlayer.All.OfType<Player>())
            {
                if (p.IsCharLoaded && p.CitizenId == citizenId)
                {
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Player sedang online, gunakan /ban.");
                    return;
                }
            }

            int adminLevel = result.admin;
            if (adminLevel > player.Admin)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Kamu tidak dapat ban admin dengan level lebih tinggi!");
                return;
            }

            var banTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var banExpire = days > 0 ? banTime + (days * 86400) : 0;

            var query = @"UPDATE `players` SET `ban` = 1, `bantime` = @BanTime, `banexpire` = @BanExpire, 
                         `banreason` = @Reason, `banadmin` = @Admin WHERE `citizenId` = @CitizenId";
            await DatabaseManager.ExecuteAsync(query, new
            {
                BanTime = banTime,
                BanExpire = banExpire,
                Reason = reason,
                Admin = player.Ucp,
                CitizenId = citizenId
            });

            var msg = days > 0
                ? $"{{992712}}<AdmCmd> {username} telah di-offline ban dari server oleh {player.Ucp} selama {days} hari."
                : $"{{992712}}<AdmCmd> {username} telah di-offline ban secara permanent dari server oleh {player.Ucp}.";

            BasePlayer.SendClientMessageToAll(Color.White, msg);
            BasePlayer.SendClientMessageToAll(Color.White, $"{{992712}}Alasan: {reason}");
        }

        [Command("unban")]
        public static async void Unban(Player player, string username)
        {
            if (!CheckAdmin(player, 3)) return;

            var checkQuery = "SELECT * FROM `players` WHERE JSON_UNQUOTE(JSON_EXTRACT(char_info, '$.Username')) = @Username LIMIT 1";
            var result = await DatabaseManager.QueryFirstAsync<dynamic>(checkQuery, new { Username = username });

            if (result == null)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Player dengan nama {username} tidak ditemukan di database!");
                return;
            }

            int isBanned = result.ban;
            if (isBanned == 0)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Player tersebut tidak dalam status banned!");
                return;
            }

            var query = @"UPDATE `players` SET `ban` = 0, `bantime` = 0, `banexpire` = 0, 
                         `banreason` = '', `banadmin` = '' WHERE JSON_UNQUOTE(JSON_EXTRACT(char_info, '$.Username')) = @Username";
            await DatabaseManager.ExecuteAsync(query, new { Username = username });

            BasePlayer.SendClientMessageToAll(Color.White, $"{{992712}}<AdmCmd> {username} telah di-unban dari server oleh {player.Ucp}.");
        }

        [Command("revive")]
        public static void Revive(Player player, string targetInput)
        {
            if (!CheckAdmin(player, 3)) return;

            var target = GetTargetPlayer(player, targetInput);
            if (!ValidateTarget(player, target)) return;

            if (target.Condition.Injured < 1)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Player tersebut tidak dalam keadaan mati!");
                return;
            }

            ConditionService.RevivePlayerInPlace(target);
            player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Kamu telah melakukan revive terhadap {{00FFFF}}{target.CharInfo.Username} (ID:{target.Id}){{FFFFFF}}!");
            target.SendClientMessage(Color.White, $"{Msg.AdmCmd} Admin {{00FFFF}}{player.Ucp}{{FFFFFF}} telah melakukan revive terhadap kamu");
        }
    }
}