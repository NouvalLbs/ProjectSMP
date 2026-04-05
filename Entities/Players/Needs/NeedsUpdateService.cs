using ProjectSMP.Extensions;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System.Collections.Generic;

namespace ProjectSMP.Entities.Players.Needs
{
    internal static class NeedsUpdateService
    {
        private static SampSharp.GameMode.SAMP.Timer _timer;
        private static readonly Dictionary<int, int> _lastUpdate = new();

        public static void Initialize()
        {
            _timer = new SampSharp.GameMode.SAMP.Timer(1000, true);
            _timer.Tick += OnTimerTick;
        }

        public static void Dispose()
        {
            if (_timer != null && _timer.IsRunning)
                _timer.Dispose();
            _lastUpdate.Clear();
        }

        public static void RegisterPlayer(Player player)
        {
            _lastUpdate[player.Id] = 0;
        }

        public static void UnregisterPlayer(Player player)
        {
            _lastUpdate.Remove(player.Id);
        }

        private static void OnTimerTick(object sender, System.EventArgs e)
        {
            foreach (var kvp in _lastUpdate)
            {
                var player = BasePlayer.Find(kvp.Key) as Player;
                if (player == null || player.IsDisposed || !player.IsCharLoaded) continue;

                UpdateNeeds(player);
            }
        }

        private static void UpdateNeeds(Player player)
        {
            if (!player.IsCharLoaded) return;
            if (player.Condition.Injured != 0) return;
            var animIdx = player.AnimationIndex;
            player.GetKeys(out var keys, out _, out _);

            const float baseHunger = 0.03f * 0.2f;
            const float baseEnergy = 0.04f * 0.2f;

            var hungerAdjust = baseHunger;
            var energyAdjust = baseEnergy;

            if (animIdx == 43)
            {
                hungerAdjust *= 0.2f;
                energyAdjust *= 0.2f;
            }
            else if (animIdx == 1159)
            {
                hungerAdjust *= 1.1f;
                energyAdjust *= 1.1f;
            }
            else if (animIdx == 1195 || (animIdx == 1231 && (keys & Keys.Jump) != 0))
            {
                hungerAdjust *= 3.2f;
                energyAdjust *= 3.2f;
            }
            else if (animIdx == 1231)
            {
                if ((keys & Keys.Walk) != 0)
                {
                    hungerAdjust *= 1.2f;
                    energyAdjust *= 1.2f;
                }
                else if ((keys & Keys.Sprint) != 0)
                {
                    hungerAdjust *= 2.2f;
                    energyAdjust *= 2.2f;
                }
                else
                {
                    hungerAdjust *= 2.0f;
                    energyAdjust *= 2.0f;
                }
            }

            player.Vitals.Health = player.GetHealthSafe();
            player.Vitals.Armour = player.GetArmourSafe();

            if (player.Settings.HBEMode == 0)
            {
                SetPlayerHunger(player, player.Vitals.Hunger - hungerAdjust);
                SetPlayerEnergy(player, player.Vitals.Energy - energyAdjust);
                NeedsService.SendHudDataToCef(player);
            }
            else
            {
                SetPlayerHunger(player, player.Vitals.Hunger - hungerAdjust);
                SetPlayerEnergy(player, player.Vitals.Energy - energyAdjust);
                NeedsHudManager.UpdateHud(player);
            }
        }

        private static void SetPlayerHunger(Player player, float hunger)
        {
            if (hunger > 100) hunger = 100;
            else if (hunger < 0) hunger = 0;

            player.Vitals.Hunger = hunger;
            NeedsHudManager.UpdateHud(player);
        }

        private static void SetPlayerEnergy(Player player, float energy)
        {
            if (energy > 100) energy = 100;
            else if (energy < 0) energy = 0;

            player.Vitals.Energy = energy;
            NeedsHudManager.UpdateHud(player);
        }
    }
}