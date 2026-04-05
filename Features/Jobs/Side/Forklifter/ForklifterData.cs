using System.Collections.Generic;

namespace ProjectSMP.Features.Jobs.Side.Forklifter
{
    public enum ForklifterPhase { GoToLoad, Loading, GoToUnload, Unloading, ReturnToParking }

    public class ForklifterSession
    {
        public bool IsActive { get; set; }
        public ForklifterPhase Phase { get; set; }
        public int CycleCount { get; set; }
        public int VehicleId { get; set; }
        public Queue<int> LoadQueue { get; set; } = new();
        public Queue<int> UnloadQueue { get; set; } = new();
    }
}