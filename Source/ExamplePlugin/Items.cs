using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace Faithful
{
    internal class Items
    {
        // Toolbox
        protected Toolbox toolbox;

        // List of items
        List<Item> items;

        // Constructor
        public Items(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Initialise items list
            items = new List<Item>();
        }

        public Item AddItem(string _token, ItemTag[] _tags, string _iconDir, string _modelDir, ItemTier _tier = ItemTier.Tier1, bool _simulacrumBanned = false, bool _canRemove = true, bool _hidden = false, string _corruptToken = null)
        {
            // Create item
            Item newItem = new Item(toolbox, _token, _tags, _iconDir, _modelDir, _tier, _simulacrumBanned, _canRemove, _hidden, _corruptToken);

            // Add item to items list
            items.Add(newItem);

            // Return new item
            return newItem;
        }

        public Item GetItem(string _token)
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
            return null;
        }
    }
}
