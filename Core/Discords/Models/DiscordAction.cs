namespace ProjectSMP.Core.Discords.Models
{
    public class DiscordAction
    {
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public string DiscordId { get; set; }
        public string Action { get; set; }
        public string Data { get; set; }
        public bool Processed { get; set; }
        public string Timestamp { get; set; }
    }
}