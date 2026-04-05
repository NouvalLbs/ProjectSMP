namespace ProjectSMP
{
    public partial class Player
    {
        public bool AdminOnDuty { get; set; }
        public bool MaskActive { get; set; }
        public float SavedPosX { get; set; }
        public float SavedPosY { get; set; }
        public float SavedPosZ { get; set; }
        public int SavedInterior { get; set; }
        public int SavedWorld { get; set; }
    }
}