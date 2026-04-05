using SampSharp.GameMode.World;

namespace ProjectSMP.Plugins.RealtimeClock
{
    public static class PlayerExtensions
    {
        public static void FreezeWorldTime(this BasePlayer player)
        {
            RealtimeClockService.FreezeForPlayer(player.Id);
        }

        public static void UnfreezeWorldTime(this BasePlayer player)
        {
            RealtimeClockService.UnfreezeForPlayer(player.Id);
        }

        public static bool IsWorldTimeFrozen(this BasePlayer player)
        {
            return RealtimeClockService.IsPlayerFrozen(player.Id);
        }

        public static void SyncWorldTime(this BasePlayer player)
        {
            RealtimeClockService.SyncPlayerTime(player.Id);
        }

        public static void SyncWeather(this BasePlayer player)
        {
            WeatherManager.SyncPlayerWeather(player.Id);
        }

        public static int GetCurrentWeather()
        {
            return WeatherManager.GetCurrentWeather();
        }
    }
}