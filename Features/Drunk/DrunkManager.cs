#nullable enable
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Features.Drunk
{
    public static class DrunkManager
    {
        private sealed class SourceState
        {
            public int Level;
            public int DecayPerTick;
        }

        private sealed class PlayerDrunkData
        {
            public readonly Dictionary<DrunkSource, SourceState> Sources = new();
        }

        private const int TickMs = 100;
        private static readonly Dictionary<int, PlayerDrunkData> _data = new();
        private static Timer _timer = null!;

        public static void Initialize()
        {
            _timer = new Timer(TickMs, true);
            _timer.Tick += OnTick;
        }

        public static void Dispose()
        {
            _timer?.Dispose();
            _data.Clear();
        }

        public static void RegisterPlayer(Player player)
        {
            _data[player.Id] = new PlayerDrunkData();
        }

        public static void UnregisterPlayer(Player player)
        {
            if (_data.Remove(player.Id) && !player.IsDisposed)
                player.DrunkLevel = 0;
        }

        public static void SetDrunk(Player player, DrunkSource source, int level, int decayPerTick = 0)
        {
            if (!_data.TryGetValue(player.Id, out var data)) return;

            if (level <= 0)
                data.Sources.Remove(source);
            else
                data.Sources[source] = new SourceState { Level = level, DecayPerTick = decayPerTick };

            Apply(player, data);
        }

        public static void ClearDrunk(Player player, DrunkSource source)
        {
            if (!_data.TryGetValue(player.Id, out var data)) return;
            data.Sources.Remove(source);
            Apply(player, data);
        }

        public static void ClearAll(Player player)
        {
            if (!_data.TryGetValue(player.Id, out var data)) return;
            data.Sources.Clear();
            Apply(player, data);
        }

        public static bool IsActive(Player player, DrunkSource source)
            => _data.TryGetValue(player.Id, out var d) && d.Sources.ContainsKey(source);

        private static void OnTick(object? sender, EventArgs e)
        {
            foreach (var kvp in _data)
            {
                var player = BasePlayer.Find(kvp.Key) as Player;
                if (player == null || player.IsDisposed) continue;

                var data = kvp.Value;
                var changed = false;

                List<DrunkSource>? toRemove = null;

                foreach (var (src, state) in data.Sources)
                {
                    if (state.DecayPerTick <= 0) continue;

                    state.Level -= state.DecayPerTick;
                    changed = true;

                    if (state.Level <= 0)
                        (toRemove ??= new()).Add(src);
                }

                if (toRemove != null)
                    foreach (var src in toRemove)
                        data.Sources.Remove(src);

                if (changed)
                    Apply(player, data);
            }
        }

        private static void Apply(Player player, PlayerDrunkData data)
        {
            var combined = 0;
            foreach (var state in data.Sources.Values)
                combined = Math.Max(combined, state.Level);

            player.DrunkLevel = combined;
        }
    }
}