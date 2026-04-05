#nullable enable

namespace ProjectSMP.Features.PreviewModelDialog
{
    public sealed class PreviewModelResponseArgs
    {
        public bool Accepted { get; }
        public int ListItem { get; }
        public int ModelId { get; }

        internal PreviewModelResponseArgs(bool accepted, int listItem, int modelId)
        {
            Accepted = accepted;
            ListItem = listItem;
            ModelId = modelId;
        }
    }
}