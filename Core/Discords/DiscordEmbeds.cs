using Discord;
using ProjectSMP.Core.Discords.Models;
using System;

namespace ProjectSMP.Core.Discords
{
    public static class DiscordEmbeds
    {
        public static Embed BuildUCPPanel(DiscordConfigs config)
        {
            return new EmbedBuilder()
                .WithTitle(config.UcpPanelTitle)
                .WithDescription(config.UcpPanelDescription)
                .WithColor(Color.Blue)
                .WithThumbnailUrl(config.UcpPanelThumbnailUrl)
                .WithFooter(config.UcpPanelFooterText, config.UcpPanelFooterIconUrl)
                .WithCurrentTimestamp()
                .Build();
        }

        public static Embed BuildSuccess(string title, string message)
        {
            return new EmbedBuilder()
                .WithTitle($"✅ {title}")
                .WithDescription(message)
                .WithColor(Color.Green)
                .WithCurrentTimestamp()
                .Build();
        }

        public static Embed BuildError(string title, string message)
        {
            return new EmbedBuilder()
                .WithTitle($"❌ {title}")
                .WithDescription(message)
                .WithColor(Color.Red)
                .WithCurrentTimestamp()
                .Build();
        }

        public static Embed BuildInfo(string title, string message)
        {
            return new EmbedBuilder()
                .WithTitle($"ℹ️ {title}")
                .WithDescription(message)
                .WithColor(Color.Orange)
                .WithCurrentTimestamp()
                .Build();
        }

        public static Embed BuildVerificationCode(string username, string code, DiscordConfigs config)
        {
            var timestamp = DateTimeOffset.UtcNow.AddHours(7);
            var formattedTime = timestamp.ToString("dddd, MMMM dd, yyyy 'at' hh:mm tt",
                System.Globalization.CultureInfo.InvariantCulture);

            return new EmbedBuilder()
                .WithColor(new Color(43, 45, 49))
                .WithTitle($"🎉 Register UCP — {config.ServerName}")
                .WithDescription($"**Pendaftaran Berhasil!**\n\nHalo, **{username}** — terima kasih telah mendaftar.\nBerikut informasi akunmu:")
                .AddField("👤 Username (UCP)", $"```{username}```", inline: true)
                .AddField("🔐 Verification PIN", $"```{code}```", inline: true)
                .AddField("🌐 Server IP", $"```{config.ServerIp}```", inline: false)
                .AddField("⏱️ Waktu Pendaftaran", formattedTime, inline: false)
                .WithImageUrl(config.RegistrationBannerUrl)
                .WithFooter("Pastikan simpan Verification PIN kamu!")
                .WithCurrentTimestamp()
                .Build();
        }

        public static Embed BuildResendCode(string username, string code, DiscordConfigs config)
        {
            return new EmbedBuilder()
                .WithColor(new Color(88, 101, 242))
                .WithTitle($"🎉 Resend PIN — {config.ServerName}")
                .WithDescription(
                    "**Mengirim Ulang PIN Berhasil!**\n\n" +
                    $"Halo, **{username}** — terima kasih sudah setia di {config.ServerName}.\n\n" +
                    "Berikut informasi akunmu:")
                .AddField("👤 **Username (UCP)**", $"```{username}```", inline: true)
                .AddField("🔐 **Verification PIN**", $"```{code}```", inline: true)
                .WithImageUrl(config.ResendCodeBannerUrl)
                .WithFooter("⚠️ Jangan bagikan PIN kamu ke siapapun!")
                .WithCurrentTimestamp()
                .Build();
        }

        public static Embed BuildChangePassword(string username, string newPassword, DiscordConfigs config)
        {
            return new EmbedBuilder()
                .WithColor(new Color(88, 101, 242))
                .WithTitle($"🔐 Change Password — {config.ServerName}")
                .WithDescription(
                    "**Perubahan Password Berhasil!**\n\n" +
                    $"Halo, **{username}** — password akun kamu telah berhasil diperbarui.\n\n" +
                    "Jika ini bukan kamu, segera hubungi admin.")
                .AddField("👤 **Username (UCP)**", $"```{username}```", inline: true)
                .AddField("📌 **New Password**", $"```{newPassword}```", inline: true)
                .WithImageUrl(config.ChangePasswordBannerUrl)
                .WithFooter("⚠️ Jangan pernah membagikan data akun kamu ke siapapun!")
                .WithCurrentTimestamp()
                .Build();
        }
    }
}