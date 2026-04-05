using SampSharp.Streamer.World;

namespace ProjectSMP.Features.Chat
{
    internal class PlayerRoleplayPrefix
    {
        public bool AdoActive { get; set; }
        public DynamicTextLabel AdoTag { get; set; }
        public bool AbActive { get; set; }
        public DynamicTextLabel AbTag { get; set; }
    }

    internal static class ChatColors
    {
        public const int LocalOOC = unchecked((int)0xD0D0D0FF);
        public const int SpeechBubble = unchecked((int)0xEEEEEEFF);
        public const int GlobalOOC = unchecked((int)0xE0FFFFAA);
        public const int Purple = unchecked((int)0xD0AEEBFF);
        public const int LightBrown = unchecked((int)0xA38587FF);
        public const int Whisper = unchecked((int)0xFFFF00AA);
    }

    internal static class ChatDistance
    {
        public const float Shout = 27.5f;
        public const float Normal = 12.5f;
        public const float Low = 4.0f;
    }
}