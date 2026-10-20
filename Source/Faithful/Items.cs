using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;

namespace Faithful
{
    internal static class Items
    {
        // List of items
        static List<Item> items;

        // Override settings for all items
        static Setting<bool> allItemsDisableSetting;
        static Setting<bool> allItemsDisableItemDisplaysSetting;
        static Setting<bool> allItemsExtendedPickupDescSetting;

        public static void Init()
        {
            // Initialise items list
            items = new List<Item>();

            // Create item override settings
            CreateAllItemsSettings();

            // Hook behaviour
            Behaviour.AddCanSelectItemEntryCallback(OnCanSelectItemEntry);
        }

        static void CreateAllItemsSettings()
        {
            // Create setting for disabling all items
            allItemsDisableSetting = Config.CreateSetting("ALL_ITEMS_DISABLED", "All Items", "Disable All Items", false, "Should all items from the Faithful mod be disabled?", false, _restartRequired: true);
            allItemsDisableItemDisplaysSetting = Config.CreateSetting("ALL_ITEM_DISPLAYS_DISABLED", "All Items", "Disable All Item Displays", false, "Should all items have their item displays disabled on compatible character models?", false, true, _restartRequired: true);
            allItemsExtendedPickupDescSetting = Config.CreateSetting("ALL_ITEMS_EXTENDED_PICKUP_DESC", "All Items", "All Items Extended Pickup Description", false, "Should all items have the logbook description appear when picking them up during runs?\n\nPlease note:\nThis setting can sometimes be overwritten by mods such as LookingGlass.", false, true);
        }

        public static void FetchSettings()
        {
            // Cycle through items
            foreach (Item item in items)
            {
                // Fetch settings for item
                item.FetchSettings();
            }
        }

        public static Item AddItem(ItemOptions _options)
        {
            // Create item
            Item newItem = new(_options);

            // Add item to items list
            items.Add(newItem);

            // Return new item
            return newItem;
        }

        static bool OnCanSelectItemEntry(ItemDef itemDef, Dictionary<ExpansionDef, bool> expansionAvailability)
        {
            // Check if item is hidden from logbook
            foreach (Item item in items) if (item.itemDef == itemDef) if (item.hiddenFromLogbook) return false;

            // Leave normal selection rules in place for other entries
            return true;
        }

        public static Item GetItem(string _token)
        {
            // Cycle through items
            foreach (Item item in items)
            {
                // Check if correct token
                if (item.token == _token)
                {
                    // Return item
                    return item;
                }
            }

            // Return null if not found
            Log.Error($"Attempted to fetch item '{_token}' but couldn't find it");
            return null;
        }

        public static List<Item> AllItems => items;

        public static bool allItemsDisabled
        {
            get
            {
                // Return if all items should be disabled according to config
                return allItemsDisableSetting.Value;
            }
        }

        public static bool allItemDisplaysDisabled
        {
            get
            {
                // Return if all item displays should be disabled according to config
                return allItemsDisableItemDisplaysSetting.Value;
            }
        }

        public static bool extendAllPickupDescriptions
        {
            get
            {
                // Return if all item pickup descriptions should be extended according to config (or LookingGlass)
                return allItemsExtendedPickupDescSetting.Value;
            }
        }
    }
}
