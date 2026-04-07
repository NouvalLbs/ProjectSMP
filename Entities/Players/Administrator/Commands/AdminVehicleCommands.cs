using ProjectSMP.Core;
using ProjectSMP.Entities.Players.Administrator.Data;
using ProjectSMP.Features.PreviewModelDialog;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;
using System.Collections.Generic;
using System.Linq;

namespace ProjectSMP.Entities.Players.Administrator.Commands
{
    public class AdminVehicleCommands : AdminCommandBase
    {
        private const int DialogId = 9001;

        private static readonly string[] CategoryNames =
        {
            "Cars", "Bikes", "Aircraft", "Boats", "Heavy", "Public Service", "All Vehicles"
        };

        [Command("aveh")]
        public static void AVeh(Player player, string input = "")
        {
            if (!CheckAdmin(player, 1)) return;

            if (string.IsNullOrEmpty(input))
            {
                ShowCategoryDialog(player);
                return;
            }

            if (int.TryParse(input, out var modelId))
            {
                if (!VehicleService.IsValidModel(modelId))
                {
                    player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Model ID {{00FFFF}}{modelId}{{FFFFFF}} tidak valid!");
                    return;
                }
                DoSpawn(player, modelId);
                return;
            }

            var results = VehicleService.Search(input);
            if (results.Count == 0)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Kendaraan '{input}' tidak ditemukan.");
                return;
            }

            if (results.Count == 1)
            {
                DoSpawn(player, results[0].ModelId);
                return;
            }

            ShowPreviewDialog(player, results, null);
        }

        private static void ShowCategoryDialog(Player player)
        {
            player.ShowList("Spawn Kendaraan - Kategori", CategoryNames)
                .WithButtons("Pilih", "Batal")
                .Show(e =>
                {
                    if (e.DialogButton != SampSharp.GameMode.Definitions.DialogButton.Left) return;
                    var catId = e.ListItem == 6 ? -1 : e.ListItem;
                    var vehicles = VehicleService.GetByCategory(catId);
                    ShowPreviewDialog(player, vehicles, e.ListItem);
                });
        }

        private static void ShowPreviewDialog(Player player, List<VehicleModelInfo> vehicles, int? fromCatIndex)
        {
            var items = vehicles.Select(v => new PreviewModelItem
            {
                ModelId = v.ModelId,
                Text = v.Name
            }).ToList();

            PreviewModelDialog.Show(player, DialogId, "Spawn Kendaraan", items, "Spawn", "Kembali",
                e =>
                {
                    if (!e.Accepted)
                    {
                        ShowCategoryDialog(player);
                        return;
                    }
                    DoSpawn(player, e.ModelId);
                });
        }

        private static void DoSpawn(Player player, int modelId)
        {
            AdminVehicleService.Spawn(player, modelId);
            var name = VehicleService.GetVehicleName(modelId);
            player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Kendaraan {{00FFFF}}{name}{{FFFFFF}} (ID:{modelId}) berhasil di-spawn!");
        }
    }
}