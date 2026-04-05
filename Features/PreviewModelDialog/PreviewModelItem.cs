#nullable enable

namespace ProjectSMP.Features.PreviewModelDialog
{
    public sealed class PreviewModelItem
    {
        public int ModelId { get; init; }
        public string Text { get; init; } = string.Empty;
        public float RotX { get; set; }
        public float RotY { get; set; }
        public float RotZ { get; set; } = -45f;
        public float Zoom { get; set; } = 1f;
        public int Color1 { get; init; } = -1;
        public int Color2 { get; init; } = -1;
    }
}