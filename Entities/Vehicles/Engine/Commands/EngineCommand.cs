using ProjectSMP.Core;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;
using SampSharp.GameMode.Definitions;

namespace ProjectSMP.Entities.Vehicles.Engine.Commands
{
    public class EngineCommand
    {
        [Command("engine", Shortcut = "eng")]
        public static void OnEngineCommand(Player player)
        {
            if (player.State != PlayerState.Driving)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Kamu harus berada di dalam kendaraan.");
                return;
            }
            EngineService.ToggleEngine(player);
        }
    }
}