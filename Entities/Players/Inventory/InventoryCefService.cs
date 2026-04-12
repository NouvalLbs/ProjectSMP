#nullable enable
using ProjectSMP.Core;
using ProjectSMP.Entities.Players.Inventory.Data;
using ProjectSMP.Plugins.CEF;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System;
using System.Linq;

namespace ProjectSMP.Entities.Players.Inventory
{
    public static class InventoryCefService
    {
        public static void HandleOpen(Player player)
        {
            if (!player.IsCharLoaded || player.InventoryData == null) return;
            InventoryService.Organize(player);
            InventoryService.SyncMoneyToInventory(player);
            InventoryService.RemoveExpiredItems(player);
            SendInventoryData(player);
        }

        public static void HandleClose(Player player) { }

        public static void HandleMove(Player player, int fromIndex, int toIndex, int amount)
        {
            if (player.InventoryData == null) return;
            var inv = player.InventoryData.Inventory;
            if (fromIndex < 0 || fromIndex >= inv.Count) return;
            if (toIndex < 0 || toIndex >= player.InventoryData.Slots || fromIndex == toIndex) return;

            var fromItem = inv[fromIndex];

            if (toIndex >= inv.Count)
            {
                var moveAmt = (amount <= 0 || amount >= fromItem.Amount) ? fromItem.Amount : amount;
                if (moveAmt == fromItem.Amount)
                {
                    inv.RemoveAt(fromIndex);
                    inv.Add(fromItem);
                }
                else
                {
                    fromItem.Amount -= moveAmt;
                    inv.Add(new ItemData { ItemName = fromItem.ItemName, Amount = moveAmt, Durability = fromItem.Durability });
                }
            }
            else
            {
                var toItem = inv[toIndex];
                var def = ItemDatabase.Get(fromItem.ItemName);
                var isSame = toItem.ItemName == fromItem.ItemName && toItem.Durability == fromItem.Durability;
                var moveAmt = (amount <= 0 || amount >= fromItem.Amount) ? fromItem.Amount : amount;

                if (isSame && def != null)
                {
                    var space = def.ItemStack - toItem.Amount;
                    var transfer = Math.Min(moveAmt, space);
                    toItem.Amount += transfer;
                    fromItem.Amount -= transfer;
                    if (fromItem.Amount <= 0) inv.RemoveAt(fromIndex);
                }
                else if (moveAmt >= fromItem.Amount)
                {
                    inv[fromIndex] = toItem;
                    inv[toIndex] = fromItem;
                }
            }

            SendInventoryData(player);
        }

        public static void HandleAction(Player player, string action, int index, int amount)
        {
            if (player.InventoryData == null) return;
            var inv = player.InventoryData.Inventory;
            if (index < 0 || index >= inv.Count) return;

            var item = inv[index];
            var def = ItemDatabase.Get(item.ItemName);
            if (def == null) return;

            switch (action)
            {
                case "use":
                    if (!def.CanUsed) return;
                    if (item.Durability > 0 && item.Durability <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                    {
                        EmitToast(player, "Item sudah kadaluarsa!", "warning");
                        return;
                    }
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
                            itemName: item.ItemName
                        );
                    }
                    else
                    {
                        InventoryService.OnItemUseComplete(player, item.ItemName, index);
                        SendInventoryData(player);
                    }
                    break;

                case "drop":
                    if (!def.CanDrop) return;
                    var dropAmt = (amount <= 0 || amount > item.Amount) ? item.Amount : amount;
                    var dur = item.Durability;
                    var dropName = item.ItemName;
                    InventoryService.RemoveItemByIndex(player, index, dropAmt);
                    DropService.AddItemToDropPoint(player, dropName, dropAmt, dur);
                    player.SendClientMessage(Color.White, $"{Msg.Inventory} Kamu drop {{ebe6ae}}{dropName}{{FFFFFF}} x{dropAmt}.");
                    EmitToast(player, $"Dropped {dropAmt}x {dropName}", "info");
                    SendInventoryData(player);
                    break;
            }
        }

