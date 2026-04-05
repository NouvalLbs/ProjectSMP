#nullable enable
using ProjectSMP.Core;
using ProjectSMP.Entities.Players.Account.Data;
using ProjectSMP.Entities.Players.Character;
using SampSharp.GameMode.Definitions;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectSMP.Entities.Players.Account
{
    internal static class UserControlService
    {
        private const string Table = "player_ucp";
        private const int MaxAttempts = 3;
        private const int LoginTimeoutMs = 60_000;
        private const int InitDelayMs = 1_500;

        private static readonly Dictionary<int, PlayerUcpData> _sessions = new();
        private static readonly Dictionary<int, CancellationTokenSource> _kickTimers = new();
        private static readonly Regex _passwordPattern = new(@"^[A-Za-z0-9\[\]()\._@#]+$", RegexOptions.Compiled);

        private static string L(string s, string k) => LocalizationManager.Get(Language.ID, s, k);
        private static string L(string s, string k, params object[] a) => LocalizationManager.Get(Language.ID, s, k, a);

        public static async void InitAsync(Player player)
        {
            try
            {
                await Task.Delay(InitDelayMs);
                if (player.IsDisposed) return;

                ResetSession(player);

                if (player.Name.Contains('_'))
                {
                    ShowInvalidNameDialog(player);
                    ScheduleKick(player, 3_000);
                    return;
                }

                var data = await DatabaseManager.QueryFirstAsync<PlayerUcpData>(
                    $"SELECT `ucp` AS UCP, `password` AS Password, " +
                    $"`discordId` AS DiscordId, `verifycode` AS VerifyCode " +
                    $"FROM `{Table}` WHERE `ucp` = @Ucp LIMIT 1",
                    new { Ucp = player.Name });

                if (player.IsDisposed) return;

                if (data is null)
                {
                    ShowNotRegisteredDialog(player);
                    ScheduleKick(player, 1_000);
                    return;
                }

                var session = _sessions[player.Id];
                session.UCP = data.UCP;
                session.Password = data.Password;
                session.DiscordId = data.DiscordId;
                session.VerifyCode = data.VerifyCode;
                session.LoginAttempt = MaxAttempts;

                ScheduleKick(player, LoginTimeoutMs);

                if (string.IsNullOrEmpty(data.Password))
                    ShowActivateDialog(player);
                else
                    ShowLoginDialog(player);
            }
            catch (Exception ex) { Console.WriteLine($"[UserControl] InitAsync: {ex.Message}"); }
        }

        public static void Cleanup(Player player)
        {
            _sessions.Remove(player.Id);
            CancelKickTimer(player);
        }

        public static PlayerUcpData? GetSession(Player player)
            => _sessions.TryGetValue(player.Id, out var s) ? s : null;

        private static void ShowInvalidNameDialog(Player player)
        {
            player.ShowMessage(
                L("AUTH", "INVALID_NAME_TITLE"),
                L("AUTH", "INVALID_NAME_MSG", player.Name, player.ClientType, player.ClientVersion))
                .WithButtons(L("GENERAL", "BTN_OK"))
                .Show();
        }

        private static void ShowNotRegisteredDialog(Player player)
        {
            player.ShowMessage(
                L("AUTH", "NOT_REGISTERED_TITLE"),
                L("AUTH", "NOT_REGISTERED_MSG", player.Name, player.ClientType, player.ClientVersion))
                .WithButtons(L("GENERAL", "BTN_OK"))
                .Show();
        }

        private static void ShowActivateDialog(Player player, string? errKey = null)
        {
            var session = _sessions[player.Id];
            var body = L("AUTH", "ACTIVATE_MSG", session.UCP, player.ClientType, player.ClientVersion);
            if (errKey != null) body += $"\n{{FF0000}}" + L("AUTH", errKey);

            player.ShowInput(L("AUTH", "ACTIVATE_TITLE"), body)
                .WithButtons(L("GENERAL", "BTN_INPUT"), L("GENERAL", "BTN_CANCEL"))
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) { KickDelayed(player); return; }
                    HandleActivate(player, e.InputText);
                });
        }

        private static void ShowRegisterDialog(Player player, string? errorType = null)
        {
            var msgKey = errorType switch
            {
                null => "REGISTER_MSG",
                "short" => "REGISTER_MSG_SHORT",
                _ => "REGISTER_MSG_INVALID"
            };

            player.ShowInput(L("AUTH", "REGISTER_TITLE"), L("AUTH", msgKey))
                .AsPassword()
                .WithButtons(L("GENERAL", "BTN_REGISTER"), L("GENERAL", "BTN_ABORT"))
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) { KickDelayed(player); return; }
                    HandleRegisterAsync(player, e.InputText);
                });
        }

        private static void ShowLoginDialog(Player player)
        {
            var session = _sessions[player.Id];

            player.ShowInput(
                L("AUTH", "LOGIN_TITLE"),
                L("AUTH", "LOGIN_MSG", session.UCP, player.ClientType, player.ClientVersion, session.LoginAttempt))
                .AsPassword()
                .WithButtons(L("GENERAL", "BTN_LOGIN"), L("GENERAL", "BTN_ABORT"))
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) { KickDelayed(player); return; }
                    HandleLogin(player, e.InputText);
                });
        }

        private static void HandleActivate(Player player, string pin)
        {
            var session = GetSession(player);
            if (session is null) return;

            if (string.IsNullOrWhiteSpace(pin) || !int.TryParse(pin, out var pinValue))
            { ShowActivateDialog(player, "ACTIVATE_ERR_PIN_INVALID"); return; }

            if (pinValue != session.VerifyCode)
            { ShowActivateDialog(player, "ACTIVATE_ERR_PIN_WRONG"); return; }

            ShowRegisterDialog(player);
        }

        private static async void HandleRegisterAsync(Player player, string password)
        {
            var session = GetSession(player);
            if (session is null) return;

            if (password.Length < 6) { ShowRegisterDialog(player, "short"); return; }
            if (!_passwordPattern.IsMatch(password)) { ShowRegisterDialog(player, "invalid"); return; }

            var hashed = HashPassword(password);

            await DatabaseManager.ExecuteAsync(
                $"UPDATE `{Table}` SET `password` = @Password WHERE `ucp` = @Ucp",
                new { Password = hashed, Ucp = session.UCP });

            if (player.IsDisposed) return;

            session.Password = hashed;
            CancelKickTimer(player);
            CharacterService.CheckPlayerCharAsync(player);
        }

        private static void HandleLogin(Player player, string password)
        {
            var session = GetSession(player);
            if (session is null) return;

            if (HashPassword(password) != session.Password)
            {
                session.LoginAttempt--;
                if (session.LoginAttempt <= 0) { KickDelayed(player); return; }
                ShowLoginDialog(player);
                return;
            }

            CancelKickTimer(player);
            CharacterService.CheckPlayerCharAsync(player);
        }

        private static void ResetSession(Player player)
        {
            _sessions[player.Id] = new PlayerUcpData();
            CancelKickTimer(player);
        }

        private static void ScheduleKick(Player player, int delayMs)
        {
            CancelKickTimer(player);
            var cts = new CancellationTokenSource();
            _kickTimers[player.Id] = cts;
            KickAfterAsync(player, delayMs, cts.Token);
        }

        private static async void KickAfterAsync(Player player, int delayMs, CancellationToken ct)
        {
            try { await Task.Delay(delayMs, ct); if (!player.IsDisposed) player.Kick(); }
            catch (OperationCanceledException) { }
        }

        private static void KickDelayed(Player player)
        {
            CancelKickTimer(player);
            ScheduleKick(player, 200);
        }

        private static void CancelKickTimer(Player player)
        {
            if (!_kickTimers.TryGetValue(player.Id, out var cts)) return;
            cts.Cancel();
            _kickTimers.Remove(player.Id);
        }

        private static string HashPassword(string raw)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            var sb = new StringBuilder(64);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}