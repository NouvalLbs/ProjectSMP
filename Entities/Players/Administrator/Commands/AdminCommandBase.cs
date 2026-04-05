using ProjectSMP.Core;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System;
using System.Linq;

namespace ProjectSMP.Entities.Players.Administrator.Commands
{
    public abstract class AdminCommandBase
    {
        protected static bool CheckAdmin(Player player, int level)
        {
            if (player.Admin < level)
            {
                player.SendClientMessage(Color.White, "{b9b9b9}Command tidak ada, gunakan '/help'.");
                return false;
            }
            if (!player.AdminOnDuty)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Command tidak dapat digunakan ketika kamu tidak duty.");
                return false;
            }
            return true;
        }

        protected static bool ValidateCharLoaded(Player player)
        {
            if (!player.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Kamu belum login!");
                return false;
            }
            return true;
        }

        protected static bool ValidateTarget(Player player, Player target)
        {
            if (target == null) return false;

            if (!target.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Player target belum spawn!");
                return false;
            }
            return true;
        }

        protected static bool CheckAdminRank(Player player, Player target)
        {
            if (target.Admin > player.Admin)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Kamu tidak dapat melakukan aksi pada admin dengan level lebih tinggi!");
                return false;
            }
            return true;
        }

        protected static Player GetTargetPlayer(Player sender, string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                sender.SendClientMessage(Color.White, $"{Msg.Error} Input tidak valid!");
                return null;
            }

            if (int.TryParse(input, out var playerId))
            {
                if (playerId < 0 || playerId >= 1000)
                {
                    sender.SendClientMessage(Color.White, $"{Msg.Error} Player ID {playerId} tidak valid (0-999)!");
                    return null;
                }

                var targetById = BasePlayer.Find(playerId) as Player;
                if (targetById == null || !targetById.IsConnected)
                {
                    sender.SendClientMessage(Color.White, $"{Msg.Error} Player dengan ID {playerId} tidak ditemukan atau tidak online.");
                    return null;
                }

                return targetById;
            }

            var inputLower = input.ToLower();
            var exactMatch = BasePlayer.All.OfType<Player>()
                .FirstOrDefault(p => p.IsConnected && p.CharInfo.Username.ToLower() == inputLower);

            if (exactMatch != null)
                return exactMatch;

            var matches = BasePlayer.All.OfType<Player>()
                .Where(p => p.IsConnected && p.CharInfo.Username.Contains(input, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matches.Count == 0)
            {
                sender.SendClientMessage(Color.White, $"{Msg.Error} Tidak ada player dengan nama '{input}' yang ditemukan.");
                return null;
            }

            if (matches.Count == 1)
                return matches[0];

            sender.SendClientMessage(Color.White, $"{{FF6347}}[!]{{FFFFFF}} Ditemukan {matches.Count} player dengan nama mirip '{input}':");

            var displayCount = Math.Min(matches.Count, 10);
            for (var i = 0; i < displayCount; i++)
            {
                var match = matches[i];
                sender.SendClientMessage(Color.White, $"{{FF6347}}>{{FFFFFF}} {match.CharInfo.Username} {{c8c8c8}}(ID: {match.Id})");
            }

            if (matches.Count > 10)
                sender.SendClientMessage(Color.White, $"{{c8c8c8}}... dan {matches.Count - 10} player lainnya.");

            sender.SendClientMessage(Color.White, "{c8c8c8}Tip: Gunakan Player ID atau nama lengkap untuk target yang spesifik.");
            return null;
        }
    }
}