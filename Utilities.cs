using ProjectSMP.Core;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProjectSMP
{
    public static class Utilities
    {
        private static readonly Random Random = new();
        public static int RandomEx(int min, int max) {
            return Random.Next(min, max + 1);
        }

        public static string GroupDigits(int value, bool showCurrency = true)
        {
            var isNegative = value < 0;
            var absValue = Math.Abs(value);
            var dollars = absValue / 100;
            var cents = absValue % 100;

            var formatted = dollars.ToString("N0").Replace(",", ".");
            var result = $"{formatted},{cents:D2}";

            if (isNegative) result = "-" + result;
            if (showCurrency) result = "$" + result;

            return result;
        }

        public static string ReturnName(Player player)
        {
            if (player.AdminOnDuty)
                return $"{player.Ucp}";

            if (player.MaskActive)
                return $"Mask_#{player.MaskId}";

            return player.CharInfo.Username.Replace('_', ' ');
        }

        public static string ReturnNameEx(Player player)
        {
            return player.CharInfo.Username;
        }

        public static bool NearPlayer(BasePlayer player, BasePlayer target, float radius)
        {
            if (player.Interior != target.Interior || player.VirtualWorld != target.VirtualWorld)
                return false;

            return player.Position.DistanceTo(target.Position) <= radius;
        }

        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        public static string GenerateRandomNumber(int length)
        {
            return new string(Enumerable.Repeat("0123456789", length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        public static string FormatBankRekening(string input)
        {
            if (input.Length != 11) return input;
            return $"{input.Substring(0, 4)}-{input.Substring(4, 3)}-{input.Substring(7, 4)}";
        }

        public static string GetAdminString(Player player)
        {
            var levels = new[] {
                "{808080}None{ffffff}",
                "{00FF00}Helper{ffffff}",
                "{0000FF}Moderator{ffffff}",
                "{FFA500}Admin{ffffff}",
                "{FF4500}High Admin{ffffff}",
                "{FF0000}Senior Admin{ffffff}"
            };
            var level = Math.Clamp(player.Admin, 0, 5);
            return $"{levels[level]}({{ebeb00}}{level}{{ffffff}})";
        }

        public static string GetAdminStringChat(Player player)
        {
            var levels = new[] { "None", "Helper", "Moderator", "Admin", "High Admin", "Senior Admin" };
            return levels[Math.Clamp(player.Admin, 0, 5)];
        }

        public static string GetWarningString(Player player)
        {
            var color = player.Warn switch
            {
                0 => "00FF00",
                <= 5 => "7FFF00",
                <= 10 => "FFFF00",
                <= 15 => "FFA500",
                _ => "FF0000"
            };
            return $"{{{color}}}{player.Warn}{{ffffff}}/{{FF0000}}20{{ffffff}}";
        }

        public static string ColouredText(string text)
        {
            return Regex.Replace(text, @"#([0-9A-Fa-f]{6})", "{$1}");
        }

        public static void SendStaffMessage(int color, string message, params object[] args)
        {
            var formatted = string.Format(message, args);
            foreach (var player in BasePlayer.All.OfType<Player>())
            {
                if (player.Admin >= 1)
                    player.SendClientMessage(color, formatted);
            }
        }

        public static Player GetPlayerFromPartOfName(Player sender, string input)
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

        public static void KickEx(BasePlayer player, int delayMs = 1000)
        {
            var timer = new SampSharp.GameMode.SAMP.Timer(delayMs, false);
            timer.Tick += (s, e) => { player.Kick(); timer.Dispose(); };
        }
    }
}