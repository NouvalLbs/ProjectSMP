using ProjectSMP.Extensions;
using SampSharp.GameMode;
using SampSharp.GameMode.SAMP;
using SampSharp.Streamer.World;
using System.Collections.Generic;

namespace ProjectSMP.Entities.Players.NameTag
{
    public static class NameTagService
    {
        private static readonly Dictionary<int, DynamicTextLabel> _nameTags = new();

        public static void Refresh(Player player)
        {
            if (!player.IsCharLoaded) return;

            var textFormat = BuildNameTagText(player);
            var color = GetNameTagColor(player);

            GetOrCreateNameTag(player, textFormat, color);
        }

        public static void Destroy(Player player)
        {
            if (!_nameTags.TryGetValue(player.Id, out var tag)) return;

            tag?.Dispose();
            _nameTags.Remove(player.Id);
        }

        public static void Cleanup(Player player)
        {
            Destroy(player);
        }

        private static string BuildNameTagText(Player player)
        {
            if (player.AdminOnDuty)
                return "Admin";

            if (player.MaskActive)
            {
                var hp = player.GetHealthSafe();
                var ap = player.GetArmourSafe();
                return $"Mask_#{player.MaskId}\nHP: {{FF0000}}{hp:F1}{{FFFFFF}} AP: {{00FF00}}{ap:F1}{{FFFFFF}}";
            }

            return $"{{FFFFFF}}{Utilities.ReturnName(player)}({player.Id})";
        }

        private static Color GetNameTagColor(Player player)
        {
            if (player.AdminOnDuty)
                return Color.Blue;

            return Color.White;
        }

        private static void GetOrCreateNameTag(Player player, string text, Color color)
        {
            if (_nameTags.TryGetValue(player.Id, out var existingTag) && existingTag != null)
            {
                existingTag.Text = text;
                existingTag.Color = color;
            }
            else
            {
                var tag = new DynamicTextLabel(
                    text,
                    color,
                    new Vector3(0, 0, 0.18f),
                    8.0f,
                    player
                );

                _nameTags[player.Id] = tag;
            }
        }
    }
}