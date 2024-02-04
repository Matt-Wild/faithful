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

            // Should hide this item due to temporary assets?
            bool forceHide = !toolbox.utils.debugMode && (_iconName == "textemporalcubeicon" || _modelName == "temporalcubemesh");

            // Should hide anyway due to config?
            if (!forceHide)
            {
                forceHide = !toolbox.config.CheckTag(_token);
            }

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

            // Set item tier (force untiered if forced to be hidden)
            itemDef.deprecatedTier = forceHide ? ItemTier.NoTier : _tier;

            // Banned from Simulacrum?
            if (_simulacrumBanned)
            {
                // Ban from Simulacrum
                toolbox.utils.BanFromSimulacrum(itemDef);
            }

            // Set can remove (Can a shrine of chance or printer etc. take this item)
            itemDef.canRemove = _canRemove;

            // Is item hidden (Also hide if using temporary assets when not in debug mode)
            itemDef.hidden = _hidden || forceHide;

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

            if (forceHide)
            {
                if (!toolbox.config.CheckTag(_token))
                {
                    Log.Debug($"Hiding item '{_token}' due to user preference");
                }
                else
                {
                    Log.Debug($"Hiding item '{_token}' due to use of temporary assets outside of debug mode");
                }
            }
        }
    }
}
