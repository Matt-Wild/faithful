using RoR2;
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
        }

        static void CreateAllItemsSettings()
        {
            // Create setting for disabling all items
            allItemsDisableSetting = Config.CreateSetting("ALL_ITEMS_DISABLED", "All Items", "Disable All Items", false, "Should all items from the Faithful mod be disabled?", false);
            allItemsDisableItemDisplaysSetting = Config.CreateSetting("ALL_ITEM_DISPLAYS_DISABLED", "All Items", "Disable All Item Displays", false, "Should all items have their item displays disabled on compatible character models?", false, true);
            allItemsExtendedPickupDescSetting = Config.CreateSetting("ALL_ITEMS_EXTENDED_PICKUP_DESC", "All Items", "All Items Extended Pickup Description", false, "Should all items have the logbook description appear when picking them up during runs?", false, true);
        }

        public static Item AddItem(string _token, ItemTag[] _tags, string _iconDir, string _modelDir, ItemTier _tier = ItemTier.Tier1, bool _simulacrumBanned = false, bool _canRemove = true, bool _hidden = false, string _corruptToken = null, ItemDisplaySettings _displaySettings = null, ModifyPrefabCallback _modifyItemModelPrefabCallback = null, ModifyPrefabCallback _modifyItemDisplayPrefabCallback = null, bool _debugOnly = false)
        {
            // Create item
            Item newItem = new Item(_token, _tags, _iconDir, _modelDir, _tier, _simulacrumBanned, _canRemove, _hidden, _corruptToken, _displaySettings, _modifyItemModelPrefabCallback, _modifyItemDisplayPrefabCallback, _debugOnly);

            // Add item to items list
            items.Add(newItem);

            // Return new item
            return newItem;
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
                // Return if all item pickup descriptions should be extended according to config
                return allItemsExtendedPickupDescSetting.Value;
            }
        }
    }
}
