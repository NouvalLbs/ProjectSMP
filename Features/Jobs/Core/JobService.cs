using ProjectSMP.Entities.Players.Character;
using ProjectSMP.Entities.Players.Delay;
using System;
using System.Linq;

namespace ProjectSMP.Features.Jobs.Core
{
    public static class JobService
    {
        private const int MaxJobs = 2;

        public static bool AddJob(Player player, string jobName)
        {
            if (string.IsNullOrWhiteSpace(jobName))
                return false;

            if (HasJob(player, jobName))
                return false;

            if (player.Jobs.Count >= MaxJobs)
                return false;

            player.Jobs.Add(new CharJob
            {
                JobName = jobName,
                RegisterDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });

            return true;
        }

        public static bool RemoveJob(Player player, string jobName)
        {
            if (string.IsNullOrWhiteSpace(jobName))
                return false;

            var job = player.Jobs.FirstOrDefault(j =>
                j.JobName.Equals(jobName, StringComparison.OrdinalIgnoreCase));

            if (job == null)
                return false;

            player.Jobs.Remove(job);
            DelayService.SetQuitJobDelay(player, 1);
            return true;
        }

        public static bool HasJob(Player player, string jobName)
        {
            if (string.IsNullOrWhiteSpace(jobName))
                return false;

            return player.Jobs.Any(j =>
                j.JobName.Equals(jobName, StringComparison.OrdinalIgnoreCase));
        }

        public static void ClearAllJobs(Player player)
        {
            player.Jobs.Clear();
        }

        public static string GetAllJobsString(Player player)
        {
            if (player.Jobs.Count == 0)
                return "None";

            return string.Join(", ", player.Jobs.Select(j => j.JobName));
        }

        public static int GetJobCount(Player player)
        {
            return player.Jobs.Count;
        }

        public static CharJob GetJob(Player player, string jobName)
        {
            return player.Jobs.FirstOrDefault(j =>
                j.JobName.Equals(jobName, StringComparison.OrdinalIgnoreCase));
        }

        public static CharJob[] GetAllJobs(Player player)
        {
            return player.Jobs.ToArray();
        }
    }
}