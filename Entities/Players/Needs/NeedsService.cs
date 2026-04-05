using ProjectSMP.Plugins.CEF;
using SampSharp.GameMode.Definitions;

namespace ProjectSMP.Entities.Players.Needs
{
    public static class NeedsService
    {
        public static void Initialize()
        {
            NeedsUpdateService.Initialize();
            NeedsEffectService.Initialize();
        }

        public static void Dispose()
        {
            NeedsUpdateService.Dispose();
            NeedsEffectService.Dispose();
        }

        public static void OnPlayerSpawn(Player player)
        {
            NeedsUpdateService.RegisterPlayer(player);
            NeedsEffectService.RegisterPlayer(player);

            if (player.Settings.HBEMode == 0)
            {
                CefService.EmitEvent(player.Id, "setHudVisible", new { visible = true });
                SendHudDataToCef(player);
            }
            else
            {
                NeedsHudManager.Initialize(player);
            }
        }

        public static void OnPlayerDisconnect(Player player)
        {
            NeedsUpdateService.UnregisterPlayer(player);
            NeedsEffectService.UnregisterPlayer(player);
            NeedsHudManager.Cleanup(player);
        }

        public static void SetHunger(Player player, float value)
        {
            if (value > 100) value = 100;
            else if (value < 0) value = 0;

            player.Vitals.Hunger = value;
            NeedsHudManager.UpdateHud(player);
        }

        public static void SetEnergy(Player player, float value)
        {
            if (value > 100) value = 100;
            else if (value < 0) value = 0;

            player.Vitals.Energy = value;
            NeedsHudManager.UpdateHud(player);
        }

        public static void RefreshHud(Player player)
        {
            if (player.Settings.HBEMode == 0) {
                SendHudDataToCef(player);
            } else {
                NeedsHudManager.RegenerateHud(player);
            }

            if (player.State == PlayerState.Driving)
                Vehicles.Speedo.SpeedometerService.Regenerate(player);
        }

        public static void SendHudDataToCef(Player player)
        {
            var hudData = new
            {
                ShowHealth = player.Settings.ShowHealth,
                HealthValue = (int)player.Vitals.Health,

                ShowArmour = player.Settings.ShowArmour,
                ArmourValue = (int)player.Vitals.Armour,

                ShowHunger = player.Settings.ShowHunger,
                HungerValue = (int)player.Vitals.Hunger,

                ShowThirst = player.Settings.ShowThirst,
                ThirstValue = (int)player.Vitals.Energy,

                ShowStress = player.Settings.ShowStress,
                StressValue = (int)player.Vitals.Stress
            };

            CefService.EmitEvent(player.Id, "updateHud", hudData);
        }
    }
}