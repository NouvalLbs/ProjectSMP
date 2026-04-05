namespace ProjectSMP.Features.Jobs.Side.Bus
{
    public enum BusRoute { A = 0, B = 1, C = 2 }

    public enum BusPhase { Driving, Waiting }

    public class BusSession
    {
        public BusRoute Route { get; set; }
        public int CheckpointIndex { get; set; }
        public int VehicleId { get; set; }
        public BusPhase Phase { get; set; }
    }
}