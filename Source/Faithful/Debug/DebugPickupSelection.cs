using RoR2;
using System;
using System.Collections.Generic;
using TMPro;

namespace Faithful
{
    internal static class DebugPickupSelection
    {
        public static TMP_Dropdown Create(TMP_Dropdown _dropdown, Dictionary<string, PickupDef> _pickupLookup, Func<PickupDef, bool> _filter = null)
        {
            // Share pickup labels and the reusable dropdown between debug tools
            List<string> pickupNames = new List<string>();
            _pickupLookup.Clear();
            foreach (PickupDef pickup in PickupCatalog.allPickups)
            {
                if (_filter != null && !_filter(pickup)) continue;
                string name = $"{Utils.GetPickupDisplayName(pickup)} [{pickup.pickupIndex.value}]";
                pickupNames.Add(name);
                _pickupLookup[name] = pickup;
            }

            // Keep long pickup lists alphabetical and create only the visible option rows
            pickupNames.Sort();
            TMP_Dropdown dropdown = DebugSelectionDropdown.Create(_dropdown);
            dropdown.ClearOptions();
            dropdown.AddOptions(pickupNames);
            return dropdown;
        }

    }
}
