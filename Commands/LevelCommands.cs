using ProjectSMP.Core;
using ProjectSMP.Features.LevelSystem;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Commands
{
    public class LevelCommands
    {
        private const int LevelsPerPage = 50;

        [Command("levels")]
        public static void Levels(Player player)
        {
            if (!player.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Kamu belum login.");
                return;
            }

            ShowLevelProgression(player, 0);
        }

        private static void ShowLevelProgression(Player player, int page)
        {
            const int MaxLevel = 100;
            var startLevel = page * LevelsPerPage + 1;
            var endLevel = Math.Min(startLevel + LevelsPerPage - 1, MaxLevel);

            var rows = new List<string[]>();

            for (var i = startLevel; i <= endLevel; i++)
            {
                var pointsRequired = LevelService.GetPointsRequired(i);
                var totalHours = LevelService.GetCumulativeHoursRequired(i);

                var completed = player.Level > i;
                var isCurrentLevel = player.Level == i;

                var currentPoints = isCurrentLevel ? player.LevelPoints : (completed ? pointsRequired : 0);

                var progressPoints = LevelService.CreateProgressBar(currentPoints, pointsRequired, completed);

                var levelText = $"{{ffffff}}Level {i}{(isCurrentLevel ? " (current level)" : "")}";
                var pointsText = $"{progressPoints} {{ffeea8}}({currentPoints}/{pointsRequired})";

                rows.Add(new[] { levelText, pointsText, totalHours.ToString() });
            }

            if (page > 0)
                rows.Add(new[] { "{FF69B4}<< Previous", "", "" });

            if (endLevel < MaxLevel)
                rows.Add(new[] { "{ADFF2F}>> Next", "", "" });

            player.ShowTabList("Level Progress", new[] { "Level", "Points", "Total Jam" })
                .WithRows(rows.ToArray())
                .WithButtons("Select", "Close")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) return;

                    var selected = rows[e.ListItem][0];

                    if (selected.Contains("<< Previous"))
                    {
                        ShowLevelProgression(player, page - 1);
                        return;
                    }

                    if (selected.Contains(">> Next"))
                    {
                        ShowLevelProgression(player, page + 1);
                        return;
                    }
                });
        }
    }
}