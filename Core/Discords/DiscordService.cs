using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using ProjectSMP.Core.Discords.Models;
using SampSharp.GameMode.SAMP;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;

namespace ProjectSMP.Core.Discords
{
    public static class DiscordService
    {
        private static DiscordSocketClient _client;
        private static InteractionService _interactions;
        private static Timer _responseTimer;
        private static bool _isRunning;
        private static DiscordConfigs _config;
        private const string ConfigPath = "scriptfiles/DiscordConfig.json";

        public static async Task InitializeAsync()
        {
            _config = await LoadOrCreateConfig();
            if (string.IsNullOrEmpty(_config.Token))
            {
                Console.WriteLine("[+] Discord - Token not configured in DiscordConfig.json");
                return;
            }

            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers,
                AlwaysDownloadUsers = true
            };

            _client = new DiscordSocketClient(config);
            _interactions = new InteractionService(_client);

            _client.Log += Log;
            _client.Ready += OnReady;
            _client.InteractionCreated += HandleInteraction;

            await _interactions.AddModuleAsync<DiscordCommands>(null);
            await _interactions.AddModuleAsync<DiscordModals>(null);

            await _client.LoginAsync(TokenType.Bot, _config.Token);
            await _client.StartAsync();

            _responseTimer = new Timer(3000, true);
            _responseTimer.Tick += ProcessResponses;

            _isRunning = true;
            Console.WriteLine("[+] Discord - Service initialized");
        }

        public static async Task ShutdownAsync()
        {
            _isRunning = false;
            _responseTimer?.Dispose();

            if (_client != null)
            {
                await _client.LogoutAsync();
                await _client.StopAsync();
                _client.Dispose();
            }

            Console.WriteLine("[+] Discord - Service stopped");
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine($"[+] Discord - {msg}");
            return Task.CompletedTask;
        }

        private static async Task OnReady()
        {
            try
            {
                await _interactions.RegisterCommandsToGuildAsync(_config.GuildId);
                Console.WriteLine($"[+] Discord - Bot ready! Logged in as {_client.CurrentUser.Username}");

                if (_config.AutoCreateUcpPanel)
                    await EnsureUcpPanel();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[+] Discord - Error on ready: {ex.Message}");
            }
        }

