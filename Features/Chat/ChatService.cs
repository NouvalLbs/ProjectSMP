using SampSharp.GameMode;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using SampSharp.Streamer.World;
using ProjectSMP.Core;
using System.Collections.Generic;

namespace ProjectSMP.Features.Chat
{
    internal static class ChatService
    {
        private const int ChatBubbleTime = 6000;
        private static readonly Dictionary<int, PlayerRoleplayPrefix> _prefixes = new();

        public static void Initialize(Player player)
        {
            _prefixes[player.Id] = new PlayerRoleplayPrefix();
        }

        public static void Cleanup(Player player)
        {
            if (_prefixes.TryGetValue(player.Id, out var prefix))
            {
                prefix.AdoTag?.Dispose();
                prefix.AbTag?.Dispose();
            }
            _prefixes.Remove(player.Id);
        }

        public static void SendNearbyMessage(Player source, float radius, Color color, string message)
        {
            foreach (var player in BasePlayer.All)
            {
                if (player is Player p && p.IsCharLoaded && Utilities.NearPlayer(p, source, radius))
                {
                    p.SendClientMessage(color, message);
                }
            }
        }

        public static void TalkMessage(float distance, Player player, string prefix, string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            var msg = message;
            if (player.Settings.ToggleUppercase && msg.Length > 0)
                msg = char.ToUpper(msg[0]) + msg.Substring(1);

            var bubble = msg;
            var text = string.IsNullOrEmpty(prefix)
                ? $" says: \"{msg}\""
                : $" {prefix}: \"{msg}\"";

            if (player.AdminOnDuty)
            {
                var adminMsg = $"{{ff0000}}{Utilities.ReturnName(player)}:{{E0FFFF}} (( {{D0D0D0}}{msg}{{E0FFFF}} ))";
                player.SendClientMessage(new Color(ChatColors.LocalOOC), adminMsg);

                var pos = player.Position;
                foreach (var other in BasePlayer.All)
                {
                    if (other is Player p && p.IsCharLoaded && other.Id != player.Id &&
                        IsPlayerStreamedIn(player, p))
                    {
                        var dist = p.Position.DistanceTo(pos);
                        if (dist < distance)
                        {
                            p.SendClientMessage(new Color(ChatColors.LocalOOC), adminMsg);
                        }
                    }
                }
            }
            else
            {
                var fullMsg = Utilities.ReturnName(player) + text;
                player.SetChatBubble(bubble, new Color(ChatColors.SpeechBubble), distance, ChatBubbleTime);
                player.SendClientMessage(new Color(ChatColors.LocalOOC), fullMsg);

                var pos = player.Position;
                foreach (var other in BasePlayer.All)
                {
                    if (other is Player p && p.IsCharLoaded && other.Id != player.Id &&
                        IsPlayerStreamedIn(player, p))
                    {
                        var dist = p.Position.DistanceTo(pos);
                        if (dist < distance)
                        {
                            var normDist = 1.0f - (dist / distance);
                            int colorScale = normDist > 0.75f ? 220 : (int)(96.0f + (128.0f * normDist));
                            var colorValue = new Color((byte)0xFF, (byte)0xFF, (byte)0xFF, (byte)colorScale);
                            p.SendClientMessage(colorValue, fullMsg);
                        }
                    }
                }
            }
        }

        public static void ProcessWhisper(Player sender, Player target, string message)
        {
            var msg = message;
            if (sender.Settings.ToggleUppercase && msg.Length > 0)
                msg = char.ToUpper(msg[0]) + msg.Substring(1);

            sender.SendClientMessage(new Color(ChatColors.Whisper), $"Whisper to {target.CharInfo.Username}({target.Id}): {msg}");
            target.SendClientMessage(new Color(ChatColors.Whisper), $"Whisper from {sender.CharInfo.Username}({target.Id}): {msg}");
            target.PlaySound(1085, Vector3.Zero);
        }

        public static void ProcessPrivateMessage(Player sender, Player target, string message)
        {
            var msg = message;
            if (sender.Settings.ToggleUppercase && msg.Length > 0)
                msg = char.ToUpper(msg[0]) + msg.Substring(1);

            sender.SendClientMessage(new Color(ChatColors.Whisper), $"(( PM to {target.CharInfo.Username}({target.Id}): {msg} ))");
            target.SendClientMessage(new Color(ChatColors.Whisper), $"(( PM from {sender.CharInfo.Username}({sender.Id}): {msg} ))");
            target.PlaySound(1085, Vector3.Zero);
        }

