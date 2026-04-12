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

            InventoryService.SyncMoneyToInventory(player);
            InventoryService.RemoveExpiredItems(player);
            SendInventoryData(player);
            SendDropPointData(player);
        }

        public static void HandleClose(Player player) { }

        public static void HandleMove(Player player, int fromSlot, int toSlot, int amount)
        {
            if (player.InventoryData == null) return;
            var inv = player.InventoryData.Inventory;
            if (fromSlot < 0 || fromSlot >= player.InventoryData.Slots) return;
            if (toSlot < 0 || toSlot >= player.InventoryData.Slots || fromSlot == toSlot) return;

            var fromItem = inv.FirstOrDefault(i => i.Slot == fromSlot);
            if (fromItem == null) return;

            var toItem = inv.FirstOrDefault(i => i.Slot == toSlot);
            var def = ItemDatabase.Get(fromItem.ItemName);
            var moveAmt = (amount <= 0 || amount >= fromItem.Amount) ? fromItem.Amount : amount;

            if (toItem == null)
            {
                if (moveAmt == fromItem.Amount)
                {
                    fromItem.Slot = toSlot;
                }
                else
                {
                    fromItem.Amount -= moveAmt;
                    inv.Add(new ItemData
                    {
                        ItemName = fromItem.ItemName,
                        Amount = moveAmt,
                        Durability = fromItem.Durability,
                        Slot = toSlot
                    });
                }
            }
            else
            {
                var isSame = toItem.ItemName == fromItem.ItemName && toItem.Durability == fromItem.Durability;
                if (isSame && def != null)
                {
                    var space = def.ItemStack - toItem.Amount;
                    var transfer = Math.Min(moveAmt, space);
                    toItem.Amount += transfer;
                    fromItem.Amount -= transfer;
                    if (fromItem.Amount <= 0) inv.Remove(fromItem);
                }
                else if (moveAmt >= fromItem.Amount)
                {
                    fromItem.Slot = toSlot;
                    toItem.Slot = fromSlot;
                }
            }

            SendInventoryData(player);
        }

        public static void HandleAction(Player player, string action, int slot, int amount)
        {
            if (player.InventoryData == null) return;
            var inv = player.InventoryData.Inventory;

            var item = inv.FirstOrDefault(i => i.Slot == slot);
            if (item == null) return;

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
                            itemSlot: slot,
                            itemName: item.ItemName
                        );
                    }
                    else
                    {
                        InventoryService.OnItemUseComplete(player, item.ItemName, slot);
                        SendInventoryData(player);
                    }
                    break;

                case "drop":
                    if (!def.CanDrop) return;
                    var dropAmt = (amount <= 0 || amount > item.Amount) ? item.Amount : amount;
                    var dur = item.Durability;
                    var dropName = item.ItemName;
                    InventoryService.RemoveItemBySlot(player, slot, dropAmt);
                    DropService.AddItemToDropPoint(player, dropName, dropAmt, dur);
                    player.SendClientMessage(Color.White, $"{Msg.Inventory} Kamu drop {{ebe6ae}}{dropName}{{FFFFFF}} x{dropAmt}.");
                    EmitToast(player, $"Dropped {dropAmt}x {dropName}", "info");
                    SendInventoryData(player);
                    break;
            }
        }

        public static void HandleGiveRequest(Player player, int slot, int amount)
        {
            if (player.InventoryData == null) return;
            var inv = player.InventoryData.Inventory;

            var item = inv.FirstOrDefault(i => i.Slot == slot);
            if (item == null) return;

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

            var giveAmt = (amount <= 0 || amount > item.Amount) ? item.Amount : amount;

            CefService.EmitEvent(player.Id, "inv:give_players", new
            {
                players = nearby,
                index = slot,
                amount = giveAmt
            });
        }

        public static void HandleGiveConfirm(Player player, int slot, int amount, int targetId)
        {
            if (player.InventoryData == null) return;
            var inv = player.InventoryData.Inventory;

            var item = inv.FirstOrDefault(i => i.Slot == slot);
            if (item == null) return;

            var target = BasePlayer.Find(targetId) as Player;
            if (target == null || !target.IsConnected || !target.IsCharLoaded)
            {
                EmitToast(player, "Player tidak ditemukan.", "warning");
                return;
            }

            var giveAmt = (amount <= 0 || amount > item.Amount) ? item.Amount : amount;

            if (!InventoryService.CanReceiveItem(target, item.ItemName, giveAmt))
            {
                EmitToast(player, $"Inventory {target.CharInfo.Username} penuh.", "warning");
                return;
            }

            var itemName = item.ItemName;
            var durability = item.Durability;
            InventoryService.RemoveItemBySlot(player, slot, giveAmt);
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
            var slots = new object?[slotCount];

            foreach (var item in player.InventoryData.Inventory)
            {
                if (item.Slot < 0 || item.Slot >= slotCount) continue;
                var def = ItemDatabase.Get(item.ItemName);
                slots[item.Slot] = new
                {
                    index = item.Slot,
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

        public static void SendDropPointData(Player player)
        {
            var data = DropService.GetDropPointCefData(player);
            if (data != null)
                CefService.EmitEvent(player.Id, "inv:drop_data", data);
            else
                CefService.EmitEvent(player.Id, "inv:drop_clear", new { });
        }

        public static void HandleDropTake(Player player, int dropSlot, int amount)
        {
            var dp = DropService.GetNearestDropPoint(player);
            if (dp == null) { EmitToast(player, "Drop point tidak ditemukan.", "warning"); return; }
            if (dropSlot < 0 || dropSlot >= dp.Items.Count) return;

            var item = dp.Items[dropSlot];
            var takeAmt = (amount <= 0 || amount > item.Amount) ? item.Amount : amount;

            if (!InventoryService.CanReceiveItem(player, item.ItemName, takeAmt))
            {
                EmitToast(player, "Inventory penuh.", "warning");
                return;
            }

            InventoryService.AddItem(player, item.ItemName, takeAmt, item.Durability);
            item.Amount -= takeAmt;
            if (item.Amount <= 0) dp.Items.RemoveAt(dropSlot);

            player.SendClientMessage(Color.White, $"{Msg.Inventory} Kamu mengambil {{ebe6ae}}{item.ItemName}{{FFFFFF}} x{takeAmt}.");
            EmitToast(player, $"Took {takeAmt}x {item.ItemName}", "success");
            SendInventoryData(player);
            SendDropPointData(player);
        }

        public static void HandleDropToInv(Player player, int dropSlot, int toSlot, int amount)
        {
            var dp = DropService.GetNearestDropPoint(player);
            if (dp == null) { EmitToast(player, "Drop point tidak ditemukan.", "warning"); return; }
            if (dropSlot < 0 || dropSlot >= dp.Items.Count) return;

            var item = dp.Items[dropSlot];
            var takeAmt = (amount <= 0 || amount > item.Amount) ? item.Amount : amount;

            if (!InventoryService.CanReceiveItem(player, item.ItemName, takeAmt))
            {
                EmitToast(player, "Inventory penuh.", "warning");
                return;
            }

            var inv = player.InventoryData.Inventory;
            var targetItem = toSlot >= 0 ? inv.FirstOrDefault(i => i.Slot == toSlot) : null;

            if (toSlot >= 0 && targetItem == null)
            {
                inv.Add(new ItemData { ItemName = item.ItemName, Amount = takeAmt, Durability = item.Durability, Slot = toSlot });
            }
            else
            {
                InventoryService.AddItem(player, item.ItemName, takeAmt, item.Durability);
            }

            item.Amount -= takeAmt;
            if (item.Amount <= 0) dp.Items.RemoveAt(dropSlot);
            DropService.CheckAndCleanup(dp);

            player.SendClientMessage(Color.White, $"{Msg.Inventory} Kamu mengambil {{ebe6ae}}{item.ItemName}{{FFFFFF}} x{takeAmt}.");
            EmitToast(player, $"Took {takeAmt}x {item.ItemName}", "success");
            SendInventoryData(player);
            SendDropPointData(player);
        }

        public static void HandleInvToDrop(Player player, int fromSlot, int amount)
        {
            if (player.InventoryData == null) return;
            var item = player.InventoryData.Inventory.FirstOrDefault(i => i.Slot == fromSlot);
            if (item == null) return;

            var def = ItemDatabase.Get(item.ItemName);
            if (def == null || !def.CanDrop) { EmitToast(player, "Item tidak bisa di-drop.", "warning"); return; }

            var dropAmt = (amount <= 0 || amount > item.Amount) ? item.Amount : amount;
            var dur = item.Durability;
            var name = item.ItemName;
            InventoryService.RemoveItemBySlot(player, fromSlot, dropAmt);
            DropService.AddItemToDropPoint(player, name, dropAmt, dur);
            player.SendClientMessage(Color.White, $"{Msg.Inventory} Kamu drop {{ebe6ae}}{name}{{FFFFFF}} x{dropAmt}.");
            EmitToast(player, $"Dropped {dropAmt}x {name}", "info");
            SendInventoryData(player);
            SendDropPointData(player);
        }
    }
}