namespace ProjectSMP.Core.Discords.Models
{
    public class DiscordConfigs
    {
        public string Token { get; set; } = "";
        public ulong GuildId { get; set; }
        public ulong UcpPanelChannelId { get; set; }
        public ulong UcpPanelMessageId { get; set; }
        public bool AutoCreateUcpPanel { get; set; } = true;
        public string UcpPanelTitle { get; set; } = "🎮 State Side UCP Panel";
        public string UcpPanelDescription { get; set; } = "Halo! Selamat datang di server State Side Roleplay! Di sini, Anda akan mendaftar akun UCP (User Control Panel), melakukan verifikasi ulang akun, dan mengirim ulang kode.\n\nJangan ragu untuk bertanya jika Anda membutuhkan bantuan lebih lanjut.";
        public string UcpPanelThumbnailUrl { get; set; } = "https://i.imgur.com/example.png";
        public string UcpPanelFooterText { get; set; } = "State Side Roleplay";
        public string UcpPanelFooterIconUrl { get; set; } = "https://i.imgur.com/footer.png";
        public string ServerName { get; set; } = "Prestige World Roleplay";
        public string ServerIp { get; set; } = "38.47.176.214:7777";
        public string RegistrationBannerUrl { get; set; } = "https://i.imgur.com/example.png";
        public string ResendCodeBannerUrl { get; set; } = "https://i.imgur.com/example.png";
        public string ChangePasswordBannerUrl { get; set; } = "https://i.imgur.com/example.png";
        public ulong ReverifRoleId { get; set; } = 0;
    }
}