using ProjectSMP.Core;
using ProjectSMP.Entities.Players.Inventory.Data;
using ProjectSMP.Extensions;
using ProjectSMP.Features.Dynamic.DynamicDoor;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProjectSMP.Entities.Players.Inventory
{
    public static class InventoryService
    {
        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

        public static void Initialize(Player player)
        {
            if (player.InventoryData == null)
            {
                player.InventoryData = new InventoryData
                {
                    Enabled = 1,
                    Slots = 32,
                    MaxWeight = 60000,
                    Inventory = new List<ItemData>()
                };
            }

            SyncMoneyToInventory(player);
            RemoveExpiredItems(player);
        }

        public static async Task SaveAsync(Player player)
        {
            if (!player.IsCharLoaded) return;

            var json = JsonSerializer.Serialize(player.InventoryData, JsonOpts);
            await DatabaseManager.ExecuteAsync(
                "UPDATE players SET backpack = @Data WHERE citizen_id = @CitizenId",
                new { Data = json, CitizenId = player.CitizenId });
        }

        public static bool CanReceiveItem(Player player, string itemName, int amount)
        {
            if (!ItemDatabase.Exists(itemName)) return false;

            var def = ItemDatabase.Get(itemName);
            var totalWeight = GetTotalWeight(player);
            var itemWeight = def.Weight * amount;

            if (totalWeight + itemWeight > player.InventoryData.MaxWeight)
                return false;

            var requiredSlots = CalculateRequiredSlots(player, itemName, amount);
            var usedSlots = player.InventoryData.Inventory.Count;

            return usedSlots + requiredSlots <= player.InventoryData.Slots;
        }

        public static bool AddItem(Player player, string itemName, int amount, long durability = 0)
        {
            if (!ItemDatabase.Exists(itemName)) return false;
            if (!CanReceiveItem(player, itemName, amount)) return false;

            var def = ItemDatabase.Get(itemName);

            if (durability == 0 && def.DurabilityDuration > 0)
                durability = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + def.DurabilityDuration;

            var inventory = player.InventoryData.Inventory;

            foreach (var item in inventory.Where(i => i.ItemName == itemName && i.Durability == durability))
            {
                var remaining = def.ItemStack - item.Amount;
                if (amount <= remaining)
                {
                    item.Amount += amount;
                    return true;
                }
                item.Amount = def.ItemStack;
                amount -= remaining;
            }

            while (amount > 0)
            {
                var newItem = new ItemData
                {
                    ItemName = itemName,
                    Amount = Math.Min(amount, def.ItemStack),
                    Durability = durability
                };
                inventory.Add(newItem);
                amount -= newItem.Amount;
            }

            return true;
        }

        public static bool RemoveItem(Player player, string itemName, int amount)
        {
            var inventory = player.InventoryData.Inventory;
            var items = inventory.Where(i => i.ItemName == itemName).ToList();

            foreach (var item in items)
            {
                if (item.Amount >= amount)
                {
                    item.Amount -= amount;
                    if (item.Amount <= 0)
                        inventory.Remove(item);
                    return true;
                }
                amount -= item.Amount;
                inventory.Remove(item);
            }

            return amount == 0;
        }

        public static bool RemoveItemByIndex(Player player, int index, int amount)
        {
            var inventory = player.InventoryData.Inventory;
            if (index < 0 || index >= inventory.Count) return false;

            var item = inventory[index];
            if (item.Amount >= amount)
            {
                item.Amount -= amount;
                if (item.Amount <= 0)
                    inventory.RemoveAt(index);
                return true;
            }

            inventory.RemoveAt(index);
            return false;
        }

        public static int GetItemCount(Player player, string itemName)
        {
            return player.InventoryData.Inventory
                .Where(i => i.ItemName == itemName)
                .Sum(i => i.Amount);
        }

        public static int GetTotalWeight(Player player)
        {
            return player.InventoryData.Inventory.Sum(item =>
            {
                var def = ItemDatabase.Get(item.ItemName);
                return def?.Weight * item.Amount ?? 0;
            });
        }

        public static void Organize(Player player)
        {
            var inventory = player.InventoryData.Inventory;
            var money = inventory.FirstOrDefault(i => i.ItemName == "Money");

            var sorted = inventory.Where(i => i.ItemName != "Money")
                .OrderBy(i => i.ItemName)
                .ThenBy(i => i.Durability)
                .ToList();

            inventory.Clear();
            if (money != null) inventory.Add(money);
            inventory.AddRange(sorted);
        }

        public static void SyncMoneyToInventory(Player player)
        {
            RemoveItem(player, "Money", GetItemCount(player, "Money"));
            if (player.CharMoney > 0)
                AddItem(player, "Money", player.CharMoney, 0);
        }

        public static void RemoveExpiredItems(Player player)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var expired = player.InventoryData.Inventory
                .Where(i => i.Durability > 0 && i.Durability <= now)
                .ToList();

            foreach (var item in expired)
            {
                player.InventoryData.Inventory.Remove(item);
                player.SendClientMessage(Color.White, $"{Msg.Inventory} Item {{ebe6ae}}{item.ItemName}{{FFFFFF}} telah kadaluarsa.");
            }
        }

        private class InventoryGroupData
        {
            public string Name { get; set; }
            public int Total { get; set; }
            public bool Mixed { get; set; }
        }

        public static void ShowInventory(Player player)
        {
            Organize(player);
            SyncMoneyToInventory(player);
            RemoveExpiredItems(player);

            var grouped = player.InventoryData.Inventory
                .GroupBy(i => i.ItemName)
                .Select(g => new InventoryGroupData
                {
                    Name = g.Key,
                    Total = g.Sum(i => i.Amount),
                    Mixed = g.Select(i => i.Durability).Distinct().Count() > 1
                })
                .ToList();

            var rows = new List<string[]>();
            var mapping = new Dictionary<int, string>();

            for (int i = 0; i < grouped.Count; i++)
            {
                var item = grouped[i];
                var def = ItemDatabase.Get(item.Name);
                var durText = item.Mixed ? "Mixed" : GetDurabilityText(player, item.Name);

                if (item.Name == "Money")
                    rows.Add(new[] { $"{{33CC33}}Money", durText, $"{{33CC33}}{Utilities.GroupDigits(item.Total)}" });
                else
                    rows.Add(new[] { item.Name, durText, item.Total.ToString() });

                mapping[i] = item.Name;
            }

            player.SetData("InventoryMapping", mapping);
            player.SetData("InventoryGrouped", grouped);

            var totalWeight = GetTotalWeight(player);
            var maxWeight = player.InventoryData.MaxWeight;
            var title = $"{player.CharInfo.Username}'s Inventory [{grouped.Count}/{player.InventoryData.Slots}] [{totalWeight / 1000f:F2}/{maxWeight / 1000f:F2} Kg]";

            player.ShowTabList(title, new[] { "Item", "Durability", "Amount" })
                .WithRows(rows.ToArray())
                .WithButtons("Select", "Close")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        player.SetData<Dictionary<int, string>>("InventoryMapping", null);
                        player.SetData<object>("InventoryGrouped", null);
                        return;
                    }

                    var map = player.GetData<Dictionary<int, string>>("InventoryMapping", null);
                    var grp = player.GetData<List<InventoryGroupData>>("InventoryGrouped", null);

                    if (map == null || grp == null || !map.ContainsKey(e.ListItem)) return;

                    var selectedName = map[e.ListItem];
                    var selectedGroup = grp[e.ListItem];

                    if (selectedGroup.Mixed)
                    {
                        ShowMixedItemDialog(player, selectedName);
                    }
                    else
                    {
                        var firstIndex = player.InventoryData.Inventory.FindIndex(i => i.ItemName == selectedName);
                        ShowItemActions(player, selectedName, firstIndex);
                    }
                });
        }

        private static void ShowMixedItemDialog(Player player, string itemName)
        {
            var items = player.InventoryData.Inventory
                .Select((item, index) => new { item, index })
                .Where(x => x.item.ItemName == itemName)
                .ToList();

            var rows = items.Select(x =>
            {
                var durText = GetDurabilityText(player, x.item.ItemName, x.index);
                return new[] { itemName, durText, x.item.Amount.ToString() };
            }).ToArray();

            var mapping = items.ToDictionary(x => items.IndexOf(x), x => x.index);
            player.SetData("MixedMapping", mapping);

            player.ShowTabList($"Inventory > Mixed {itemName}", new[] { "Item", "Durability", "Amount" })
                .WithRows(rows)
                .WithButtons("Select", "Back")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        ShowInventory(player);
                        return;
                    }

                    var map = player.GetData<Dictionary<int, int>>("MixedMapping", null);
                    if (map == null || !map.ContainsKey(e.ListItem)) return;

                    ShowItemActions(player, itemName, map[e.ListItem]);
                });
        }

        private static void ShowItemActions(Player player, string itemName, int inventoryIndex)
        {
            var def = ItemDatabase.Get(itemName);
            if (def == null) return;

            var actions = new List<string>();
            if (def.CanUsed) actions.Add("Use");
            if (def.CanGive) actions.Add("Give");
            if (def.CanDrop) actions.Add("Drop");
            if (def.CanDetail) actions.Add("Detail");

            if (actions.Count == 0)
            {
                ShowInventory(player);
                return;
            }

            player.SetData("ItemActionName", itemName);
            player.SetData("ItemActionIndex", inventoryIndex);

            player.ShowList($"Inventory > {itemName}", actions.ToArray())
                .WithButtons("Select", "Cancel")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        ShowInventory(player);
                        return;
                    }

                    var action = actions[e.ListItem];
                    HandleItemAction(player, action);
                });
        }

        private static void HandleItemAction(Player player, string action)
        {
            var itemName = player.GetData("ItemActionName", "");
            var index = player.GetData("ItemActionIndex", -1);

            switch (action)
            {
                case "Use":
                    UseItem(player, itemName, index);
                    break;
                case "Give":
                    ShowGiveDialog(player);
                    break;
                case "Drop":
                    ShowDropDialog(player);
                    break;
                case "Detail":
                    ShowDetailDialog(player);
                    break;
            }
        }

        private static void UseItem(Player player, string itemName, int index)
        {
            var inventory = player.InventoryData.Inventory;
            if (index < 0 || index >= inventory.Count) return;

            var item = inventory[index];
            if (item.Durability > 0 && item.Durability <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                player.SendClientMessage(Color.White, $"{Msg.Inventory} Item sudah kadaluarsa!");
                return;
            }

            var def = ItemDatabase.Get(itemName);
            if (def == null) return;

            if (def.ProgDur > 0)
            {
                Features.ProgressBar.ProgressBarService.StartProgress(
                    player,
                    duration: def.ProgDur,
                    text: def.ProgText,
                    callbackType: Features.ProgressBar.Data.ProgressCallbackType.UseItem,
                    animIndex: def.ProgAnimIndex,
                    animLib: def.ProgAnimLib,
                    animName: def.ProgAnimName,
                    itemSlot: index,
                    itemName: itemName
                );
                return;
            }

            ApplyItemEffect(player, itemName, index);
        }

        public static void OnItemUseComplete(Player player, string itemName, int index)
        {
            ApplyItemEffect(player, itemName, index);
        }

        private static void ApplyItemEffect(Player player, string itemName, int index)
        {
            var inventory = player.InventoryData.Inventory;
            if (index < 0 || index >= inventory.Count) return;

            switch (itemName)
            {
                case "Snack":
                    RemoveItemByIndex(player, index, 1);
                    player.Vitals.Hunger = Math.Min(100, player.Vitals.Hunger + 10);
                    player.SendClientMessage(Color.White, $"{Msg.Inventory} Kamu mengkonsumsi Snack! {{fd9804}}+10 Hunger");
                    break;

                case "Sprunk":
                    RemoveItemByIndex(player, index, 1);
                    player.Vitals.Energy = Math.Min(100, player.Vitals.Energy + 10);
                    player.SendClientMessage(Color.White, $"{Msg.Inventory} Kamu meminum Sprunk! {{00c5f6}}+10 Thirst");
                    break;
            }
        }

        private static void ShowGiveDialog(Player player)
        {
            var itemName = player.GetData("ItemActionName", "");
            var index = player.GetData("ItemActionIndex", -1);
            var inventory = player.InventoryData.Inventory;

            if (index < 0 || index >= inventory.Count) return;

            var nearbyPlayers = SampSharp.GameMode.World.BasePlayer.All
                .OfType<Player>()
                .Where(p => p.IsConnected && p.Id != player.Id && p.IsCharLoaded &&
                           p.Position.DistanceTo(player.Position) <= 8.0f)
                .ToList();

            if (nearbyPlayers.Count == 0)
            {
                player.SendClientMessage(Color.White, $"{Msg.Inventory} Tidak ada player di sekitar.");
                ShowInventory(player);
                return;
            }

            var rows = nearbyPlayers.Select(p => new[] { $"{p.CharInfo.Username} - (( Id:{p.Id} ))" }).ToArray();
            var mapping = nearbyPlayers.Select((p, i) => new { i, p.Id }).ToDictionary(x => x.i, x => x.Id);

            player.SetData("GiveMapping", mapping);

            player.ShowList($"Inventory > Give {itemName}", rows.Select(r => r[0]).ToArray())
                .WithButtons("Select", "Cancel")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        ShowInventory(player);
                        return;
                    }

                    var map = player.GetData<Dictionary<int, int>>("GiveMapping", null);
                    if (map == null || !map.ContainsKey(e.ListItem)) return;

                    var targetId = map[e.ListItem];
                    player.SetData("GiveTargetId", targetId);
                    ShowGiveAmountDialog(player);
                });
        }

        private static void ShowGiveAmountDialog(Player player)
        {
            var itemName = player.GetData("ItemActionName", "");
            var index = player.GetData("ItemActionIndex", -1);
            var inventory = player.InventoryData.Inventory;

            if (index < 0 || index >= inventory.Count) return;

            var item = inventory[index];

            player.ShowInput(
                $"Inventory > Give {itemName}",
                $"{{99b3c9}}Item: {itemName}\nDimiliki: {item.Amount}\n\nInput jumlah yang ingin di give")
                .WithButtons("Give", "Cancel")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        ShowInventory(player);
                        return;
                    }

                    if (!int.TryParse(e.InputText, out var amount) || amount <= 0)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Inventory} Input tidak valid.");
                        ShowGiveAmountDialog(player);
                        return;
                    }

                    if (amount > item.Amount)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Inventory} Jumlah tidak cukup.");
                        ShowGiveAmountDialog(player);
                        return;
                    }

                    var targetId = player.GetData("GiveTargetId", -1);
                    var target = SampSharp.GameMode.World.BasePlayer.Find(targetId) as Player;

                    if (target == null || !target.IsConnected)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Inventory} Player tidak ditemukan.");
                        ShowInventory(player);
                        return;
                    }

                    if (!CanReceiveItem(target, itemName, amount))
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Inventory} Inventory {target.CharInfo.Username} penuh.");
                        ShowInventory(player);
                        return;
                    }

                    var durability = item.Durability;
                    RemoveItemByIndex(player, index, amount);
                    AddItem(target, itemName, amount, durability);

                    player.SendClientMessage(Color.White, $"{Msg.Inventory} Kamu memberikan {{ebe6ae}}{itemName}{{FFFFFF}} x{amount} ke {{ebe6ae}}{target.CharInfo.Username}{{FFFFFF}}.");
                    target.SendClientMessage(Color.White, $"{Msg.Inventory} {{ebe6ae}}{player.CharInfo.Username}{{FFFFFF}} memberikan {{ebe6ae}}{itemName}{{FFFFFF}} x{amount} ke kamu.");

                    ShowInventory(player);
                });
        }

        private static void ShowDropDialog(Player player)
        {
            var itemName = player.GetData("ItemActionName", "");
            var index = player.GetData("ItemActionIndex", -1);
            var inventory = player.InventoryData.Inventory;

            if (index < 0 || index >= inventory.Count) return;

            var item = inventory[index];

            player.ShowInput(
                $"Inventory > Drop {itemName}",
                $"{{99b3c9}}Item: {itemName}\nDimiliki: {item.Amount}\n\nInput jumlah yang ingin di drop")
                .WithButtons("Drop", "Cancel")
                .Show(e =>
                {
                    if (e.DialogButton != DialogButton.Left)
                    {
                        ShowInventory(player);
                        return;
                    }

                    if (!int.TryParse(e.InputText, out var amount) || amount <= 0)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Inventory} Input tidak valid.");
                        ShowDropDialog(player);
                        return;
                    }

                    if (amount > item.Amount)
                    {
                        player.SendClientMessage(Color.White, $"{Msg.Inventory} Jumlah tidak cukup.");
                        ShowDropDialog(player);
                        return;
                    }

                    var durability = item.Durability;
                    RemoveItemByIndex(player, index, amount);
                    DropService.AddItemToDropPoint(player, itemName, amount, durability);

                    player.SendClientMessage(Color.White, $"{Msg.Inventory} Kamu drop {{ebe6ae}}{itemName}{{FFFFFF}} x{amount}.");
                    ShowInventory(player);
                });
        }

        private static void ShowDetailDialog(Player player)
        {
            var itemName = player.GetData("ItemActionName", "");
            var def = ItemDatabase.Get(itemName);

            if (def == null) return;

            var totalAmount = GetItemCount(player, itemName);
            var weightPerItem = def.Weight / 1000f;
            var totalWeight = (def.Weight * totalAmount) / 1000f;

            var message = $"{{99b3c9}}Item Name: {itemName}\n" +
                         $"{{99b3c9}}Item Type: {def.ItemType}\n" +
                         $"{{99b3c9}}Item Weight: {weightPerItem:F2}kg\n" +
                         $"{{99b3c9}}Item Limit: {def.ItemStack}\n" +
                         $"{{99b3c9}}Item Quantity: {totalAmount} ({totalWeight:F2}kg)";

            player.ShowMessage("Inventory > Item Detail", message).Show();
        }

        private static string GetDurabilityText(Player player, string itemName, int index = -1)
        {
            var def = ItemDatabase.Get(itemName);
            if (def?.DurabilityDuration <= 0) return "-";

            ItemData item;
            if (index >= 0)
                item = player.InventoryData.Inventory[index];
            else
                item = player.InventoryData.Inventory.FirstOrDefault(i => i.ItemName == itemName);

            if (item == null || item.Durability <= 0) return "-";

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var startTime = item.Durability - def.DurabilityDuration;
            var remaining = item.Durability - now;

            if (remaining <= 0) return "{FF0000}0%";

            var percent = (remaining * 100) / def.DurabilityDuration;
            return $"{percent}%";
        }

        private static int CalculateRequiredSlots(Player player, string itemName, int amount)
        {
            var def = ItemDatabase.Get(itemName);
            if (def == null) return 0;

            var freeSpace = player.InventoryData.Inventory
                .Where(i => i.ItemName == itemName)
                .Sum(i => def.ItemStack - i.Amount);

            if (amount <= freeSpace) return 0;

            amount -= freeSpace;
            var slots = amount / def.ItemStack;
            if (amount % def.ItemStack > 0) slots++;

            return slots;
        }
    }
}