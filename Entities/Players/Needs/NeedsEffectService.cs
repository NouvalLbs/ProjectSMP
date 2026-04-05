using ProjectSMP.Core;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System.Collections.Generic;

namespace ProjectSMP.Entities.Players.Needs
{
    internal static class NeedsEffectService
    {
        private static SampSharp.GameMode.SAMP.Timer _timer;
        private static readonly Dictionary<int, int> _lastCheck = new();

        public static void Initialize()
        {
            _timer = new SampSharp.GameMode.SAMP.Timer(1000, true);
            _timer.Tick += OnTimerTick;
        }

        public static void Dispose()
        {
            if (_timer != null && _timer.IsRunning)
                _timer.Dispose();
            _lastCheck.Clear();
        }

        public static void RegisterPlayer(Player player)
        {
            _lastCheck[player.Id] = 0;
        }

        public static void UnregisterPlayer(Player player)
        {
            _lastCheck.Remove(player.Id);
        }

        private static void OnTimerTick(object sender, System.EventArgs e)
        {
            foreach (var kvp in _lastCheck)
            {
                var player = BasePlayer.Find(kvp.Key) as Player;
                if (player == null || player.IsDisposed || !player.IsCharLoaded) continue;

                ProcessEffects(player);
            }
        }

        private static void ProcessEffects(Player player)
        {
            if (player.Vitals.Hunger >= 10 || player.Vitals.Energy >= 10)
            {
                player.DrunkLevel = 0;
                return;
            }

            if (player.Vitals.Hunger <= 10 || player.Vitals.Energy <= 10)
            {
                if (player.Condition.Migrain > 1 || player.Condition.Fever > 0)
                {
                    player.DrunkLevel = 5000 * (player.Condition.Fever + 1);
                }

                if (player.Condition.Fever > 0)
                {
                    player.Condition.FeverTime++;
                    if (player.Condition.FeverTime > 10)
                    {
                        player.DrunkLevel = 5000 * (player.Condition.Fever + 1);
                        player.Condition.FeverTime = 0;
                    }
                }

                player.Condition.MigrainTime++;
                if (player.Condition.MigrainTime >= 300)
                {
                    if (player.Condition.Fever > 0 && player.Condition.Fever <= 2)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Sick} Kamu terkena demam tinggi, ini penyakit berbahaya, kamu harus segera pergi ke dokter!");
                        player.Condition.Fever++;
                    }
                    else if (player.Condition.Fever == 0 && player.Condition.Migrain < 4)
                    {
                        player.Condition.Migrain++;
                        if (player.Condition.Migrain < 3)
                        {
                            player.SendClientMessage(Color.White, $"{Msg.Sick} Kamu terkena sakit kepala, periksalah ke dokter untuk mengobati penyakitmu!");
                        }
                        else if (player.Condition.Migrain == 4)
                        {
                            player.SendClientMessage(Color.White, $"{Msg.Sick} Kamu terkena demam tinggi, ini penyakit berbahaya, kamu harus segera pergi ke dokter!");
                            player.Condition.Fever++;
                            player.Condition.Migrain = 0;
                        }
                    }
                    player.Condition.MigrainTime = 0;
                }
            }
        }
    }
}