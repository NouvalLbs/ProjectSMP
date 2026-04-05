using ProjectSMP.Core;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;
using System.Linq;

namespace ProjectSMP.Entities.Players.Administrator.Commands
{
    public class VehicleCommands
    {
        private static bool CheckAdmin(Player player, int level)
        {
            if (player.Admin < level)
            {
                player.SendClientMessage(Color.White, "{b9b9b9}Command tidak ada, gunakan '/help'.");
                return false;
            }
            if (!player.AdminOnDuty)
            {
                player.SendClientMessage(Color.White, $"{Msg.AdmCmd} Command tidak dapat digunakan ketika kamu tidak duty.");
                return false;
            }
            return true;
        }

        [Command("vehname")]
        public static void VehName(Player player, string searchQuery = "")
        {
            if (!CheckAdmin(player, 3)) return;

            if (string.IsNullOrEmpty(searchQuery))
            {
                ShowAllVehicles(player);
                return;
            }

            var results = VehicleService.Search(searchQuery);

            if (results.Count == 0)
            {
                player.SendClientMessage(Color.White,
                    $"{Msg.AdmCmd} Tidak ditemukan kendaraan dengan nama '{searchQuery}'.");
                return;
            }

            if (results.Count == 1)
            {
                player.SendClientMessage(Color.White,
                    $"{{FF6347}}>{{FFFFFF}} {results[0].Name} | VehicleId: {results[0].ModelId}");
                return;
            }

            var rows = results.Select(v => new[] { v.Name, v.ModelId.ToString() }).ToArray();

            player.ShowTabList(
                "Vehicle Search Results",
                new[] { "Vehicle Name", "Model ID" })
                .WithRows(rows)
                .WithButtons("Close", "")
                .Show();

            player.SendClientMessage(Color.White,
                $"{Msg.AdmCmd} Ditemukan {results.Count} kendaraan dengan kata kunci '{searchQuery}'.");
        }

        [Command("vehmodels")]
        public static void VehModels(Player player, string input = "")
        {
            if (!CheckAdmin(player, 3)) return;

            if (string.IsNullOrEmpty(input))
            {
                ShowCategoryDialog(player);
                return;
            }

            if (int.TryParse(input, out var modelId) && VehicleService.IsValidModel(modelId))
            {
                var name = VehicleService.GetVehicleName(modelId);
                player.SendClientMessage(Color.White,
                    $"{{FF6347}}>{{FFFFFF}} VehicleId: {modelId} | Name: {name}");
                return;
            }

            var results = VehicleService.Search(input);

            if (results.Count == 0)
            {
                player.SendClientMessage(Color.White,
                    $"{Msg.AdmCmd} Tidak ditemukan kendaraan dengan nama '{input}'.");
                return;
            }

            if (results.Count == 1)
            {
                player.SendClientMessage(Color.White,
                    $"{Msg.AdmCmd} VehicleId: {results[0].ModelId} | Name: {results[0].Name}");
                return;
            }

            var rows = results.Select(v => new[] { v.Name, v.ModelId.ToString() }).ToArray();

            player.ShowTabList(
                "Vehicle Search Results",
                new[] { "Vehicle Name", "Model ID" })
                .WithRows(rows)
                .WithButtons("Close", "")
                .Show();

            player.SendClientMessage(Color.White,
                $"{Msg.AdmCmd} Ditemukan {results.Count} kendaraan dengan kata kunci '{input}'.");
        }

        private static void ShowCategoryDialog(Player player)
        {
            player.ShowTabList(
                "Vehicle Categories",
                new[] { "Category", "Description" })
                .WithRows(new[]
                {
                    new[] { "Cars", "Sedans, Sports Cars, Muscle Cars etc." },
                    new[] { "Bikes", "Motorcycles and Bicycles" },
                    new[] { "Aircraft", "Helicopters and Planes" },
                    new[] { "Boats", "Boats and Water Vehicles" },
                    new[] { "Heavy", "Trucks, Industrial and Construction" },
                    new[] { "Public", "Buses, Trains and Service Vehicles" },
                    new[] { "All", "All vehicle models" }
                })
                .WithButtons("Select", "Cancel")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left) return;

                    var categoryId = e.ListItem == 6 ? -1 : e.ListItem;
                    ShowVehiclesByCategory(player, categoryId);
                });
        }

        private static void ShowVehiclesByCategory(Player player, int categoryId)
        {
            var vehicles = VehicleService.GetByCategory(categoryId);
            var rows = vehicles.Select(v => new[] { v.Name, v.ModelId.ToString() }).ToArray();

            var categoryName = categoryId switch
            {
                0 => "Cars",
                1 => "Bikes",
                2 => "Aircraft",
                3 => "Boats",
                4 => "Heavy Vehicles",
                5 => "Public Vehicles",
                _ => "All Vehicles"
            };

            player.ShowTabList(
                $"{categoryName} ({vehicles.Count})",
                new[] { "Vehicle Name", "Model ID" })
                .WithRows(rows)
                .WithButtons("Close", "")
                .Show();

            player.SendClientMessage(Color.White,
                $"{Msg.AdmCmd} Menampilkan daftar {vehicles.Count} kendaraan.");
        }

        private static void ShowAllVehicles(Player player)
        {
            var vehicles = VehicleService.GetByCategory(-1);
            var rows = vehicles.Select(v => new[] { v.Name, v.ModelId.ToString() }).ToArray();

            player.ShowTabList(
                $"All Vehicles ({vehicles.Count})",
                new[] { "Vehicle Name", "Model ID" })
                .WithRows(rows)
                .WithButtons("Close", "")
                .Show();

            player.SendClientMessage(Color.White,
                $"{Msg.AdmCmd} Menampilkan daftar {vehicles.Count} kendaraan.");
        }
    }
}