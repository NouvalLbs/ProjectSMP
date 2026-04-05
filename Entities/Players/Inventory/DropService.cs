using ProjectSMP.Core;
using ProjectSMP.Entities.Players.Inventory.Data;
using ProjectSMP.Extensions;
using SampSharp.GameMode.SAMP;
using SampSharp.Streamer.World;
using System.Collections.Generic;
using System.Linq;

namespace ProjectSMP.Entities.Players.Inventory
{
    public static class DropService
    {
        private const float DropRadius = 3.0f;
        private static readonly List<DropPointData> DropPoints = new();

        public static void AddItemToDropPoint(Player player, string itemName, int amount, long durability)
        {
            var pos = player.Position;
            var existing = DropPoints.FirstOrDefault(dp =>
                dp.Position.DistanceTo(pos) <= DropRadius &&
                dp.VirtualWorld == player.VirtualWorld &&
                dp.Interior == player.Interior);

            if (existing != null)
            {
                existing.Items.Add(new ItemData { ItemName = itemName, Amount = amount, Durability = durability });
                UpdateLabel(existing);
            }
            else
            {
                var newPoint = new DropPointData
                {
                    Position = pos,
                    VirtualWorld = player.VirtualWorld,
                    Interior = player.Interior,
                    Items = new List<ItemData> { new() { ItemName = itemName, Amount = amount, Durability = durability } }
                };

                newPoint.Label = new DynamicTextLabel(
                    $"Loot Drop\n{{FFFFFF}}Items: {{00FF00}}{newPoint.Items.Count}",
                    Color.Orange,
                    pos,
                    10.0f,
                    interiorid: player.Interior,
                    worldid: player.VirtualWorld);

                DropPoints.Add(newPoint);
            }
        }

        public static DropPointData GetNearestDropPoint(Player player)
        {
            return DropPoints.FirstOrDefault(dp =>
                dp.Position.DistanceTo(player.Position) <= DropRadius &&
                dp.VirtualWorld == player.VirtualWorld &&
                dp.Interior == player.Interior);
        }

        public static void ShowDropPointItems(Player player, DropPointData dropPoint)
        {
            var rows = dropPoint.Items.Select((item, index) => new[]
            {
                item.ItemName,
                GetDurabilityText(item),
                item.Amount.ToString()
            }).ToArray();

            player.SetData("CurrentDropPoint", dropPoint);

            player.ShowTabList("Loot Drop", new[] { "Item", "Durability", "Amount" })
                .WithRows(rows)
                .WithButtons("Take", "Close")
                .Show(e =>
                {
                    if (e.DialogButton != SampSharp.GameMode.Definitions.DialogButton.Left)
                    {
                        player.SetData<DropPointData>("CurrentDropPoint", null);
                        return;
                    }

                    var dp = player.GetData<DropPointData>("CurrentDropPoint", null);
                    if (dp == null || e.ListItem >= dp.Items.Count) return;

                    var item = dp.Items[e.ListItem];
                    if (!InventoryService.CanReceiveItem(player, item.ItemName, item.Amount))
                    {
                        player.SendClientMessage(SampSharp.GameMode.SAMP.Color.White, $"{Msg.Inventory} Inventory penuh.");
                        return;
                    }

                    InventoryService.AddItem(player, item.ItemName, item.Amount, item.Durability);
                    dp.Items.RemoveAt(e.ListItem);

                    player.SendClientMessage(SampSharp.GameMode.SAMP.Color.White, $"{Msg.Inventory} Kamu mengambil {{ebe6ae}}{item.ItemName}{{FFFFFF}} x{item.Amount}.");

                    if (dp.Items.Count == 0)
                    {
                        dp.Label?.Dispose();
                        DropPoints.Remove(dp);
                        player.SetData<DropPointData>("CurrentDropPoint", null);
                    }
                    else
                    {
                        UpdateLabel(dp);
                        ShowDropPointItems(player, dp);
                    }
                });
        }

        private static void UpdateLabel(DropPointData dropPoint)
        {
            if (dropPoint.Label != null)
                dropPoint.Label.Text = $"Loot Drop\n{{FFFFFF}}Items: {{00FF00}}{dropPoint.Items.Count}";
        }

        private static string GetDurabilityText(ItemData item)
        {
            var def = ItemDatabase.Get(item.ItemName);
            if (def?.DurabilityDuration <= 0) return "-";

            if (item.Durability <= 0) return "-";

            var now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var remaining = item.Durability - now;

            if (remaining <= 0) return "0%";

            var percent = (remaining * 100) / def.DurabilityDuration;
            return $"{percent}%";
        }
    }
}