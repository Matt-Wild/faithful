using RoR2;
using R2API;
using UnityEngine;
using RoR2.ExpansionManagement;
using IL.RoR2.Items;
using HarmonyLib;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Faithful
{
    internal class Item
    {
        // Toolbox
        protected Toolbox toolbox;

        // Item def
        public ItemDef itemDef;

        // Item token
        public string token;

        // Constructor
        public Item(Toolbox _toolbox, string _token, ItemTag[] _tags, string _iconName, string _modelName, ItemTier _tier = ItemTier.Tier1, bool _simulacrumBanned = false, bool _canRemove = true, bool _hidden = false, string _corruptToken = null, ItemDisplaySettings _displaySettings = null)
        {
            toolbox = _toolbox;

            // Assign token
            token = _token;

            // Create item def
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            // Set item texts
            itemDef.name = $"FAITHFUL_{_token}_NAME";
            itemDef.nameToken = $"FAITHFUL_{_token}_NAME";
            itemDef.pickupToken = $"FAITHFUL_{_token}_PICKUP";
            itemDef.descriptionToken = $"FAITHFUL_{_token}_DESC";
            itemDef.loreToken = $"FAITHFUL_{_token}_LORE";

            // Set item tags
            itemDef.tags = _tags;

            // Set item tier
            itemDef.deprecatedTier = _tier;

            // Banned from Simulacrum?
            if (_simulacrumBanned)
            {
                // Ban from Simulacrum
                toolbox.utils.BanFromSimulacrum(itemDef);
            }

            // Set can remove (Can a shrine of chance or printer etc. take this item)
            itemDef.canRemove = _canRemove;

            // Is item hidden
            itemDef.hidden = _hidden;

            // Corrupts item?
            if (_corruptToken != null)
            {
                // Add corruption pair
                toolbox.utils.AddCorruptionPair(itemDef, _corruptToken);
            }

            // Set icon and model
            itemDef.pickupIconSprite = toolbox.assets.GetIcon(_iconName);
            itemDef.pickupModelPrefab = toolbox.assets.GetModel(_modelName);

            // Check for item display settings
            if (_displaySettings != null)
            {
                // Add item and pass in item display settings
                ItemAPI.Add(new CustomItem(itemDef, _displaySettings.GetRules()));
            }
            else
            {
                // Add item and pass in null Item Display Rules
                ItemAPI.Add(new CustomItem(itemDef, new ItemDisplayRuleDict(null)));
            }

            Log.Debug($"Created item '{_token}'");
        }
    }
}
