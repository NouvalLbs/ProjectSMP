using ProjectSMP.Core;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectSMP.Features.Bank.Paycheck
{
    public static class PaycheckService
    {
        private const int ClaimInterval = 3600;
        private static Timer _timer;
        private static readonly HashSet<int> _players = new();
        private const double IncomeTaxRate = 0.05;
        private const int RoadTax = 300;

        private static int CalculateInterest(int balance)
        {
            double rate = balance switch
            {
                <= 2_500_000 => 0.004,
                <= 10_000_000 => 0.0025,
                <= 25_000_000 => 0.0015,
                _ => 0.0005
            };
            return (int)Math.Round(balance * rate);
        }

        public static void Initialize()
        {
            _timer = new Timer(1000, true);
            _timer.Tick += OnTick;
        }

        public static void Dispose() => _timer?.Dispose();

        public static void RegisterPlayer(Player player) => _players.Add(player.Id);

        public static void UnregisterPlayer(Player player) => _players.Remove(player.Id);

        private static void OnTick(object sender, EventArgs e)
        {
            foreach (var id in _players.ToList())
            {
                var player = BasePlayer.Find(id) as Player;
                if (player == null || !player.IsCharLoaded) continue;
                player.PaycheckData.PaycheckTime++;
            }
        }

        public static void GivePaycheck(Player player, int amount, string from)
        {
            player.PaycheckData.PaycheckList.Add(new PaycheckEntry
            {
                Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                From = from,
                Amount = amount
            });
        }

        public static bool CanClaim(Player player) => player.PaycheckData.PaycheckTime >= ClaimInterval;

        public static int GetTotal(Player player) => player.PaycheckData.PaycheckList.Sum(e => e.Amount);

        public static string GetTimeLeft(Player player)
        {
            var rem = Math.Max(0, ClaimInterval - player.PaycheckData.PaycheckTime);
            return $"{rem / 3600:D2}:{rem % 3600 / 60:D2}:{rem % 60:D2}";
        }

        public static bool ClaimPaycheck(Player player)
        {
            if (!CanClaim(player)) return false;
            var gross = GetTotal(player);
            if (gross <= 0) return false;

            var account = player.BankAccounts.FirstOrDefault(a => a.IsActive);
            if (account == null) return false;

            var prevBalance = account.Balance;
            var interest = CalculateInterest(prevBalance);
            var incomeTax = (int)Math.Round(gross * IncomeTaxRate);
            var net = gross - incomeTax - RoadTax + interest;

            account.Balance += net;
            BankService.UpdateTransactionDate(account);
            _ = BankService.SaveAccountAsync(account);

            player.PaycheckData.PaycheckNumber++;
            var num = player.PaycheckData.PaycheckNumber;

            var sep = "_________________";
            player.SendClientMessage(Color.White, $"{sep} {{FFFF00}}San Andreas Bank Paycheck #{num} {{FFFFFF}}{sep}");
            player.SendClientMessage(Color.White, $"{{FFFFFF}}Previous Balance: {{00FF00}}{Utilities.GroupDigits(prevBalance)}");
            player.SendClientMessage(Color.White, $"{{FFFFFF}}Bank Interest: {{00FF00}}{Utilities.GroupDigits(interest)}");
            player.SendClientMessage(Color.White, $"{{FFFFFF}}Income Balance: {{00FF00}}{Utilities.GroupDigits(gross)}");
            player.SendClientMessage(Color.White, $"{{FFFFFF}}Income Tax: {{FF0000}}-{Utilities.GroupDigits(incomeTax)}");
            player.SendClientMessage(Color.White, $"{{FFFFFF}}Road Tax: {{FF0000}}-{Utilities.GroupDigits(RoadTax)}");
            player.SendClientMessage(Color.White, $"{{FFFFFF}}New Balance: {{00FF00}}{Utilities.GroupDigits(account.Balance)}");

            player.PaycheckData.PaycheckList.Clear();
            player.PaycheckData.PaycheckTime = 0;
            return true;
        }
    }
}