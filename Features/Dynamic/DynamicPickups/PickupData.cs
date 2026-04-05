using SampSharp.GameMode;
using SampSharp.Streamer.World;

namespace ProjectSMP.Features.Dynamic.DynamicPickups
{
    public class DynamicPickupData
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Name { get; set; } = "";
        public int ModelId { get; set; }
        public int Type { get; set; }
        public int VirtualWorld { get; set; }
        public int Interior { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public string Callback { get; set; } = "";

        public DynamicPickup Pickup { get; set; }
        public DynamicTextLabel Label { get; set; }
        public Core.Polygon Polygon { get; set; }
    }

    public class PickupDatabaseRow
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public int Model { get; set; }
        public int Type { get; set; }
        public int Vw { get; set; }
        public int Interior { get; set; }
        public float Posx { get; set; }
        public float Posy { get; set; }
        public float Posz { get; set; }
        public string Callback { get; set; }
    }
}