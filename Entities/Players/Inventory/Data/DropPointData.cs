using SampSharp.GameMode;
using SampSharp.GameMode.World;
using SampSharp.Streamer.World;
using System.Collections.Generic;

namespace ProjectSMP.Entities.Players.Inventory.Data
{
    public class DropPointData
    {
        public Vector3 Position { get; set; }
        public int VirtualWorld { get; set; }
        public int Interior { get; set; }
        public List<ItemData> Items { get; set; } = new();
        public DynamicTextLabel Label { get; set; }
    }
}