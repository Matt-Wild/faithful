using RoR2;
using R2API;
using UnityEngine;

namespace Faithful
{
    internal delegate void ModifyPrefabCallback(GameObject _prefab);

    internal class Item
    {
        // Create default settings that all items have
        public Setting<bool> enabled;
        public Setting<bool> extendedPickupDesc;
        public Setting<bool> enableItemDisplays;

        // Item def
        public ItemDef itemDef;

        // Item token and name
        public string token;
        public string name;

        // Constructor
        public Item(string _token, ItemTag[] _tags, string _iconName, string _modelName, ItemTier _tier = ItemTier.Tier1, bool _simulacrumBanned = false, bool _canRemove = true, bool _hidden = false, string _corruptToken = null, ItemDisplaySettings _displaySettings = null, ModifyPrefabCallback _modifyItemModelPrefabCallback = null, ModifyPrefabCallback _modifyItemDisplayPrefabCallback = null, bool _debugOnly = false)
        {
            // Assign token
            token = _token;

            // Assign name
            name = Utils.GetLanguageString($"FAITHFUL_{_token}_NAME");

            // Create default settings (MUST HAPPEN AFTER TOKEN AND NAME IS ASSIGNED)
            CreateDefaultSettings();

            // Should hide this item due to temporary assets or debug only?
            bool forceHide = !Utils.debugMode && (_debugOnly || _iconName == "textemporalcubeicon" || !Assets.HasAsset(_iconName) || _modelName == "temporalcubemesh" || !Assets.HasAsset(_modelName));

            // Should hide anyway due to config?
            if (!forceHide)
            {
                forceHide = !enabled.Value;
            }

            // Create item def
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            // Set item texts
            itemDef.name = $"FAITHFUL_{_token}_NAME";
            itemDef.nameToken = $"FAITHFUL_{_token}_NAME";
            itemDef.pickupToken = extendedPickupDesc.Value ? $"FAITHFUL_{_token}_DESC" : $"FAITHFUL_{_token}_PICKUP";
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
                Utils.BanFromSimulacrum(itemDef);
            }

            // Set can remove (Can a shrine of chance or printer etc. take this item)
            itemDef.canRemove = _canRemove;

            // Is item hidden (Also hide if using temporary assets when not in debug mode)
            itemDef.hidden = _hidden || forceHide;

            // Corrupts item?
            if (_corruptToken != null)
            {
                // Add corruption pair
                Utils.AddCorruptionPair(itemDef, _corruptToken);
            }

            // Set icon and model
            itemDef.pickupIconSprite = Assets.GetIcon(_iconName);
            itemDef.pickupModelPrefab = Assets.GetModel(_modelName);

            // Modify pickup model prefab
            ModelPanelParameters mdlPanelParams = itemDef.pickupModelPrefab.AddComponent<ModelPanelParameters>();

            // Check for focus point
            GameObject focusPoint = Utils.FindChildByName(itemDef.pickupModelPrefab.transform, "FocusPoint");
            if (focusPoint != null)
            {
                // Set focus point transform
                mdlPanelParams.focusPointTransform = focusPoint.transform;
            }

            else
            {
                // No focus point found so create one
                mdlPanelParams.focusPointTransform = new GameObject("FocusPoint").transform;
                mdlPanelParams.focusPointTransform.SetParent(itemDef.pickupModelPrefab.transform);
            }

            // Check for camera position
            GameObject cameraPos = Utils.FindChildByName(itemDef.pickupModelPrefab.transform, "CameraPosition");
            if (cameraPos != null)
            {
                // Set camera position transform
                mdlPanelParams.cameraPositionTransform = cameraPos.transform;
            }

            else
            {
                // No camera position found so create one
                mdlPanelParams.cameraPositionTransform = new GameObject("CameraPosition").transform;
                mdlPanelParams.cameraPositionTransform.SetParent(itemDef.pickupModelPrefab.transform);
            }

            // Check for model prefab modify callback
            if (_modifyItemModelPrefabCallback != null)
            {
                // Call modify model prefab callback
                _modifyItemModelPrefabCallback(itemDef.pickupModelPrefab);
            }

            // Check for item display settings and if config allows it
            if (_displaySettings != null && enableItemDisplays.Value)
            {
                // Check for display prefab modify callback
                if (_modifyItemDisplayPrefabCallback != null)
                {
                    // Call modify display prefab callback
                    _modifyItemDisplayPrefabCallback(_displaySettings.GetModel());
                }

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
                if (!enabled.Value)
                {
                    Log.Debug($"Hiding item '{_token}' due to user preference");
                }
                else if (_debugOnly)
                {
                    Log.Debug($"Hiding WIP item '{_token}'");
                }
                else
                {
                    Log.Debug($"Hiding item '{_token}' due to use of temporary assets outside of debug mode");
                }
            }
        }

        private void CreateDefaultSettings()
        {
            // Create the settings which every item should have
            enabled = CreateSetting("ENABLED", "Enable item?", true, "Should this item appear in runs?");
            enableItemDisplays = CreateSetting("ENABLE_ITEM_DISPLAYS", "Enable item displays?", true, "Should this item have item displays on the compatible character models?");
            extendedPickupDesc = CreateSetting("EXTENDED_PICKUP_DESC", "Extended Pickup Description", false, "Should this item have the logbook description appear when picking it up during runs?");
        }

        public Setting<T> CreateSetting<T>(string _tokenAddition, string _key, T _defaultValue, string _description)
        {
            // Return new setting
            return Config.CreateSetting($"ITEM_{token}_{_tokenAddition}", $"Item: {name.Replace("'", "")}", _key, _defaultValue, _description);
        }
    }
}
