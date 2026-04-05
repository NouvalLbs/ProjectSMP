using ProjectSMP.Core;
using ProjectSMP.Extensions;
using ProjectSMP.Plugins.WeaponConfig;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Entities.Players.Administrator.Commands
{
    public class DamageLogCommands : AdminCommandBase
    {
        [Command("damagelog")]
        public static async void DamageLog(Player player, string targetInput)
        {
            if (!CheckAdmin(player, 2)) return;

            var target = GetTargetPlayer(player, targetInput);
            if (!ValidateTarget(player, target)) return;

            var logs = await DamageLogService.GetLogsAsync(target.CitizenId);

            if (logs.Count == 0)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Tidak ada damage log untuk {{00FFFF}}{target.CharInfo.Username}{{FFFFFF}}.");
                return;
            }

            ShowDamageLogDialog(player, target, logs, 0);
        }

        private static void ShowDamageLogDialog(Player player, Player target, List<DamageLogEntry> logs, int page)
        {
            const int itemsPerPage = 40;
            var totalPages = (int)Math.Ceiling((double)logs.Count / itemsPerPage);

            if (page >= totalPages) page = totalPages - 1;
            if (page < 0) page = 0;

            var startIdx = page * itemsPerPage;
            var endIdx = Math.Min(startIdx + itemsPerPage, logs.Count);

            var rows = new List<string[]>();

            for (var i = startIdx; i < endIdx; i++)
            {
                var log = logs[i];
                var dt = DateTimeOffset.FromUnixTimeSeconds(log.Timestamp).ToLocalTime();
                var dateTime = dt.ToString("dd/MM/yyyy HH:mm:ss");
                var weaponName = WeaponConfigService.GetWeaponName(log.Weapon);
                var bodypart = WeaponConfigService.GetBodypartName(log.Bodypart);
                var damage = $"{log.Amount:F2} - {bodypart}";

                rows.Add(new[] { log.Issuer, dateTime, weaponName, damage });
            }

            if (page > 0)
                rows.Add(new[] { "{FF6347}<< Previous", "", "", "" });

            if (page < totalPages - 1)
                rows.Add(new[] { "{91ff00}>> Next", "", "", "" });

            player.SetData("DamageLog_Page", page);
            player.SetData("DamageLog_Target", target.Id);
            player.SetData("DamageLog_Logs", logs);

            var title = totalPages > 1
                ? $"Damage Log: {target.CharInfo.Username} (Page {page + 1}/{totalPages})"
                : $"Damage Log: {target.CharInfo.Username}";

            player.ShowTabList(title, new[] { "Issuer", "Date & Time", "Weapon", "Damage" })
                .WithRows(rows.ToArray())
                .WithButtons("Close", "")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        player.SetData("DamageLog_Page", -1);
                        player.SetData("DamageLog_Target", -1);
                        player.SetData<List<DamageLogEntry>>("DamageLog_Logs", null);
                        return;
                    }

                    var selectedRow = rows[e.ListItem];

                    if (selectedRow[0].Contains("<< Previous"))
                    {
                        var currentPage = player.GetData("DamageLog_Page", 0);
                        var currentLogs = player.GetData<List<DamageLogEntry>>("DamageLog_Logs", null);
                        var currentTarget = SampSharp.GameMode.World.BasePlayer.Find(player.GetData("DamageLog_Target", -1)) as Player;

                        if (currentLogs != null && currentTarget != null)
                            ShowDamageLogDialog(player, currentTarget, currentLogs, currentPage - 1);
                        return;
                    }

                    if (selectedRow[0].Contains(">> Next"))
                    {
                        var currentPage = player.GetData("DamageLog_Page", 0);
                        var currentLogs = player.GetData<List<DamageLogEntry>>("DamageLog_Logs", null);
                        var currentTarget = SampSharp.GameMode.World.BasePlayer.Find(player.GetData("DamageLog_Target", -1)) as Player;

                        if (currentLogs != null && currentTarget != null)
                            ShowDamageLogDialog(player, currentTarget, currentLogs, currentPage + 1);
                        return;
                    }
                });
        }
    }
}