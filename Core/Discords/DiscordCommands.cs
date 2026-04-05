using Discord;
using Discord.Interactions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectSMP.Core.Discords
{
    public class DiscordCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [ComponentInteraction("btn_register")]
        public async Task HandleRegister()
        {
            var user = Context.User;
            var discordId = user.Id.ToString();

            var existing = await DatabaseManager.QueryFirstAsync<dynamic>(
                "SELECT ucp FROM player_ucp WHERE discordId = @DiscordId LIMIT 1",
                new { DiscordId = discordId });

            if (existing != null)
            {
                await RespondAsync(embed: DiscordEmbeds.BuildError("Already Registered",
                    $"Your Discord is already linked to UCP: **{existing.ucp}**"), ephemeral: true);
                return;
            }

            var modal = new ModalBuilder()
                .WithTitle("Register UCP - Prestige World")
                .WithCustomId("modal_register")
                .AddTextInput("UCP Name", "ucp_name", TextInputStyle.Short,
                    "Enter your desired UCP name", 3, 24)
                .Build();

            await RespondWithModalAsync(modal);
        }

        [ComponentInteraction("btn_resend")]
        public async Task HandleResend()
        {
            var user = Context.User;
            var discordId = user.Id.ToString();

            var ucpData = await DatabaseManager.QueryFirstAsync<dynamic>(
                "SELECT ucp, verifycode, password FROM player_ucp WHERE discordId = @DiscordId LIMIT 1",
                new { DiscordId = discordId });

            if (ucpData == null)
            {
                await RespondAsync(embed: DiscordEmbeds.BuildError(
                    "Not Found",
                    "Your Discord account is not linked to any UCP.\nPlease register first using the **Register** button."), ephemeral: true);
                return;
            }

            if (!string.IsNullOrEmpty(ucpData.password))
            {
                await RespondAsync(embed: DiscordEmbeds.BuildError(
                    "Already Activated",
                    $"Your UCP **{ucpData.ucp}** is already activated.\nYou don't need a verification pin anymore."), ephemeral: true);
                return;
            }

            var code = ucpData.verifycode.ToString();
            var config = DiscordService.GetConfig();

            try
            {
                var dmChannel = await user.CreateDMChannelAsync();
                var embed = DiscordEmbeds.BuildResendCode(ucpData.ucp, code, config);
                await dmChannel.SendMessageAsync(embed: embed);

                await RespondAsync(embed: DiscordEmbeds.BuildSuccess(
                    "PIN Resent!",
                    $"Your verification PIN for **{ucpData.ucp}** has been sent to your DM.\nPlease check your Direct Messages."), ephemeral: true);
            }
            catch
            {
                await RespondAsync(embed: DiscordEmbeds.BuildError(
                    "DM Failed",
                    "Cannot send you a DM. Please enable DMs from server members."), ephemeral: true);
            }
        }

        [ComponentInteraction("btn_chgpass")]
        public async Task HandleChangePassword()
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
                    "Your Discord account is not linked to any UCP.\nPlease register first using the **Register** button."), ephemeral: true);
                return;
            }

            if (string.IsNullOrEmpty(ucpData.password))
            {
                await RespondAsync(embed: DiscordEmbeds.BuildError(
                    "Account Not Activated",
                    $"Your UCP **{ucpData.ucp}** is not activated yet.\nPlease activate your account in-game first."), ephemeral: true);
                return;
            }

            var modal = new ModalBuilder()
                .WithTitle("Change Password - Prestige World")
                .WithCustomId("modal_changepass")
                .AddTextInput("New Password", "new_password", TextInputStyle.Short,
                    "Enter your new password", 6, 32)
                .AddTextInput("Confirm New Password", "confirm_password", TextInputStyle.Short,
                    "Re-enter your new password", 6, 32)
                .Build();

            await RespondWithModalAsync(modal);
        }

        [ComponentInteraction("btn_reverif")]
        public async Task HandleReverif()
        {
            var user = Context.User;
            var discordId = user.Id.ToString();

            var ucpData = await DatabaseManager.QueryFirstAsync<dynamic>(
                "SELECT ucp FROM player_ucp WHERE discordId = @DiscordId LIMIT 1",
                new { DiscordId = discordId });

            if (ucpData == null)
            {
                await RespondAsync(embed: DiscordEmbeds.BuildError(
                    "No UCP Found",
                    "Your Discord account is not linked to any UCP.\nPlease register first using the **Register** button."), ephemeral: true);
                return;
            }

            var config = DiscordService.GetConfig();

            if (config.ReverifRoleId == 0)
            {
                await RespondAsync(embed: DiscordEmbeds.BuildError(
                    "Configuration Error",
                    "Reverification role is not configured. Please contact server administrator."), ephemeral: true);
                return;
            }

            try
            {
                var guild = Context.Guild;
                if (guild == null)
                {
                    await RespondAsync(embed: DiscordEmbeds.BuildError(
                        "Error",
                        "This command must be used in a server."), ephemeral: true);
                    return;
                }

                var guildUser = guild.GetUser(user.Id);
                if (guildUser == null)
                {
                    await RespondAsync(embed: DiscordEmbeds.BuildError(
                        "Error",
                        "Cannot find your user profile in this server."), ephemeral: true);
                    return;
                }

                var role = guild.GetRole(config.ReverifRoleId);
                if (role == null)
                {
                    await RespondAsync(embed: DiscordEmbeds.BuildError(
                        "Configuration Error",
                        "Reverification role not found. Please contact server administrator."), ephemeral: true);
                    return;
                }

                if (guildUser.Roles.Any(r => r.Id == config.ReverifRoleId))
                {
                    await RespondAsync(embed: DiscordEmbeds.BuildInfo(
                        "Already Verified",
                        $"You already have the verification role: **{role.Name}**"), ephemeral: true);
                    return;
                }

                await guildUser.AddRoleAsync(role);

                await RespondAsync(embed: DiscordEmbeds.BuildSuccess(
                    "Reverification Successful!",
                    $"You have been granted the **{role.Name}** role.\nYour UCP: **{ucpData.ucp}**"), ephemeral: true);

                Console.WriteLine($"[+] Discord - Reverif: {user.Username} ({user.Id}) -> UCP: {ucpData.ucp}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[+] Discord - Reverif error: {ex.Message}");
                await RespondAsync(embed: DiscordEmbeds.BuildError(
                    "Permission Error",
                    "Bot doesn't have permission to assign roles. Please contact server administrator."), ephemeral: true);
            }
        }
    }
}