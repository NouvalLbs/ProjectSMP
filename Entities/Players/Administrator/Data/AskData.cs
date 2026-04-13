using System;

namespace ProjectSMP.Entities.Players.Administrator.Data
{
    public class AskData
    {
        public int Id { get; set; }
        public bool InUse { get; set; }
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = "";
        public string Question { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public int TimeToExpire { get; set; }
        public int LockedBy { get; set; } = -1;
        public string LockedByName { get; set; } = "";
    }
}