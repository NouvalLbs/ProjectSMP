namespace ProjectSMP.Features.Jobs.Side.Trashmaster
{
    public enum TrashmasterPhase { GoToTrash, Collecting, GoToTrunk, Dumping, GoToDropout }

    public class TrashmasterSession
    {
        public bool IsActive { get; set; }
        public TrashmasterPhase Phase { get; set; }
        public int TrashCollected { get; set; }
        public int VehicleId { get; set; }
        public int CurrentTrashId { get; set; } = -1;
    }
}