        private static async Task HandleInteraction(SocketInteraction interaction)
        {
            try
            {
                var context = new SocketInteractionContext(_client, interaction);
                await _interactions.ExecuteCommandAsync(context, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[+] Discord - Error handling interaction: {ex.Message}");

                if (interaction.Type == InteractionType.ApplicationCommand)
                    await interaction.RespondAsync($"Error: {ex.Message}", ephemeral: true);
            }
        }

        private static async void ProcessResponses(object sender, EventArgs e)
        {
            if (!_isRunning) return;

            try
            {
                var responses = await DatabaseManager.QueryAsync<DiscordResponse>(
                    "SELECT * FROM discord_responses WHERE processed = 0 LIMIT 10");

                foreach (var response in responses)
                {
                    await SendResponse(response);

                    await DatabaseManager.ExecuteAsync(
                        "UPDATE discord_responses SET processed = 1 WHERE id = @Id",
                        new { response.Id });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[+] Discord - Error processing responses: {ex.Message}");
            }
        }

        private static async Task SendResponse(DiscordResponse response)
        {
            try
            {
                var user = await _client.GetUserAsync(response.UserId);
                if (user == null) return;

                if (!string.IsNullOrEmpty(response.EmbedData))
                {
                    var embedBuilder = new EmbedBuilder()
                        .WithDescription(response.Message)
                        .WithColor(Discord.Color.Blue)
                        .WithCurrentTimestamp();

                    await user.SendMessageAsync(embed: embedBuilder.Build());
                }
                else
                {
                    await user.SendMessageAsync(response.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[+] Discord - Error sending response to {response.UserId}: {ex.Message}");
            }
        }

        public static async Task SendDMAsync(ulong userId, string message, Embed embed = null)
        {
            try
            {
                var user = await _client.GetUserAsync(userId);
                if (user == null) return;

                if (embed != null)
                    await user.SendMessageAsync(message, embed: embed);
                else
                    await user.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[+] Discord - Error sending DM: {ex.Message}");
            }
        }

        private static async Task EnsureUcpPanel()
        {
            try
            {
                if (_config.UcpPanelChannelId == 0)
                {
                    Console.WriteLine("[+] Discord - UCP Panel channel ID not configured");
                    return;
                }

                var channel = await _client.GetChannelAsync(_config.UcpPanelChannelId) as ITextChannel;
                if (channel == null)
                {
                    Console.WriteLine($"[+] Discord - UCP Panel channel {_config.UcpPanelChannelId} not found");
                    return;
                }

                if (_config.UcpPanelMessageId > 0)
                {
                    try
                    {
                        var existingMsg = await channel.GetMessageAsync(_config.UcpPanelMessageId);
                        if (existingMsg != null)
                        {
                            Console.WriteLine($"[+] Discord - UCP Panel already exists (Message ID: {_config.UcpPanelMessageId})");
                            return;
                        }
                    }
                    catch
                    {
                        Console.WriteLine("[+] Discord - Previous UCP Panel message not found, creating new one");
                    }
                }

                var embed = DiscordEmbeds.BuildUCPPanel(_config);
                var component = new ComponentBuilder()
                    .WithButton("📝 Register", "btn_register", ButtonStyle.Primary, row: 0)
                    .WithButton("🔑 Resend Code", "btn_resend", ButtonStyle.Secondary, row: 0)
                    .WithButton("📑 Reverif", "btn_reverif", ButtonStyle.Success, row: 0)
                    .WithButton("📌 Change Password", "btn_chgpass", ButtonStyle.Danger, row: 0)
                    .Build();

                var message = await channel.SendMessageAsync(embed: embed, components: component);

                _config.UcpPanelMessageId = message.Id;
                await SaveConfig();

                Console.WriteLine($"[+] Discord - UCP Panel created (Message ID: {message.Id})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[+] Discord - Error ensuring UCP panel: {ex.Message}");
            }
        }

        private static async Task<DiscordConfigs> LoadOrCreateConfig()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    var defaultConfig = new DiscordConfigs
                    {
                        Token = "YOUR_BOT_TOKEN_HERE",
                        GuildId = 0,
                        UcpPanelChannelId = 0,
                        UcpPanelMessageId = 0,
                        AutoCreateUcpPanel = true,
                        UcpPanelTitle = "🎮 State Side UCP Panel",
                        UcpPanelDescription = "Halo! Selamat datang di server State Side Roleplay! Di sini, Anda akan mendaftar akun UCP (User Control Panel), melakukan verifikasi ulang akun, dan mengirim ulang kode.\n\nJangan ragu untuk bertanya jika Anda membutuhkan bantuan lebih lanjut.",
                        UcpPanelThumbnailUrl = "https://i.imgur.com/example.png",
                        UcpPanelFooterText = "State Side Roleplay",
                        UcpPanelFooterIconUrl = "https://i.imgur.com/footer.png"
                    };

                    var json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(ConfigPath, json);

                    Console.WriteLine($"[+] Discord - Created default config at {ConfigPath}");
                    Console.WriteLine("[+] Discord - Please configure Token, GuildId, and UcpPanelChannelId");

                    return defaultConfig;
                }

                var configJson = await File.ReadAllTextAsync(ConfigPath);
                return JsonSerializer.Deserialize<DiscordConfigs>(configJson) ?? new DiscordConfigs();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[+] Discord - Error loading config: {ex.Message}");
                return new DiscordConfigs();
            }
        }

        private static async Task SaveConfig()
        {
            try
            {
                var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(ConfigPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[+] Discord - Error saving config: {ex.Message}");
            }
        }

        public static DiscordConfigs GetConfig()
        {
            return _config;
        }

        public static bool IsRunning => _isRunning;
    }
}