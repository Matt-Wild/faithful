using RoR2;
using System.Collections.Generic;

namespace Faithful
{
    internal static class Items
    {
        // List of items
        static List<Item> items;

        public static void Init()
        {
            // Initialise items list
            items = new List<Item>();
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
    }
}
