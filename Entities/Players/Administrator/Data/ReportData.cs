using System;

namespace ProjectSMP.Entities.Players.Administrator.Data
{
    public class ReportEntry
    {
        public int Id { get; set; }
        public bool InUse { get; set; }
        public string Message { get; set; } = "";
        public int ReporterId { get; set; }
        public string ReporterName { get; set; } = "";
        public int TimeToExpire { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CheckingBy { get; set; }
    }
}