using RoR2;
using System;
using System.Collections.Generic;

namespace Faithful
{
    internal sealed class ContentPackItemResolver
    {
        private readonly Dictionary<string, ItemDef> lookup = new(StringComparer.OrdinalIgnoreCase);

        public ContentPackItemResolver(IEnumerable<ItemDef> _availableItems, IDictionary<string, ItemBase> _faithfulItems)
        {
            // Add all content-pack visible items first
            foreach (ItemDef itemDef in _availableItems)
            {
                RegisterAliases(itemDef);
            }

            // Add Faithful's own registered items as extra aliases
            foreach (KeyValuePair<string, ItemBase> kvp in _faithfulItems)
            {
                ItemBase itemBase = kvp.Value;
                if (itemBase?.mainItem.itemDef == null) continue;

                RegisterAliases(itemBase.mainItem.itemDef);

                // Add plenty of aliases for Faithful items
                AddAlias(kvp.Key, itemBase.mainItem.itemDef);
                AddAlias($"ITEM_{kvp.Key}", itemBase.mainItem.itemDef);
                AddAlias($"FAITHFUL_{kvp.Key}_NAME", itemBase.mainItem.itemDef);
                AddAlias($"FAITHFUL_ITEM_{kvp.Key}_NAME", itemBase.mainItem.itemDef);
                AddAlias(itemBase.mainItem.safeName, itemBase.mainItem.itemDef);
            }
        }

        public ItemDef Resolve(string _value)
        {
            if (string.IsNullOrWhiteSpace(_value)) return null;

            _value = _value.Trim();

            if (lookup.TryGetValue(_value, out ItemDef itemDef)) return itemDef;

            if (Assets.TryGetOfficialItem(_value, out ItemDef officialItem)) return officialItem;

            return null;
        }

        private void RegisterAliases(ItemDef _itemDef)
        {
            if (_itemDef == null) return;

            AddAlias(_itemDef.name, _itemDef);
            AddAlias(_itemDef.nameToken, _itemDef);

            if (!string.IsNullOrWhiteSpace(_itemDef.nameToken))
            {
                string displayName = Language.GetString(_itemDef.nameToken);

                if (!string.IsNullOrWhiteSpace(displayName) && !string.Equals(displayName, _itemDef.nameToken, StringComparison.Ordinal))
                {
                    AddAlias(displayName, _itemDef);
                }
            }
        }

        private void AddAlias(string _alias, ItemDef _itemDef)
        {
            if (string.IsNullOrWhiteSpace(_alias) || _itemDef == null) return;

            _alias = _alias.Trim();

            if (!lookup.ContainsKey(_alias)) lookup.Add(_alias, _itemDef);
        }
    }
}
