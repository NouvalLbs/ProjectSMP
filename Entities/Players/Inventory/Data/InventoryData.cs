using System.Collections.Generic;

namespace ProjectSMP.Entities.Players.Inventory.Data
{
    public class InventoryData
    {
        public int Enabled { get; set; }
        public int Slots { get; set; } = 32;
        public int MaxWeight { get; set; } = 60000;
        public List<ItemData> Inventory { get; set; } = new();
    }
}