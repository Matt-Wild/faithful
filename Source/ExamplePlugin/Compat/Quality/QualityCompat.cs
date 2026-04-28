using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ItemQualities;
using ItemQualities.ContentManagement;
using ItemQualities.Utilities.Extensions;
using RoR2;

namespace Faithful
{
    internal static class QualityCompat
    {
        // Contains base items and their quality groups
        static Dictionary<Item, ItemQualityGroup> itemQualityGroups = [];

        // Cached item counts for more efficient lookups
        static Dictionary<Inventory, Dictionary<Item, QualityCounts>> cachedItemCountsEffective = [];
        static Dictionary<Inventory, Dictionary<Item, QualityCounts>> cachedItemCountsPermanent = [];

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static void Init()
        {
            Log.Info("Faithful detected Quality, initializing compatibility!");

            // Hook into load content event
            QualityContentManager.LoadContentAsync += LoadQualityContent;

            // Hook into inventory changed event to clear cached counts
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;

            // Hook into on destroy inventory event to clear cached counts
            On.RoR2.Inventory.OnDestroy += Inventory_OnDestroy;

            // Do a manual full inventory cache wipe when leaving a stage
            Behaviour.AddOnPreSceneExitCallback((_exitController) =>
            {
                cachedItemCountsEffective.Clear();
                cachedItemCountsPermanent.Clear();
            });
        }

        private static void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            // Call original behaviour
            orig(self);

            // Check for inventory
            if (self.inventory == null) return;

            // Clear cached counts for inventory
            cachedItemCountsEffective.Remove(self.inventory);
            cachedItemCountsPermanent.Remove(self.inventory);
        }

        private static void Inventory_OnDestroy(On.RoR2.Inventory.orig_OnDestroy orig, Inventory self)
        {
            // Call original behaviour
            orig(self);

            // Check for inventory
            if (self == null) return;

            // Clear cached counts for inventory
            cachedItemCountsEffective.Remove(self);
            cachedItemCountsPermanent.Remove(self);
        }

        private static IEnumerator LoadQualityContent(QualityContentLoadArgs _args)
        {
            // Cycle through items and create item quality groups
            foreach (Item item in Items.AllItems)
            {
                // Ignore if item doesn't support quality
                if (!item.supportsQuality) continue;

                // Create item quality group for item and add to dictionary
                itemQualityGroups.Add(item, _args.CreateItemQualityGroup(item.itemDef));
            }

            yield break;
        }

        public static QualityCounts GetItemCountsEffective(Inventory _inventory, Item _item)
        {
            // Try get item group
            if (!itemQualityGroups.TryGetValue(_item, out ItemQualityGroup itemGroup))
            {
                Log.Warning($"[QUALITY COMPAT] - Couldn't find quality group for item '{_item.name}', returning default counts!");
                return new QualityCounts();
            }

            // Check for inventory cache
            if (cachedItemCountsEffective.TryGetValue(_inventory, out Dictionary<Item, QualityCounts> inventoryCache))
            {
                // Check for cached quality counts
                if (inventoryCache.TryGetValue(_item, out QualityCounts cachedCounts))
                {
                    // Return cached counts
                    return cachedCounts;
                }
            }
            else
            {
                // Create new cache for inventory
                cachedItemCountsEffective.Add(_inventory, []);
            }

            // Get inaccessible counts
            ItemQualityCounts baseCounts = _inventory.GetItemCountsEffective(itemGroup);

            // Get counts in accessible format
            QualityCounts counts = new()
            {
                UNCOMMON = baseCounts.UncommonCount,
                RARE = baseCounts.RareCount,
                EPIC = baseCounts.EpicCount,
                LEGENDARY = baseCounts.LegendaryCount
            };

            // Add counts to cache
            cachedItemCountsEffective[_inventory].Add(_item, counts);

            // Return counts
            return counts;
        }

        public static QualityCounts GetItemCountsPermanent(Inventory _inventory, Item _item)
        {
            // Try get item group
            if (!itemQualityGroups.TryGetValue(_item, out ItemQualityGroup itemGroup))
            {
                Log.Warning($"[QUALITY COMPAT] - Couldn't find quality group for item '{_item.name}', returning default counts!");
                return new QualityCounts();
            }

            // Check for inventory cache
            if (cachedItemCountsPermanent.TryGetValue(_inventory, out Dictionary<Item, QualityCounts> inventoryCache))
            {
                // Check for cached quality counts
                if (inventoryCache.TryGetValue(_item, out QualityCounts cachedCounts))
                {
                    // Return cached counts
                    return cachedCounts;
                }
            }
            else
            {
                // Create new cache for inventory
                cachedItemCountsPermanent.Add(_inventory, []);
            }

            // Get inaccessible counts
            ItemQualityCounts baseCounts = _inventory.GetItemCountsPermanent(itemGroup);

            // Get counts in accessible format
            QualityCounts counts = new()
            {
                UNCOMMON = baseCounts.UncommonCount,
                RARE = baseCounts.RareCount,
                EPIC = baseCounts.EpicCount,
                LEGENDARY = baseCounts.LegendaryCount
            };

            // Add counts to cache
            cachedItemCountsPermanent[_inventory].Add(_item, counts);

            // Return counts
            return counts;
        }
    }
}