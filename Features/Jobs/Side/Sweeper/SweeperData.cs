namespace ProjectSMP.Features.Jobs.Side.Sweeper
{
    public enum SweeperRoute { A = 0, B = 1, C = 2 }

    public class SweeperSession
    {
        public SweeperRoute Route { get; set; }
        public int CheckpointIndex { get; set; }
        public int VehicleId { get; set; }
    }
}