using SampSharp.GameMode.SAMP;

namespace ProjectSMP.Features.LevelSystem
{
    public static class LevelService
    {
        public static int GetPointsRequired(int level)
        {
            return level * 4;
        }

        public static int GetHoursRequired(int level)
        {
            return GetPointsRequired(level);
        }

        public static int GetCumulativeHoursRequired(int level)
        {
            var total = 0;
            for (var i = 1; i <= level; i++)
                total += GetPointsRequired(i);
            return total;
        }

        public static string CreateProgressBar(int current, int max, bool completed)
        {
            if (completed)
                return "{00FF00}llllllllll";

            var progress = (int)((float)current / max * 10);
            var bar = "{00FF00}";

            for (var i = 0; i < 10; i++)
            {
                if (i < progress)
                    bar += "l";
                else
                {
                    if (i == progress) bar += "{FFFFFF}";
                    bar += "l";
                }
            }

            return bar;
        }

        public static bool CanLevelUp(Player player)
        {
            return player.LevelPoints >= GetPointsRequired(player.Level);
        }

        public static void LevelUp(Player player)
        {
            if (!CanLevelUp(player))
                return;

            player.Level++;
            player.LevelPoints = 0;

            player.SendClientMessage(Color.Yellow, $"{{00FF00}}🎉 Selamat! Kamu naik ke Level {player.Level}!");
        }

        public static void AddPlaytimePoint(Player player)
        {
            player.LevelPoints++;
            player.SendClientMessage(Color.Yellow, $"{{FFFF00}}⏱ Kamu mendapatkan 1 Point Progress dari waktu bermain!");

            if (CanLevelUp(player))
                LevelUp(player);
        }
    }
}