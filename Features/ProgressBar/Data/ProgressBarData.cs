namespace ProjectSMP.Features.ProgressBar.Data
{
    public enum ProgressCallbackType
    {
        NoCallback = 0,
        UseItem
    }

    public class ProgressBarData
    {
        public bool IsActive { get; set; }
        public int Duration { get; set; }
        public float Percentage { get; set; }
        public ProgressCallbackType CallbackType { get; set; }
        public int AnimIndex { get; set; }
        public string AnimLib { get; set; } = "";
        public string AnimName { get; set; } = "";
        public int ItemSlot { get; set; } = -1;
        public string ItemName { get; set; } = "";
    }
}