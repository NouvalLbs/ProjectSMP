#nullable enable
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Display;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectSMP.Plugins.WeaponConfig
{
    internal sealed class FeedEntry
    {
        public string Name { get; set; } = "";
        public float Amount { get; set; }
        public int Weapon { get; set; }
        public int Tick { get; set; }
    }

    internal sealed class PlayerFeedState
    {
        public PlayerTextDraw? TakenTD;
        public PlayerTextDraw? GivenTD;
        public int EnabledOverride = -1;
        public readonly FeedEntry[] Taken = new FeedEntry[5];
        public readonly FeedEntry[] Given = new FeedEntry[5];
        public int TakenIdx;
        public int GivenIdx;
        public int LastRenderTick;
        public CancellationTokenSource? HideCts;
        public int SpectatingId = -1;
    }

    internal static class WeaponConfigDamageFeed
    {
        private const int FeedHeight = 5;
        private const int MaxUpdateRateMs = 250;

        private const float DamageFeedX = 634.0f;
        private const float TakenStartY = 363.5f;
        private const float GivenStartY = 363.5f;
        private const float LineHeight = 9.0f;

        private static int _hideDelayMs = 3000;
        private static bool _isInitialized = false;

        private static readonly Color TakenColor = new(0x33, 0xCC, 0xFF, 0xFF);
        private static readonly Color GivenColor = new(0x30, 0xFF, 0x50, 0xFF);
        private static readonly Color Black = new(0x00, 0x00, 0x00, 0xFF);

        private static readonly Dictionary<int, PlayerFeedState> _feeds = new();
        private static bool _globalEnabled = true;

        internal static bool IsInternalPlayerTextDraw(PlayerTextDraw? ptd, int playerId)
        {
            if (!_isInitialized || !_feeds.TryGetValue(playerId, out var s)) return false;
            return ptd == s.TakenTD || ptd == s.GivenTD;
        }

        public static void Init(bool globalEnabled, int hideDelayMs = 3000)
        {
            _globalEnabled = globalEnabled;
            _hideDelayMs = hideDelayMs;
            _isInitialized = true;
        }

        public static void OnConnect(BasePlayer player)
        {
            var state = new PlayerFeedState { EnabledOverride = -1 };

            state.TakenTD = TextDrawManager.CreatePlayerInternal(player, new Vector2(DamageFeedX, TakenStartY), "_");
            state.TakenTD.Font = TextDrawFont.Slim;
            state.TakenTD.LetterSize = new Vector2(0.141666f, 1.2f);
            state.TakenTD.Alignment = TextDrawAlignment.Right;
            state.TakenTD.ForeColor = TakenColor;
            state.TakenTD.Shadow = 0;
            state.TakenTD.Outline = 1;
            state.TakenTD.BackColor = Black;
            state.TakenTD.Proportional = true;
            state.TakenTD.Width = 640f;
            state.TakenTD.Height = 480f;

            state.GivenTD = TextDrawManager.CreatePlayerInternal(player, new Vector2(DamageFeedX, GivenStartY), "_");
            state.GivenTD.Font = TextDrawFont.Slim;
            state.GivenTD.LetterSize = new Vector2(0.141666f, 1.2f);
            state.GivenTD.Alignment = TextDrawAlignment.Right;
            state.GivenTD.ForeColor = GivenColor;
            state.GivenTD.Shadow = 0;
            state.GivenTD.Outline = 1;
            state.GivenTD.BackColor = Black;
            state.GivenTD.Proportional = true;
            state.GivenTD.Width = 640f;
            state.GivenTD.Height = 480f;

            _feeds[player.Id] = state;
        }

        public static void OnDisconnect(BasePlayer player)
        {
            if (!_feeds.TryGetValue(player.Id, out var s)) return;
            s.HideCts?.Cancel();

            if (s.TakenTD != null)
            {
                TextDrawManager.UnregisterInternalPlayerTextDraw(player.Id, s.TakenTD);
                s.TakenTD.Dispose();
            }
            if (s.GivenTD != null)
            {
                TextDrawManager.UnregisterInternalPlayerTextDraw(player.Id, s.GivenTD);
                s.GivenTD.Dispose();
            }

            _feeds.Remove(player.Id);
            TextDrawManager.CleanupPlayer(player.Id);
        }

        public static void SetSpectating(BasePlayer spectator, int targetPlayerId)
        {
            if (_feeds.TryGetValue(spectator.Id, out var s))
                s.SpectatingId = targetPlayerId;
        }

        public static void ClearSpectating(BasePlayer spectator)
        {
            if (!_feeds.TryGetValue(spectator.Id, out var s)) return;
            s.SpectatingId = -1;
            s.TakenTD?.Hide();
            s.GivenTD?.Hide();
        }

        public static void AddTaken(BasePlayer player, string issuerName, float amount, int weapon)
        {
            if (!_feeds.TryGetValue(player.Id, out var s)) return;
            var active = s.EnabledOverride == -1 ? _globalEnabled : s.EnabledOverride == 1;
            if (!active) return;

            var idx = s.TakenIdx % FeedHeight;
            s.Taken[idx] = new FeedEntry
            { Name = issuerName, Amount = amount, Weapon = weapon, Tick = Environment.TickCount };
            s.TakenIdx++;

            var now = Environment.TickCount;
            if (now - s.LastRenderTick >= MaxUpdateRateMs)
            {
                s.LastRenderTick = now;
                RenderFeed(s);
                PushFeedToSpectators(player.Id, s);
            }
            ScheduleHide(s, player);
        }

        public static void AddGiven(BasePlayer player, string targetName, float amount, int weapon)
        {
            if (!_feeds.TryGetValue(player.Id, out var s)) return;
            var active = s.EnabledOverride == -1 ? _globalEnabled : s.EnabledOverride == 1;
            if (!active) return;

            var idx = s.GivenIdx % FeedHeight;
            s.Given[idx] = new FeedEntry
            { Name = targetName, Amount = amount, Weapon = weapon, Tick = Environment.TickCount };
            s.GivenIdx++;

            var now = Environment.TickCount;
            if (now - s.LastRenderTick >= MaxUpdateRateMs)
            {
                s.LastRenderTick = now;
                RenderFeed(s);
                PushFeedToSpectators(player.Id, s);
            }
            ScheduleHide(s, player);
        }

        public static void SetGlobalEnabled(bool enabled)
        {
            _globalEnabled = enabled;
            foreach (var (id, s) in _feeds)
            {
                if (s.EnabledOverride != -1) continue;
                if (!enabled) { s.HideCts?.Cancel(); s.TakenTD?.Hide(); s.GivenTD?.Hide(); }
            }
        }

        public static void SetEnabled(BasePlayer player, bool enable)
        {
            if (!_feeds.TryGetValue(player.Id, out var s)) return;
            s.EnabledOverride = enable ? 1 : 0;
            if (!enable) { s.HideCts?.Cancel(); s.TakenTD?.Hide(); s.GivenTD?.Hide(); }
        }

        public static void ResetToGlobal(BasePlayer player)
        {
            if (!_feeds.TryGetValue(player.Id, out var s)) return;
            s.EnabledOverride = -1;
            if (!_globalEnabled) { s.HideCts?.Cancel(); s.TakenTD?.Hide(); s.GivenTD?.Hide(); }
        }

        public static void ApplyGlobalChange(BasePlayer player, bool globalEnabled)
        {
            if (!_feeds.TryGetValue(player.Id, out var s) || s.EnabledOverride != -1) return;
            if (!globalEnabled) { s.HideCts?.Cancel(); s.TakenTD?.Hide(); s.GivenTD?.Hide(); }
        }

        public static bool IsEnabled(BasePlayer player)
            => _feeds.TryGetValue(player.Id, out var s) && (s.EnabledOverride == -1 ? _globalEnabled : s.EnabledOverride == 1);

        private static void RenderFeed(PlayerFeedState s)
        {
            s.TakenTD!.Text = BuildText(s.Taken, s.TakenIdx);
            s.GivenTD!.Text = BuildText(s.Given, s.GivenIdx);
            s.TakenTD.Show();
            s.GivenTD.Show();
        }

        private static void PushFeedToSpectators(int targetId, PlayerFeedState targetState)
        {
            foreach (var (spectId, spectState) in _feeds)
            {
                if (spectState.SpectatingId != targetId) continue;
                if (BasePlayer.Find(spectId) is not BasePlayer spec) continue;

                spectState.TakenTD!.Text = BuildText(targetState.Taken, targetState.TakenIdx);
                spectState.GivenTD!.Text = BuildText(targetState.Given, targetState.GivenIdx);
                spectState.TakenTD.Show();
                spectState.GivenTD.Show();
                ScheduleHide(spectState, spec);
            }
        }

        private static string BuildText(FeedEntry[] entries, int headIdx)
        {
            var lines = new string[FeedHeight];
            var now = Environment.TickCount;

            for (var i = 0; i < FeedHeight; i++)
            {
                var idx = ((headIdx - i - 1) % FeedHeight + FeedHeight) % FeedHeight;
                var entry = entries[idx];
                lines[FeedHeight - 1 - i] = entry == null || now - entry.Tick > _hideDelayMs
                    ? " "
                    : $"{entry.Name}: -{entry.Amount:F1}";
            }
            return string.Join("~n~", lines);
        }

        private static void ScheduleHide(PlayerFeedState s, BasePlayer player)
        {
            s.HideCts?.Cancel();
            var cts = new CancellationTokenSource();
            s.HideCts = cts;
            _ = HideAfterAsync(s, player, cts.Token);
        }

        private static async Task HideAfterAsync(PlayerFeedState s, BasePlayer player,
            CancellationToken ct)
        {
            try
            {
                await Task.Delay(_hideDelayMs + 200, ct);
                if (player.IsDisposed) return;
                s.TakenTD?.Hide();
                s.GivenTD?.Hide();
            }
            catch (OperationCanceledException) { }
        }
    }
}