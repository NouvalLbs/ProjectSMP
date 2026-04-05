#nullable enable
using System;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;

namespace ProjectSMP.Entities.Players.Delay
{
    public static class DelayService
    {
        private static Timer? _delayTimer;

        public static void Initialize()
        {
            _delayTimer = new Timer(60000, true);
            _delayTimer.Tick += OnDelayTick;
        }

        public static void Dispose()
        {
            _delayTimer?.Dispose();
        }

        private static void OnDelayTick(object sender, EventArgs e)
        {
            foreach (var bp in BasePlayer.All)
            {
                if (bp is Player p && p.IsCharLoaded && !p.IsDisposed)
                {
                    ReduceAllDelays(p, 1);
                }
            }
        }

        public static void SetQuitJobDelay(Player player, int days)
        {
            var expireTime = DateTime.UtcNow.AddDays(days);
            player.Delays.QuitJob = expireTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static bool HasQuitJobDelay(Player player)
        {
            if (string.IsNullOrEmpty(player.Delays.QuitJob))
                return false;

            if (!DateTime.TryParse(player.Delays.QuitJob, out var expireTime))
                return false;

            return DateTime.UtcNow < expireTime;
        }

        public static TimeSpan GetQuitJobRemainingTime(Player player)
        {
            if (string.IsNullOrEmpty(player.Delays.QuitJob) ||
                !DateTime.TryParse(player.Delays.QuitJob, out var expireTime))
                return TimeSpan.Zero;

            var remaining = expireTime - DateTime.UtcNow;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        public static void SetJobDelay(Player player, string jobType, int delayMinutes)
        {
            switch (jobType.ToLower())
            {
                case "lumber": player.Delays.Lumber = delayMinutes; break;
                case "product": player.Delays.Product = delayMinutes; break;
                case "truckerhauling": player.Delays.TruckerHauling = delayMinutes; break;
                case "truckermission": player.Delays.TruckerMission = delayMinutes; break;
                case "truckercontainer": player.Delays.TruckerContainer = delayMinutes; break;
                case "truckercrate": player.Delays.TruckerCrate = delayMinutes; break;
                case "forager": player.Delays.Forager = delayMinutes; break;
                case "farm": player.Delays.Farm = delayMinutes; break;
                case "sweeper": player.Delays.Sweeper = delayMinutes; break;
                case "courir": player.Delays.Courir = delayMinutes; break;
                case "forklifter": player.Delays.Forklifter = delayMinutes; break;
                case "bus": player.Delays.Bus = delayMinutes; break;
                case "trashmaster": player.Delays.Trashmaster = delayMinutes; break;
                case "mower": player.Delays.Mower = delayMinutes; break;
                case "fisherman": player.Delays.Fisherman = delayMinutes; break;
                case "honey": player.Delays.Honey = delayMinutes; break;
                case "miner": player.Delays.Miner = delayMinutes; break;
            }
        }

        public static int GetJobDelay(Player player, string jobType)
        {
            return jobType.ToLower() switch
            {
                "lumber" => player.Delays.Lumber,
                "product" => player.Delays.Product,
                "truckerhauling" => player.Delays.TruckerHauling,
                "truckermission" => player.Delays.TruckerMission,
                "truckercontainer" => player.Delays.TruckerContainer,
                "truckercrate" => player.Delays.TruckerCrate,
                "forager" => player.Delays.Forager,
                "farm" => player.Delays.Farm,
                "sweeper" => player.Delays.Sweeper,
                "courir" => player.Delays.Courir,
                "forklifter" => player.Delays.Forklifter,
                "bus" => player.Delays.Bus,
                "trashmaster" => player.Delays.Trashmaster,
                "mower" => player.Delays.Mower,
                "fisherman" => player.Delays.Fisherman,
                "honey" => player.Delays.Honey,
                "miner" => player.Delays.Miner,
                _ => 0
            };
        }

        public static bool HasJobDelay(Player player, string jobType)
        {
            return GetJobDelay(player, jobType) > 0;
        }

        public static void ReduceAllDelays(Player player, int minutesElapsed)
        {
            ProcessJobDelay(player, "Lumber", player.Delays.Lumber, v => player.Delays.Lumber = v, minutesElapsed);
            ProcessJobDelay(player, "Product", player.Delays.Product, v => player.Delays.Product = v, minutesElapsed);
            ProcessJobDelay(player, "Trucker Hauling", player.Delays.TruckerHauling, v => player.Delays.TruckerHauling = v, minutesElapsed);
            ProcessJobDelay(player, "Trucker Mission", player.Delays.TruckerMission, v => player.Delays.TruckerMission = v, minutesElapsed);
            ProcessJobDelay(player, "Trucker Container", player.Delays.TruckerContainer, v => player.Delays.TruckerContainer = v, minutesElapsed);
            ProcessJobDelay(player, "Trucker Crate", player.Delays.TruckerCrate, v => player.Delays.TruckerCrate = v, minutesElapsed);
            ProcessJobDelay(player, "Forager", player.Delays.Forager, v => player.Delays.Forager = v, minutesElapsed);
            ProcessJobDelay(player, "Farmer", player.Delays.Farm, v => player.Delays.Farm = v, minutesElapsed);
            ProcessJobDelay(player, "Sweeper", player.Delays.Sweeper, v => player.Delays.Sweeper = v, minutesElapsed);
            ProcessJobDelay(player, "Courier", player.Delays.Courir, v => player.Delays.Courir = v, minutesElapsed);
            ProcessJobDelay(player, "Forklifter", player.Delays.Forklifter, v => player.Delays.Forklifter = v, minutesElapsed);
            ProcessJobDelay(player, "Bus Driver", player.Delays.Bus, v => player.Delays.Bus = v, minutesElapsed);
            ProcessJobDelay(player, "Trashmaster", player.Delays.Trashmaster, v => player.Delays.Trashmaster = v, minutesElapsed);
            ProcessJobDelay(player, "Mower", player.Delays.Mower, v => player.Delays.Mower = v, minutesElapsed);
            ProcessJobDelay(player, "Fisherman", player.Delays.Fisherman, v => player.Delays.Fisherman = v, minutesElapsed);
            ProcessJobDelay(player, "Honey", player.Delays.Honey, v => player.Delays.Honey = v, minutesElapsed);
            ProcessJobDelay(player, "Miner", player.Delays.Miner, v => player.Delays.Miner = v, minutesElapsed);
        }

        private static void ProcessJobDelay(Player player, string jobName, int currentDelay, Action<int> updateDelay, int minutesElapsed)
        {
            if (currentDelay > 0)
            {
                int newDelay = Math.Max(0, currentDelay - minutesElapsed);
                updateDelay(newDelay);

                if (newDelay == 0 && player.IsConnected)
                {
                    player.SendClientMessage(Color.White, $"{{C6E2FF}}[{jobName.ToUpper()}]:{{FFFFFF}} You can now work as a {{FFFF00}}{jobName}{{FFFFFF}} again.");
                }
            }
        }
    }
}