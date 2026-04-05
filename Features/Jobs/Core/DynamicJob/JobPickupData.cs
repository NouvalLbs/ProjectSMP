using ProjectSMP.Core;
using SampSharp.Streamer.World;

namespace ProjectSMP.Features.Jobs.Core.DynamicJob
{
    public class DynamicJobData
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string JobName { get; set; } = "";
        public int VirtualWorld { get; set; }
        public int Interior { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }

        public DynamicPickup Pickup { get; set; }
        public DynamicTextLabel Label { get; set; }
        public Polygon Polygon { get; set; }
    }

    public class JobLocationRow
    {
        public int ID { get; set; }
        public string Name { get; set; } = "";
        public string JobName { get; set; } = "";
        public int Vw { get; set; }
        public int Interior { get; set; }
        public float Posx { get; set; }
        public float Posy { get; set; }
        public float Posz { get; set; }
    }
}