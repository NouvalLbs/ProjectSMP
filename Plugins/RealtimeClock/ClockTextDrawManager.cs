using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Display;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.RealtimeClock
{
    internal static class ClockTextDrawManager
    {
        private static readonly Dictionary<int, PlayerTextDraw> _clocks = new();
        private static readonly string[] _monthNames = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        public static void Create(int playerId)
        {
            var player = BasePlayer.Find(playerId);
            if (player == null || player.IsDisposed) return;

            if (_clocks.ContainsKey(playerId))
                Destroy(playerId);

            var clock = new PlayerTextDraw(player, new Vector2(634.0f, 427.0f), "00 Jan 0000 - 00:00:00 WIB")
            {
                Font = TextDrawFont.Slim,
                LetterSize = new Vector2(0.141666f, 1.2f),
                Alignment = TextDrawAlignment.Right,
                ForeColor = Color.White,
                Shadow = 0,
                Outline = 1,
                Proportional = true,
                Selectable = false
            };

            _clocks[playerId] = clock;
            clock.Show();
        }

        public static void SetVisible(int playerId, bool show)
        {
            if (show)
                Create(playerId);
            else
                Destroy(playerId);
        }

        public static void Destroy(int playerId)
        {
            if (!_clocks.TryGetValue(playerId, out var clock)) return;
            clock.Dispose();
            _clocks.Remove(playerId);
        }

        public static void UpdateAll(int hour, int minute, int second)
        {
            var now = DateTime.Now;
            var text = $"{now.Day:D2} {_monthNames[now.Month - 1]} {now.Year} - {hour:D2}:{minute:D2}:{second:D2} WIB";

            foreach (var kvp in _clocks)
            {
                var player = BasePlayer.Find(kvp.Key);
                if (player != null && !player.IsDisposed && player is Player p && p.IsCharLoaded)
                {
                    kvp.Value.Text = text;
                    kvp.Value.Show();
                }
            }
        }

        public static void Clear()
        {
            foreach (var clock in _clocks.Values)
                clock.Dispose();
            _clocks.Clear();
        }
    }
}