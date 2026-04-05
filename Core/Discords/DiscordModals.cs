using Discord;
using Discord.Interactions;
using System;
using System.Threading.Tasks;

namespace ProjectSMP.Core.Discords
{
    public class DiscordModals : InteractionModuleBase<SocketInteractionContext>
    {
        [ModalInteraction("modal_register")]
        public async Task HandleRegisterModal(RegisterModal modal)
        {
            var user = Context.User;
            var discordId = user.Id.ToString();
            var ucpName = modal.UcpName.Trim();

            if (string.IsNullOrWhiteSpace(ucpName) || ucpName.Length < 3 || ucpName.Length > 24)
            {
                await RespondAsync(embed: DiscordEmbeds.BuildError(
                    "Invalid UCP Name",
                    "UCP Name must be between 3-24 characters."), ephemeral: true);
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(ucpName, "^[A-Za-z0-9]+$"))
            {
                await RespondAsync(embed: DiscordEmbeds.BuildError(
                    "Invalid UCP Name",
                    "UCP Name can only contain letters and numbers.\nNo spaces, underscores, or special characters allowed."), ephemeral: true);
                return;
            }

            var existing = await DatabaseManager.QueryFirstAsync<dynamic>(
                "SELECT ucp FROM player_ucp WHERE discordId = @DiscordId LIMIT 1",
                new { DiscordId = discordId });

            if (existing != null)
            {
                await RespondAsync(embed: DiscordEmbeds.BuildError(
                    "Already Registered",
                    $"Your Discord is already linked to UCP: **{existing.ucp}**"), ephemeral: true);
                return;
            }

            var ucpExists = await DatabaseManager.ExistsAsync(
                "SELECT COUNT(*) FROM player_ucp WHERE ucp = @Ucp",
                new { Ucp = ucpName });

            if (ucpExists)
            {
                await RespondAsync(embed: DiscordEmbeds.BuildError(
                    "UCP Name Taken",
                    $"UCP Name **{ucpName}** is already registered.\nPlease choose another name."), ephemeral: true);
                return;
            }

            var verifyCode = new Random().Next(100000, 999999);

            await DatabaseManager.ExecuteAsync(
                "INSERT INTO player_ucp (ucp, password, discordId, verifycode) VALUES (@Ucp, '', @DiscordId, @VerifyCode)",
                new { Ucp = ucpName, DiscordId = discordId, VerifyCode = verifyCode });

            try
            {
                var dmChannel = await user.CreateDMChannelAsync();
                var config = DiscordService.GetConfig();
                var embed = DiscordEmbeds.BuildVerificationCode(ucpName, verifyCode.ToString(), config);
                await dmChannel.SendMessageAsync(embed: embed);

                await RespondAsync(embed: DiscordEmbeds.BuildSuccess(
                    "Registration Successful!",
                    $"Your UCP **{ucpName}** has been created.\nCheck your DM for the verification PIN!"), ephemeral: true);

                Console.WriteLine($"[+] Discord - New UCP registered: {ucpName} (Discord: {user.Id})");
            }
            catch
            {
                await DatabaseManager.ExecuteAsync(
                    "DELETE FROM player_ucp WHERE ucp = @Ucp",
                    new { Ucp = ucpName });

                await RespondAsync(embed: DiscordEmbeds.BuildError(
                    "DM Failed",
                    "Cannot send you a DM. Please enable DMs from server members and try again."), ephemeral: true);
            }
        }

        [ModalInteraction("modal_changepass")]
        public async Task HandleChangePasswordModal(ChangePasswordModal modal)
        {
            var user = Context.User;
            var discordId = user.Id.ToString();

            var ucpData = await DatabaseManager.QueryFirstAsync<dynamic>(
                "SELECT ucp, password FROM player_ucp WHERE discordId = @DiscordId LIMIT 1",
                new { DiscordId = discordId });

            if (ucpData == null)
            {
                await RespondAsync(embed: DiscordEmbeds.BuildError(
                    "No UCP Found",
                    "Your Discord account is not linked to any UCP.\nPlease register first."), ephemeral: true);
                return;
            }

            if (string.IsNullOrEmpty(ucpData.password))
            {
                await RespondAsync(embed: DiscordEmbeds.BuildError(
                    "Account Not Activated",
                    $"Your UCP **{ucpData.ucp}** is not activated yet.\nPlease activate your account in-game first."), ephemeral: true);
                return;
            }

            var newPass = modal.NewPassword.Trim();
            var confirmPass = modal.ConfirmPassword.Trim();

            if (newPass != confirmPass)
            {
                await RespondAsync(embed: DiscordEmbeds.BuildError(
                    "Password Mismatch",
                    "New password and confirmation password do not match.\nPlease try again."), ephemeral: true);
                return;
            }

            if (newPass.Length < 6 || newPass.Length > 32)
            {
                await RespondAsync(embed: DiscordEmbeds.BuildError(
                    "Invalid Password",
                    "Password must be between 6-32 characters."), ephemeral: true);
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(newPass, @"^[A-Za-z0-9\[\]()\._@#]+$"))
            {
                await RespondAsync(embed: DiscordEmbeds.BuildError(
                    "Invalid Password",
                    "Password can only contain:\nLetters (A-Z, a-z), Numbers (0-9), and symbols: _ [ ] ( ) . @ #"), ephemeral: true);
                return;
            }

            var hashedPassword = HashPassword(newPass);

            await DatabaseManager.ExecuteAsync(
                "UPDATE player_ucp SET password = @Password WHERE discordId = @DiscordId",
                new { Password = hashedPassword, DiscordId = discordId });

            try
            {
                var config = DiscordService.GetConfig();
                var dmChannel = await user.CreateDMChannelAsync();
                var embed = DiscordEmbeds.BuildChangePassword(ucpData.ucp, newPass, config);
                await dmChannel.SendMessageAsync(embed: embed);

                await RespondAsync(embed: DiscordEmbeds.BuildSuccess(
                    "Password Changed!",
                    $"Your password for **{ucpData.ucp}** has been updated successfully.\nCheck your DM for details."), ephemeral: true);

                Console.WriteLine($"[+] Discord - Password changed: {ucpData.ucp} (Discord: {user.Id})");
            }
            catch
            {
                await RespondAsync(embed: DiscordEmbeds.BuildSuccess(
                    "Password Changed!",
                    $"Your password for **{ucpData.ucp}** has been updated successfully.\n(Could not send DM - please enable DMs)"), ephemeral: true);
            }
        }

        private static string HashPassword(string raw)
        {
            var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(raw));
            var sb = new System.Text.StringBuilder(64);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }

    public class RegisterModal : IModal
    {
        public string Title => "Register UCP - Prestige World";

        [InputLabel("UCP Name")]
        [ModalTextInput("ucp_name", TextInputStyle.Short, "Enter your desired UCP name", 3, 24)]
        public string UcpName { get; set; }
    }

    public class ChangePasswordModal : IModal
    {
        public string Title => "Change Password - Prestige World";

        [InputLabel("New Password")]
        [ModalTextInput("new_password", TextInputStyle.Short, "Enter your new password", 6, 32)]
        public string NewPassword { get; set; }

        [InputLabel("Confirm New Password")]
        [ModalTextInput("confirm_password", TextInputStyle.Short, "Re-enter your new password", 6, 32)]
        public string ConfirmPassword { get; set; }
    }
}