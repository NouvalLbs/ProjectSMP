using ProjectSMP.Core;
using ProjectSMP.Features.Bank.Data;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectSMP.Features.Bank
{
    internal static class BankService
    {
        private const string Table = "players_banks";

        public static async Task LoadAsync(Player player)
        {
            var rows = await DatabaseManager.QueryAsync<PlayerBankAccount>(
                $"SELECT id AS Id, CitizenId, account_number AS AccountNumber, " +
                $"account_name AS AccountName, creation_date AS CreationDate, " +
                $"last_transaction AS LastTransaction, balance AS Balance, is_active AS IsActive " +
                $"FROM `{Table}` WHERE CitizenId = @CitizenId",
                new { player.CitizenId });

            if (player.IsDisposed) return;
            player.BankAccounts = new List<PlayerBankAccount>(rows);
        }

        public static async Task SaveAccountAsync(PlayerBankAccount account)
        {
            await DatabaseManager.ExecuteAsync(
                $"UPDATE `{Table}` SET balance=@Balance, last_transaction=@LastTransaction, " +
                $"is_active=@IsActive WHERE id=@Id",
                new { account.Balance, account.LastTransaction, account.IsActive, account.Id });
        }

        public static PlayerBankAccount GetAccount(Player player, int index)
        {
            if (index < 0 || index >= player.BankAccounts.Count)
                return null;
            return player.BankAccounts[index];
        }

        public static PlayerBankAccount GetAccountByNumber(Player player, string accountNumber)
        {
            return player.BankAccounts.FirstOrDefault(a =>
                a.AccountNumber.Equals(accountNumber, StringComparison.OrdinalIgnoreCase));
        }

        public static int GetAccountCount(Player player)
        {
            return player.BankAccounts.Count(a => a.IsActive);
        }

        public static async Task<bool> CreateAccountAsync(Player player, string accountName)
        {
            if (GetAccountCount(player) >= 5)
                return false;

            var accountNumber = GenerateAccountNumber();
            var now = DateTime.Now.ToString("yyyy-MM-dd");

            var newAccount = new PlayerBankAccount
            {
                CitizenId = player.CitizenId,
                AccountNumber = accountNumber,
                AccountName = accountName,
                CreationDate = now,
                LastTransaction = now,
                Balance = 0,
                IsActive = true
            };

            await DatabaseManager.ExecuteAsync(
                $"INSERT INTO `{Table}` (CitizenId, account_number, account_name, creation_date, last_transaction, balance, is_active) " +
                "VALUES (@CitizenId, @AccountNumber, @AccountName, @CreationDate, @LastTransaction, 0, 1)",
                new
                {
                    newAccount.CitizenId,
                    newAccount.AccountNumber,
                    newAccount.AccountName,
                    newAccount.CreationDate,
                    newAccount.LastTransaction
                });

            if (player.IsDisposed) return false;

            newAccount.Id = await DatabaseManager.ExecuteScalarAsync<int>(
                "SELECT LAST_INSERT_ID()");

            player.BankAccounts.Add(newAccount);
            return true;
        }

        public static bool Deposit(Player player, PlayerBankAccount account, int amount)
        {
            if (!account.IsActive || amount <= 0) return false;
            if (player.CharMoney < amount) return false;

            account.Balance += amount;
            player.CharMoney -= amount;
            UpdateTransactionDate(account);
            _ = SaveAccountAsync(account);

            return true;
        }

        public static bool Withdraw(Player player, PlayerBankAccount account, int amount)
        {
            if (!account.IsActive || amount <= 0) return false;
            if (account.Balance < amount) return false;

            account.Balance -= amount;
            player.CharMoney += amount;
            UpdateTransactionDate(account);
            _ = SaveAccountAsync(account);

            return true;
        }

        public static async Task<bool> TransferAsync(Player sender, PlayerBankAccount senderAccount,
            string targetAccountNumber, int amount)
        {
            if (!senderAccount.IsActive || amount <= 0)
                return false;

            if (senderAccount.Balance < amount)
                return false;

            var selfAccount = GetAccountByNumber(sender, targetAccountNumber);
            if (selfAccount != null)
            {
                senderAccount.Balance -= amount;
                selfAccount.Balance += amount;
                UpdateTransactionDate(senderAccount);
                UpdateTransactionDate(selfAccount);

                await SaveAccountAsync(senderAccount);
                await SaveAccountAsync(selfAccount);

                sender.SendClientMessage(Color.White,
                    $"{Msg.Bank} Transfer {{00FF00}}{Utilities.GroupDigits(amount)}{{FFFFFF}} dari {senderAccount.AccountName} ke {selfAccount.AccountName} berhasil!");
                return true;
            }

            var targetData = await DatabaseManager.QueryFirstAsync<dynamic>(
                "SELECT b.*, p.ID FROM players_banks b " +
                "JOIN players p ON b.CitizenId = p.citizenId " +
                "WHERE b.account_number = @AccountNumber AND b.is_active = 1",
                new { AccountNumber = targetAccountNumber });

            if (targetData == null)
            {
                sender.SendClientMessage(Color.White, $"{Msg.Error} Akun tujuan tidak ditemukan!");
                return false;
            }

            senderAccount.Balance -= amount;
            UpdateTransactionDate(senderAccount);

            await DatabaseManager.ExecuteAsync(
                $"UPDATE `{Table}` SET balance = balance - @Amount, last_transaction = CURDATE() " +
                "WHERE account_number = @AccountNumber AND CitizenId = @CitizenId",
                new { Amount = amount, senderAccount.AccountNumber, sender.CitizenId });

            await DatabaseManager.ExecuteAsync(
                $"UPDATE `{Table}` SET balance = balance + @Amount, last_transaction = CURDATE() " +
                "WHERE account_number = @AccountNumber",
                new { Amount = amount, AccountNumber = targetAccountNumber });

            sender.SendClientMessage(Color.White,
                $"{Msg.Bank} Transfer {{00FF00}}{Utilities.GroupDigits(amount)}{{FFFFFF}} ke {targetAccountNumber} berhasil! Saldo: {{00FF00}}{Utilities.GroupDigits(senderAccount.Balance)}{{FFFFFF}}");

            int targetPlayerId = targetData.ID;
            var target = BasePlayer.Find(targetPlayerId) as Player;
            if (target != null && target.IsConnected)
            {
                target.SendClientMessage(Color.White,
                    $"{Msg.Bank} Kamu menerima transfer {{00FF00}}{Utilities.GroupDigits(amount)}{{FFFFFF}} dari {senderAccount.AccountNumber}.");
            }

            return true;
        }

        public static async Task<bool> CloseAccountAsync(Player player, PlayerBankAccount account)
        {
            if (!account.IsActive)
                return false;

            if (account.Balance > 0)
            {
                player.SendClientMessage(Color.White,
                    $"{Msg.Error} Kamu masih memiliki saldo {{00FF00}}{Utilities.GroupDigits(account.Balance)}{{FFFFFF}}. Tarik semua uang sebelum menutup akun!");
                return false;
            }

            account.IsActive = false;

            await DatabaseManager.ExecuteAsync(
                $"UPDATE `{Table}` SET is_active = 0 WHERE id = @Id",
                new { account.Id });

            player.SendClientMessage(Color.White, $"{Msg.Bank} Akun bank berhasil ditutup!");
            return true;
        }

        public static void UpdateTransactionDate(PlayerBankAccount account)
        {
            account.LastTransaction = DateTime.Now.ToString("yyyy-MM-dd");
        }

        private static string GenerateAccountNumber()
        {
            var now = DateTime.Now;
            var year = now.Year % 100;
            var month = now.Month;
            var day = now.Day;

            var rand = new Random().Next(100000, 1000000);

            return $"{year:D2}{month:D2}{day:D2}{rand:D6}";
        }
    }
}