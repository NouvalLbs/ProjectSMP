#nullable enable
using ProjectSMP.Core;
using SampSharp.GameMode.SAMP.Commands;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ProjectSMP.Entities.Players.Delay.Commands
{
    public class DelaysCommand
    {
        private static Language Lang(Player p) => p.IsCharLoaded ? p.Settings.Language : Language.ID;
        private static string L(Player p, string s, string k) => LocalizationManager.Get(Lang(p), s, k);

        [Command("delays")]
        public static void OnDelaysCommand(Player player)
        {
            if (!player.IsCharLoaded)
                return;

            var rows = new List<string[]>
            {
                new[] { "{FFFF00}Quit Job", "-", FormatQuitJobDelay(player) }
            };

            AddDelayRow(rows, "Lumber", "Job", player.Delays.Lumber);
            AddDelayRow(rows, "Product", "Job", player.Delays.Product);
            AddDelayRow(rows, "Trucker Hauling", "Job", player.Delays.TruckerHauling);
            AddDelayRow(rows, "Trucker Mission", "Job", player.Delays.TruckerMission);
            AddDelayRow(rows, "Trucker Container", "Job", player.Delays.TruckerContainer);
            AddDelayRow(rows, "Trucker Crate", "Job", player.Delays.TruckerCrate);
            AddDelayRow(rows, "Forager", "Job", player.Delays.Forager);
            AddDelayRow(rows, "Farm", "Job", player.Delays.Farm);
            AddDelayRow(rows, "Sweeper", "Sidejob", player.Delays.Sweeper);
            AddDelayRow(rows, "Courir", "Sidejob", player.Delays.Courir);
            AddDelayRow(rows, "Forklifter", "Sidejob", player.Delays.Forklifter);
            AddDelayRow(rows, "Bus", "Sidejob", player.Delays.Bus);
            AddDelayRow(rows, "Trashmaster", "Sidejob", player.Delays.Trashmaster);
            AddDelayRow(rows, "Mower", "Sidejob", player.Delays.Mower);
            AddDelayRow(rows, "Miner", "Sidejob", player.Delays.Miner);
            AddDelayRow(rows, "Fisherman", "Activity", player.Delays.Fisherman);
            AddDelayRow(rows, "Honey", "Activity", player.Delays.Honey);

            player.ShowTabList(
                $"Delays: {player.CharInfo.Username}",
                new[] { "Name", "Type", "Delays" })
                .WithRows(rows.ToArray())
                .WithButtons("Close", "")
                .Show(e => { });
        }

        private static void AddDelayRow(List<string[]> rows, string name, string type, int delayMinutes)
        {
            var color = delayMinutes > 0 ? "{FF6347}" : "{00FF00}";
            var display = delayMinutes > 0 ? $"{color}{delayMinutes} Minute(s)" : $"{color}No Delay";
            rows.Add(new[] { name, type, display });
        }

        private static string FormatQuitJobDelay(Player player)
        {
            if (string.IsNullOrEmpty(player.Delays.QuitJob))
                return "{00FF00}No Delay";

            if (!DateTime.TryParse(player.Delays.QuitJob, out var expireTime))
                return "{00FF00}No Delay";

            if (DateTime.UtcNow >= expireTime)
                return "{00FF00}No Delay";

            var formatted = expireTime.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            return $"{{FF6347}}{formatted}";
        }
    }
}