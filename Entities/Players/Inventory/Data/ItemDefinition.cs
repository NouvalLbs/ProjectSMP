namespace ProjectSMP.Entities.Players.Inventory.Data
{
    public class ItemDefinition
    {
        public string ItemName { get; set; } = "";
        public int ItemStack { get; set; } = 1;
        public int Weight { get; set; }
        public bool CanUsed { get; set; }
        public bool CanGive { get; set; }
        public bool CanDrop { get; set; }
        public bool CanDetail { get; set; }
        public int ProgDur { get; set; } = -1;
        public string ProgText { get; set; } = "";
        public int ProgAnimIndex { get; set; } = -1;
        public string ProgAnimLib { get; set; } = "";
        public string ProgAnimName { get; set; } = "";
        public int DurabilityDuration { get; set; }
        public string ItemType { get; set; } = "Unknown";
    }
}