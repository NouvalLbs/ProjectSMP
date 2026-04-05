namespace ProjectSMP.Plugins.WeaponConfig
{
    public class DamageLogEntry
    {
        public string Issuer { get; set; } = "";
        public long Timestamp { get; set; }
        public int Weapon { get; set; }
        public float Amount { get; set; }
        public int Bodypart { get; set; }
    }
}