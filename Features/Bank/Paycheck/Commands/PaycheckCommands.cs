using ProjectSMP.Core;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;
using System.Collections.Generic;
using System;

namespace ProjectSMP.Features.Bank.Paycheck.Commands
{
    public class PaycheckCommands
    {
        private const int ItemsPerPage = 10;

        [Command("mysalary", Shortcut = "salary")]
        public static void MySalary(Player player)
        {
            if (!player.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Kamu belum login.");
                return;
            }
            ShowSalaryDialog(player, 0);
        }

        private static void ShowSalaryDialog(Player player, int page)
        {
            var list = player.PaycheckData.PaycheckList;
            var total = PaycheckService.GetTotal(player);
            var totalPages = Math.Max(1, (list.Count + ItemsPerPage - 1) / ItemsPerPage);
            page = Math.Clamp(page, 0, totalPages - 1);

            var startIdx = page * ItemsPerPage;
            var endIdx = Math.Min(startIdx + ItemsPerPage, list.Count);

            var rows = new List<string[]>();

            for (var i = startIdx; i < endIdx; i++)
            {
                var e = list[i];
                rows.Add(new[] { e.Time, e.From, $"{{00FF00}}{Utilities.GroupDigits(e.Amount)}{{ffffff}}" });
            }

            if (page > 0)
                rows.Add(new[] { "{FF6347}<< Previous{ffffff}", "", "" });

            if (page < totalPages - 1)
                rows.Add(new[] { "{ADFF2F}>> Next{ffffff}", "", "" });

            rows.Add(new[] { "Total Salary", ":", $"{{00FF00}}{Utilities.GroupDigits(total)}" });

            player.ShowTabList(
                $"Pending Salary (Page {page + 1}/{totalPages})",
                new[] { "Time", "From", "Amount" })
                .WithRows(rows.ToArray())
                .WithButtons("Select", "Close")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) return;

                    var selected = rows[e.ListItem][0];

                    if (selected.Contains("<< Previous")) { ShowSalaryDialog(player, page - 1); return; }
                    if (selected.Contains(">> Next")) { ShowSalaryDialog(player, page + 1); return; }
                    if (selected.Contains("Total Salary"))
                    {
                        var msg = PaycheckService.CanClaim(player)
                            ? "{00FF00}Gaji kamu siap diambil di Bank!"
                            : $"Gaji bisa diambil dalam {{FF6347}}{PaycheckService.GetTimeLeft(player)}";
                        player.SendClientMessage(Color.White, $"{Msg.Bank} {msg}");
                    }

                    ShowSalaryDialog(player, page);
                });
        }
    }
}