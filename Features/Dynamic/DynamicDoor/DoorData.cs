using SampSharp.GameMode;
using SampSharp.Streamer.World;

namespace ProjectSMP.Features.Dynamic.DynamicDoor
{
    public class DynamicDoorData
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Password { get; set; } = "";
        public int Icon { get; set; } = 19130;
        public int MapIconId { get; set; } = -1;
        public bool Locked { get; set; }
        public int AdminLevel { get; set; }
        public int VipLevel { get; set; }
        public int FactionId { get; set; }
        public int FamilyId { get; set; } = -1;
        public bool IsGarage { get; set; }
        public bool CustomInterior { get; set; }

        public int ExtVirtualWorld { get; set; }
        public int ExtInterior { get; set; }
        public float ExtPosX { get; set; }
        public float ExtPosY { get; set; }
        public float ExtPosZ { get; set; }
        public float ExtAngle { get; set; }

        public int IntVirtualWorld { get; set; }
        public int IntInterior { get; set; }
        public float IntPosX { get; set; }
        public float IntPosY { get; set; }
        public float IntPosZ { get; set; }
        public float IntAngle { get; set; }

        public DynamicPickup ExtPickup { get; set; }
        public DynamicTextLabel ExtLabel { get; set; }
        public DynamicMapIcon ExtMapIcon { get; set; }
        public Core.Polygon ExtPolygon { get; set; }

        public DynamicPickup IntPickup { get; set; }
        public DynamicTextLabel IntLabel { get; set; }
        public Core.Polygon IntPolygon { get; set; }
    }

    public class DoorDatabaseRow
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int Icon { get; set; }
        public int Mapicon { get; set; }
        public int Locked { get; set; }
        public int Admin { get; set; }
        public int Vip { get; set; }
        public int Faction { get; set; }
        public int Family { get; set; }
        public int Garage { get; set; }
        public int Custom { get; set; }
        public int Extvw { get; set; }
        public int Extint { get; set; }
        public float Extposx { get; set; }
        public float Extposy { get; set; }
        public float Extposz { get; set; }
        public float Extposa { get; set; }
        public int Intvw { get; set; }
        public int Intint { get; set; }
        public float Intposx { get; set; }
        public float Intposy { get; set; }
        public float Intposz { get; set; }
        public float Intposa { get; set; }
    }
}