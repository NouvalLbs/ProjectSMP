using SampSharp.GameMode;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System.Collections.Generic;

namespace ProjectSMP.Features.LevelSystem
{
    public static class PlaytimeService
    {
        private static Timer _timer;
        private static readonly HashSet<int> _activePlayers = new();

        public static void Initialize()
        {
            _timer = new Timer(1000, true);
            _timer.Tick += OnTimerTick;
        }

        public static void Dispose()
        {
            _timer?.Dispose();
            _activePlayers.Clear();
        }

        public static void RegisterPlayer(Player player)
        {
            _activePlayers.Add(player.Id);
        }

        public static void UnregisterPlayer(Player player)
        {
            _activePlayers.Remove(player.Id);
        }

        private static void OnTimerTick(object sender, System.EventArgs e)
        {
            foreach (var playerId in _activePlayers)
            {
                var player = BasePlayer.Find(playerId) as Player;
                if (player == null || !player.IsCharLoaded) continue;

                player.Playtime.Seconds++;

                if (player.Playtime.Seconds >= 60)
                {
                    player.Playtime.Seconds = 0;
                    player.Playtime.Minutes++;

                    if (player.Playtime.Minutes >= 60)
                    {
                        player.Playtime.Minutes = 0;
                        player.Playtime.Hours++;
                        LevelService.AddPlaytimePoint(player);
                    }
                }
            }
        }
    }
}