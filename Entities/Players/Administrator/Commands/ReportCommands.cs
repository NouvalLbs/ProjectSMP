using ProjectSMP.Core;
using ProjectSMP.Extensions;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;
using SampSharp.GameMode.World;
using System.Collections.Generic;
using System.Linq;

namespace ProjectSMP.Entities.Players.Administrator.Commands
{
    public class ReportCommands : AdminCommandBase
    {
        [Command("report")]
        public static void Report(Player player, string text)
        {
            if (!player.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Kamu belum login.");
                return;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                player.SendClientMessage(Color.White, $"{Msg.Command} Gunakan /report [text]");
                return;
            }

            if (!ReportService.CanReport(player))
            {
                var cooldown = ReportService.GetCooldown(player);
                player.SendClientMessage(Color.White, $"{Msg.Error} Wait {cooldown} seconds before sending another report!");
                return;
            }

            ReportService.AddReport(player, text);
            player.SendClientMessage(Color.White, $"{Msg.Report} Your report has been issued to the queue, use '{{ffff00}}/reports{{ffffff}}' to see your report");
        }

        [Command("reports")]
        public static void Reports(Player player)
        {
            if (!player.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Kamu belum login.");
                return;
            }

            var activeReports = ReportService.GetActiveReports();

            if (activeReports.Count == 0)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Tidak ada report aktif saat ini.");
                return;
            }

            var rows = new List<string[]>();
            var reportMapping = new Dictionary<int, int>();
            var listIndex = 0;

            foreach (var report in activeReports.Take(40))
            {
                var timeStr = report.CreatedAt.ToString("HH:mm:ss");

                rows.Add(new[]
                {
                    $"{{ffffff}}P{report.ReporterId}: {{FFFF00}}{report.ReporterName}",
                    $"{{ffffff}}{timeStr}",
                    $"{{ffffff}}{report.Message}"
                });

                reportMapping[listIndex] = report.Id;
                listIndex++;
            }

            player.SetData("ReportMapping", reportMapping);
            player.ShowTabList("Report Queue", new[] { "Reporter", "Time", "Message" })
                .WithRows(rows.ToArray())
                .WithButtons("Accept", "Close")
                .Show(e => {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        player.SetData<Dictionary<int, int>>("ReportMapping", null);
                        return;
                    }

                    var mapping = player.GetData<Dictionary<int, int>>("ReportMapping", null);
                    if (mapping == null)
                        return;

                    if (!mapping.TryGetValue(e.ListItem, out var reportId))
                        return;

                    if (player.Admin < 1)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Error} You don't have permission to accept reports.");
                        return;
                    }

                    player.SetData<Dictionary<int, int>>("ReportMapping", null);
                    ReportService.AcceptReport(player, reportId);
                });
        }

        [Command("ar")]
        public static void AcceptReport(Player player, int reportId)
        {
            if (!CheckAdmin(player, 1)) return;
            var reports = ReportService.GetActiveReports();
            var report = reports.FirstOrDefault(r => r.Id == reportId);

            if (report == null)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} That report ID is not being used!");
                return;
            }

            var reporter = BasePlayer.Find(report.ReporterId) as Player;
            if (reporter == null || !reporter.IsConnected)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} The reporter has disconnected!");
                ReportService.DeleteReport(player, reportId);
                return;
            }

            ReportService.AcceptReport(player, reportId);
        }

        [Command("dr")]
        public static void DeleteReport(Player player, int reportId)
        {
            if (!CheckAdmin(player, 1)) return;
            var reports = ReportService.GetActiveReports();
            var report = reports.FirstOrDefault(r => r.Id == reportId);

            if (report == null)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} That report ID is not being used!");
                return;
            }

            ReportService.DeleteReport(player, reportId);
        }

        [Command("clearallreports")]
        public static void ClearAllReports(Player player)
        {
            if (!CheckAdmin(player, 5)) return;
            ReportService.ClearAllReports();
            player.SendClientMessage(Color.White, $"{Msg.AdmCmd} You have cleared all the active reports.");
            Utilities.SendStaffMessage(-1, "{{FF6347}}AdmCmd: {0} has cleared all the pending reports.", player.Ucp);
        }
    }
}