        public static void ProcessActionText(Player player, string message, ActionType actionType, float distance)
        {
            var msg = message;
            if (player.Settings.ToggleUppercase && msg.Length > 0)
                msg = char.ToUpper(msg[0]) + msg.Substring(1);

            var name = Utilities.ReturnName(player);
            string actionText, bubbleText = "";
            Color color;

            switch (actionType)
            {
                case ActionType.Do:
                    actionText = $"* {msg} (({name}))";
                    bubbleText = $"* (( {msg} ))";
                    color = new Color(ChatColors.Purple);
                    break;
                case ActionType.LowerDo:
                    actionText = $"* {msg} (({name}))";
                    bubbleText = $"* (( {msg} ))";
                    color = new Color(ChatColors.LightBrown);
                    break;
                case ActionType.Me:
                    actionText = $"* {name} {msg}";
                    color = new Color(ChatColors.Purple);
                    break;
                case ActionType.LowerMe:
                    actionText = $"* {name} {msg}";
                    color = new Color(ChatColors.LightBrown);
                    break;
                case ActionType.Ame:
                    actionText = $"{Msg.AME} * {name} {msg}";
                    bubbleText = $"* {msg}";
                    color = new Color(ChatColors.Purple);
                    player.SendClientMessage(color, actionText);
                    player.SetChatBubble(bubbleText, new Color(ChatColors.Purple), distance, ChatBubbleTime);
                    return;
                default:
                    return;
            }

            SendNearbyMessage(player, distance, color, actionText);
            if (!string.IsNullOrEmpty(bubbleText))
                player.SetChatBubble(bubbleText, color, distance, ChatBubbleTime);
        }

        public static void ProcessChatText(Player player, string text)
        {
            if (text.StartsWith("!") && text.Length > 1)
            {
                var msg = text[1] == ' ' ? text.Substring(2) : text.Substring(1);
                TalkMessage(ChatDistance.Shout, player, "shouts", msg);
                return;
            }

            if (text.StartsWith("#") && text.Length > 1)
            {
                var msg = text[1] == ' ' ? text.Substring(2) : text.Substring(1);
                TalkMessage(ChatDistance.Low, player, "whispers", msg);
                return;
            }

            if (player.Settings.ToggleAutoLowChat)
            {
                TalkMessage(ChatDistance.Low, player, "whispers", text);
                return;
            }

            TalkMessage(ChatDistance.Normal, player, "", text);
        }

        public static string MessageFix(string text)
        {
            return text.Replace("\\n", "\n").Replace("\\t", "\t").Replace("\\\\", "\\");
        }

        public static void CreateOrUpdate3DLabel(Player player, bool isAdo, string text, Color color)
        {
            if (!_prefixes.TryGetValue(player.Id, out var prefix)) return;

            var pos = player.Position;

            if (isAdo)
            {
                prefix.AdoTag?.Dispose();
                prefix.AdoTag = new DynamicTextLabel(text, color, pos, 15f, null, streamdistance: 15f);
                prefix.AdoActive = true;
            }
            else
            {
                prefix.AbTag?.Dispose();
                prefix.AbTag = new DynamicTextLabel(text, color, pos, 15f, null, streamdistance: 15f);
                prefix.AbActive = true;
            }
        }

        public static void Remove3DLabel(Player player, bool isAdo)
        {
            if (!_prefixes.TryGetValue(player.Id, out var prefix)) return;

            if (isAdo)
            {
                prefix.AdoTag?.Dispose();
                prefix.AdoTag = null;
                prefix.AdoActive = false;
            }
            else
            {
                prefix.AbTag?.Dispose();
                prefix.AbTag = null;
                prefix.AbActive = false;
            }
        }

        public static bool Is3DLabelActive(Player player, bool isAdo)
        {
            if (!_prefixes.TryGetValue(player.Id, out var prefix)) return false;
            return isAdo ? prefix.AdoActive : prefix.AbActive;
        }

        private static bool IsPlayerStreamedIn(BasePlayer player, BasePlayer other)
        {
            if (player.Interior != other.Interior || player.VirtualWorld != other.VirtualWorld)
                return false;
            return true;
        }
    }

    internal enum ActionType
    {
        Me = 1,
        LowerMe,
        Ame,
        Do,
        LowerDo
    }
}