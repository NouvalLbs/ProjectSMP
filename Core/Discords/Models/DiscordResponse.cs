namespace ProjectSMP.Core.Discords.Models
{
    public class DiscordResponse
    {
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public string Message { get; set; }
        public string EmbedData { get; set; }
        public bool Processed { get; set; }
        public string Timestamp { get; set; }
    }
}