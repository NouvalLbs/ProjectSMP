using ProjectSMP.Entities.Players.Inventory.Data;
using System.Collections.Generic;
using System.Linq;

namespace ProjectSMP.Entities.Players.Inventory
{
    public static class ItemDatabase
    {
        private static readonly Dictionary<string, ItemDefinition> Items = new(System.StringComparer.OrdinalIgnoreCase);

        static ItemDatabase()
        {
            RegisterItem(new ItemDefinition
            {
                ItemName = "Money",
                ItemStack = int.MaxValue,
                Weight = 0,
                CanUsed = false,
                CanGive = true,
                CanDrop = false,
                CanDetail = true,
                ItemType = "Currency"
            });

            RegisterItem(new ItemDefinition
            {
                ItemName = "Snack",
                ItemStack = 5,
                Weight = 40,
                CanUsed = true,
                CanGive = true,
                CanDrop = true,
                CanDetail = true,
                ProgDur = 3,
                ProgText = "MENGKONSUMSI_SNACK",
                ProgAnimIndex = 536,
                ProgAnimLib = "FOOD",
                ProgAnimName = "EAT_Burger",
                DurabilityDuration = 3600,
                ItemType = "Consumable"
            });

            RegisterItem(new ItemDefinition
            {
                ItemName = "Sprunk",
                ItemStack = 5,
                Weight = 330,
                CanUsed = true,
                CanGive = true,
                CanDrop = true,
                CanDetail = true,
                ProgDur = 3,
                ProgText = "MEMINUM_SPRUNK",
                ProgAnimIndex = 1656,
                ProgAnimLib = "VENDING",
                ProgAnimName = "VEND_Drink2_P",
                DurabilityDuration = 7200,
                ItemType = "Consumable"
            });
        }

        private static void RegisterItem(ItemDefinition item)
        {
            Items[item.ItemName] = item;
        }

        public static ItemDefinition Get(string itemName)
        {
            return Items.TryGetValue(itemName, out var item) ? item : null;
        }

        public static bool Exists(string itemName) => Items.ContainsKey(itemName);

        public static IEnumerable<ItemDefinition> GetAll() => Items.Values;

        public static IEnumerable<ItemDefinition> GetByType(string type)
        {
            return Items.Values.Where(i => i.ItemType == type);
        }
    }
}