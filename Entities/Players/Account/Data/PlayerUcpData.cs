namespace ProjectSMP.Entities.Players.Account.Data {
    public class PlayerUcpData {
        public string UCP { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DiscordId { get; set; } = string.Empty;
        public int VerifyCode { get; set; }
        public int LoginAttempt { get; set; }
    }
}