using SampSharp.GameMode.Pools;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Plugins.RealtimeClock
{
    internal static class WeatherManager
    {
        private static readonly int[] _fineWeatherIds = { 2, 3, 4, 5, 6, 7, 12, 13, 14, 15, 17 };
        private static readonly int[] _wetWeatherIds = { 8 };
        private static readonly Random _random = new();
        private static int _currentWeather;

        public static void Init()
        {
            var nextWeather = _random.Next(91);
            _currentWeather = nextWeather < 70
                ? _fineWeatherIds[_random.Next(_fineWeatherIds.Length)]
                : _wetWeatherIds[0];
        }

        public static void OnTimeUpdate(int hour, int minute, int second)
        {
            if (minute == 0 && second == 0)
            {
                UpdateWeather();
            }
        }

        public static void SyncPlayerWeather(int playerId)
        {
            var player = BasePlayer.Find(playerId);
            if (player != null && !player.IsDisposed)
            {
                player.SetWeather(_currentWeather);
            }
        }

        private static void UpdateWeather()
        {
            var nextWeather = _random.Next(91);
            _currentWeather = nextWeather < 70
                ? _fineWeatherIds[_random.Next(_fineWeatherIds.Length)]
                : _wetWeatherIds[_random.Next(_wetWeatherIds.Length)];

            foreach (var player in BasePlayer.All)
            {
                if (!player.IsDisposed)
                {
                    player.SetWeather(_currentWeather);
                }
            }
        }

        public static int GetCurrentWeather() => _currentWeather;
    }
}