        public static void HandleGiveRequest(Player player, int index, int amount)
        {
            if (player.InventoryData == null) return;
            var inv = player.InventoryData.Inventory;
            if (index < 0 || index >= inv.Count) return;

            var nearby = BasePlayer.All
                .OfType<Player>()
                .Where(p => p.IsConnected && p.Id != player.Id && p.IsCharLoaded &&
                            p.Position.DistanceTo(player.Position) <= 8.0f)
                .Select(p => new { id = p.Id, name = p.CharInfo.Username })
                .ToList();

            if (!nearby.Any())
            {
                EmitToast(player, "Tidak ada player di sekitar.", "warning");
                return;
            }

            var item = inv[index];
            var giveAmt = (amount <= 0 || amount > item.Amount) ? item.Amount : amount;

            CefService.EmitEvent(player.Id, "inv:give_players", new
            {
                players = nearby,
                index,
                amount = giveAmt
            });
        }

        public static void HandleGiveConfirm(Player player, int index, int amount, int targetId)
        {
            if (player.InventoryData == null) return;
            var inv = player.InventoryData.Inventory;
            if (index < 0 || index >= inv.Count) return;

            var target = BasePlayer.Find(targetId) as Player;
            if (target == null || !target.IsConnected || !target.IsCharLoaded)
            {
                EmitToast(player, "Player tidak ditemukan.", "warning");
                return;
            }

            var item = inv[index];
            var giveAmt = (amount <= 0 || amount > item.Amount) ? item.Amount : amount;

            if (!InventoryService.CanReceiveItem(target, item.ItemName, giveAmt))
            {
                EmitToast(player, $"Inventory {target.CharInfo.Username} penuh.", "warning");
                return;
            }

            var itemName = item.ItemName;
            var durability = item.Durability;
            InventoryService.RemoveItemByIndex(player, index, giveAmt);
            InventoryService.AddItem(target, itemName, giveAmt, durability);

            player.SendClientMessage(Color.White, $"{Msg.Inventory} Kamu memberikan {{ebe6ae}}{itemName}{{FFFFFF}} x{giveAmt} ke {{ebe6ae}}{target.CharInfo.Username}{{FFFFFF}}.");
            target.SendClientMessage(Color.White, $"{Msg.Inventory} {{ebe6ae}}{player.CharInfo.Username}{{FFFFFF}} memberikan {{ebe6ae}}{itemName}{{FFFFFF}} x{giveAmt} ke kamu.");

            EmitToast(player, $"Gave {giveAmt}x {itemName} to {target.CharInfo.Username}", "success");
            SendInventoryData(player);
        }

        public static void SendInventoryData(Player player)
        {
            if (player.InventoryData == null) return;

            var slotCount = player.InventoryData.Slots;
            var inv = player.InventoryData.Inventory;
            var slots = new object?[slotCount];

            for (int i = 0; i < inv.Count && i < slotCount; i++)
            {
                var item = inv[i];
                var def = ItemDatabase.Get(item.ItemName);
                slots[i] = new
                {
                    index = i,
                    name = item.ItemName,
                    count = item.Amount,
                    weight = def != null ? (float)Math.Round(def.Weight / 1000f, 3) : 0f,
                    durability = GetDurabilityPercent(item, def),
                    type = def?.ItemType ?? "Unknown"
                };
            }

            CefService.EmitEvent(player.Id, "inv:data", new
            {
                playerName = player.CharInfo.Username,
                currentWeight = (float)Math.Round(InventoryService.GetTotalWeight(player) / 1000f, 3),
                maxWeight = player.InventoryData.MaxWeight / 1000f,
                slotCount,
                items = slots
            });
        }

        private static int GetDurabilityPercent(ItemData item, ItemDefinition? def)
        {
            if (def == null || def.DurabilityDuration <= 0 || item.Durability <= 0) return -1;
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var remaining = item.Durability - now;
            if (remaining <= 0) return 0;
            return (int)((remaining * 100) / def.DurabilityDuration);
        }

        private static void EmitToast(Player player, string message, string type)
            => CefService.EmitEvent(player.Id, "inv:toast", new { message, type });
    }
}