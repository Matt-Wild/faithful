using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ItemQualities;
using ItemQualities.ContentManagement;

namespace Faithful
{
    internal static class QualityCompat
    {
        // Contains base items and their quality groups
        static Dictionary<Item, ItemQualityGroup> itemQualityGroups = [];

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static void Init()
        {
            Log.Info("Faithful detected Quality, initializing compatibility!");

            // Hook into load content event
            QualityContentManager.LoadContentAsync += LoadQualityContent;
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
    }
}