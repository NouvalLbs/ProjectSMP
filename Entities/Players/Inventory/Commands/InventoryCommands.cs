using ProjectSMP.Core;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP.Commands;

namespace ProjectSMP.Entities.Players.Inventory.Commands
{
    public class InventoryCommands
    {
        [Command("items", Shortcut = "i")]
        public static void Items(Player player)
        {
            if (!player.IsCharLoaded)
            {
                player.SendClientMessage(SampSharp.GameMode.SAMP.Color.White, $"{Msg.Error} Kamu belum login.");
                return;
            }

            InventoryService.ShowInventory(player);
        }

        [Command("pickup")]
        public static void Pickup(Player player)
        {
            if (!player.IsCharLoaded)
            {
                player.SendClientMessage(SampSharp.GameMode.SAMP.Color.White, $"{Msg.Error} Kamu belum login.");
                return;
            }

            var dropPoint = DropService.GetNearestDropPoint(player);
            if (dropPoint == null)
            {
                player.SendClientMessage(SampSharp.GameMode.SAMP.Color.White, $"{Msg.Inventory} Tidak ada loot drop di sekitar.");
                return;
            }

            DropService.ShowDropPointItems(player, dropPoint);
        }

        [Command("giveitem")]
        public static void GiveItem(Player player, string targetInput, string itemName, int amount)
        {
            if (player.Admin < 4)
            {
                player.SendClientMessage(SampSharp.GameMode.SAMP.Color.White, "{b9b9b9}Command tidak ada, gunakan '/help'.");
                return;
            }

            if (!player.AdminOnDuty)
            {
                player.SendClientMessage(SampSharp.GameMode.SAMP.Color.White, $"{Msg.AdmCmd} Command tidak dapat digunakan ketika tidak duty.");
                return;
            }

            var target = Utilities.GetPlayerFromPartOfName(player, targetInput);
            if (target == null || !target.IsCharLoaded) return;

            var def = ItemDatabase.Get(itemName);
            if (def == null)
            {
                player.SendClientMessage(SampSharp.GameMode.SAMP.Color.White, $"{Msg.AdmCmd} Item tidak valid.");
                return;
            }

            itemName = def.ItemName;

            if (amount <= 0)
            {
                player.SendClientMessage(SampSharp.GameMode.SAMP.Color.White, $"{Msg.AdmCmd} Amount harus lebih dari 0.");
                return;
            }

            if (!InventoryService.CanReceiveItem(target, itemName, amount))
            {
                player.SendClientMessage(SampSharp.GameMode.SAMP.Color.White, $"{Msg.AdmCmd} Inventory player penuh.");
                return;
            }

            if (itemName == "Money")
            {
                target.CharMoney += amount;
                if (target.CharMoney > int.MaxValue)
                    target.CharMoney = int.MaxValue;

                InventoryService.SyncMoneyToInventory(target);
            }
            else
            {
                long durability = 0;
                if (def.DurabilityDuration > 0)
                    durability = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() + def.DurabilityDuration;

                InventoryService.AddItem(target, itemName, amount, durability);
            }

            player.SendClientMessage(SampSharp.GameMode.SAMP.Color.White, $"{Msg.AdmCmd} Berhasil memberikan {{ebe6ae}}{itemName}{{FFFFFF}} x{amount} ke {{00FFFF}}{target.CharInfo.Username}{{FFFFFF}}.");
            target.SendClientMessage(SampSharp.GameMode.SAMP.Color.White, $"{Msg.AdmCmd} Admin {{ff0000}}{player.Ucp}{{FFFFFF}} memberikan {{ebe6ae}}{itemName}{{FFFFFF}} x{amount} ke kamu.");
        }
    }
}