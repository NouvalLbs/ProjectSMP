using ProjectSMP.Core;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;

namespace ProjectSMP.Entities.Players.Settings.Commands
{
    public class SettingsCommands
    {
        [Command("settings")]
        public static void Settings(Player player)
        {
            if (!player.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Kamu belum login.");
                return;
            }
            SettingsService.ShowMainSettings(player);
        }

        [Command("hud")]
        public static void Hud(Player player)
        {
            if (!player.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Kamu belum login.");
                return;
            }
            SettingsService.ShowHudSettings(player);
        }

        [Command("toggle", Shortcut = "tog")]
        public static void Toggle(Player player)
        {
            if (!player.IsCharLoaded)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Kamu belum login.");
                return;
            }
            SettingsService.ShowToggleSettings(player);
        }
    }
}