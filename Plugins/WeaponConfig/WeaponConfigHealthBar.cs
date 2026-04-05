#nullable enable
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Display;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Plugins.WeaponConfig
{
    internal static class WeaponConfigHealthBar
    {
        private const float BarX = 502f;
        private const float BarY = 383.5f;
        private const float MaxWidth = 33.5f;
        private const float BarH = 6.5f;
        private const float BorderPad = 0.5f;
        private static bool _isInitialized = false;

        internal static bool IsInternalTextDraw(TextDraw td)
            => _isInitialized && (td == _border || td == _background);

        internal static bool IsInternalPlayerTextDraw(PlayerTextDraw ptd, int playerId)
            => _isInitialized && _bars.TryGetValue(playerId, out var bar) && ptd == bar;

        private static readonly Color FgColor = new(0xB4, 0x19, 0x1D, 0xFF);
        private static readonly Color BgColor = new(0x5A, 0x0C, 0x0E, 0xFF);
        private static readonly Color BorderColor = new(0x00, 0x00, 0x00, 0xFF);
        private static readonly Color Transparent = new(0x00, 0x00, 0x00, 0x00);

        private static TextDraw? _border;
        private static TextDraw? _background;

        private static readonly Dictionary<int, PlayerTextDraw> _bars = new();
        private static readonly Dictionary<int, bool> _enabled = new();

        public static void Init()
        {
            _border = TextDrawManager.CreateInternal(new Vector2(BarX - BorderPad, BarY - BorderPad), "LD_SPAC:WHITE");
            _border.Font = TextDrawFont.DrawSprite;
            _border.LetterSize = new Vector2(0f, 1f);
            _border.ForeColor = BorderColor;
            _border.UseBox = true;
            _border.BoxColor = BorderColor;
            _border.Width = BarX + MaxWidth + BorderPad;
            _border.Height = BarH + BorderPad * 2;
            _border.Shadow = 0;
            _border.Outline = 0;
            _border.Proportional = true;

            _background = TextDrawManager.CreateInternal(new Vector2(BarX, BarY), "LD_SPAC:WHITE");
            _background.Font = TextDrawFont.DrawSprite;
            _background.LetterSize = new Vector2(0f, 1f);
            _background.ForeColor = BgColor;
            _background.UseBox = true;
            _background.BoxColor = BgColor;
            _background.Width = BarX + MaxWidth;
            _background.Height = BarH;
            _background.Shadow = 0;
            _background.Outline = 0;
            _background.Proportional = true;

            _isInitialized = true;
        }

        public static void Dispose()
        {
            if (_border != null)
            {
                TextDrawManager.UnregisterInternalTextDraw(_border);
                _border.Dispose();
            }
            if (_background != null)
            {
                TextDrawManager.UnregisterInternalTextDraw(_background);
                _background.Dispose();
            }
            _isInitialized = false;
        }

        public static void OnConnect(BasePlayer player)
        {
            _enabled[player.Id] = true;

            var ptd = TextDrawManager.CreatePlayerInternal(player, new Vector2(BarX, BarY), "LD_SPAC:WHITE");
            ptd.Font = TextDrawFont.DrawSprite;
            ptd.LetterSize = new Vector2(0f, 1f);
            ptd.ForeColor = FgColor;
            ptd.UseBox = true;
            ptd.BoxColor = FgColor;
            ptd.Width = BarX + MaxWidth;
            ptd.Height = BarH;
            ptd.Shadow = 0;
            ptd.Outline = 0;
            ptd.Proportional = true;

            _bars[player.Id] = ptd;

            _border?.Show(player);
            _background?.Show(player);
        }

        public static void OnDisconnect(BasePlayer player)
        {
            if (_bars.TryGetValue(player.Id, out var ptd))
            {
                TextDrawManager.UnregisterInternalPlayerTextDraw(player.Id, ptd);
                ptd.Dispose();
            }
            _bars.Remove(player.Id);
            _enabled.Remove(player.Id);
            TextDrawManager.CleanupPlayer(player.Id);
        }

        public static void Update(BasePlayer player, float health, float maxHealth)
        {
            if (!_enabled.TryGetValue(player.Id, out var en) || !en) return;
            if (!_bars.TryGetValue(player.Id, out var ptd)) return;

            var pct = maxHealth > 0 ? MathF.Max(0, MathF.Min(health / maxHealth, 1f)) : 0f;
            var w = pct * MaxWidth;

            if (w <= 0) {
                ptd.BoxColor = Transparent;
                ptd.ForeColor = Transparent;
            } else {
                ptd.BoxColor = FgColor;
                ptd.ForeColor = FgColor;
                ptd.Width = BarX + w;
            }
            ptd.Show();
        }

        public static void Show(BasePlayer player)
        {
            if (!_enabled.TryGetValue(player.Id, out var en) || !en) return;
            _border?.Show(player);
            _background?.Show(player);
            if (_bars.TryGetValue(player.Id, out var ptd)) ptd.Show();
        }

        public static void Hide(BasePlayer player)
        {
            _border?.Hide(player);
            _background?.Hide(player);
            if (_bars.TryGetValue(player.Id, out var ptd)) ptd.Hide();
        }

        public static void SetEnabled(BasePlayer player, bool enable)
        {
            _enabled[player.Id] = enable;
            if (enable) Show(player);
            else Hide(player);
        }
    }
}