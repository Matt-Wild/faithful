using RoR2;
using R2API;
using UnityEngine;
using System.Collections.Generic;

namespace Faithful
{
    internal delegate void ModifyPrefabCallback(GameObject _prefab);

    internal class Item
    {
        // Create default settings that all items have (CAN BE NULL)
        public Setting<bool> enabledSetting;
        public Setting<bool> extendedPickupDescSetting;
        public Setting<bool> enableItemDisplaysSetting;

        // Item def
        public ItemDef itemDef;

        // Item token and name
        public string token;
        public string name;

        // Is this item hidden
        public bool hidden = false;

        // Constructor
        public Item(string _token, ItemTag[] _tags, string _iconName, string _modelName, ItemTier _tier = ItemTier.Tier1, bool _simulacrumBanned = false, bool _canRemove = true, bool _hidden = false, string _corruptToken = null, ItemDisplaySettings _displaySettings = null, ModifyPrefabCallback _modifyItemModelPrefabCallback = null, ModifyPrefabCallback _modifyItemDisplayPrefabCallback = null, bool _debugOnly = false)
        {
            // Assign token
            token = _token;

            // Assign if hidden
            hidden = _hidden;

            // Assign name
            name = Utils.GetLanguageString($"FAITHFUL_{token}_NAME");

            // Don't create settings for hidden items
            if (!_hidden)
            {
                // Create default settings (MUST HAPPEN AFTER TOKEN AND NAME IS ASSIGNED)
                CreateDefaultSettings();
            }

            // Should hide this item due to temporary assets or debug only?
            bool forceHide = !Utils.debugMode && (_debugOnly || _iconName == "textemporalcubeicon" || !Assets.HasAsset(_iconName) || _modelName == "temporalcubemesh" || !Assets.HasAsset(_modelName));

            // Should hide anyway due to config?
            if (!forceHide)
            {
                forceHide = _hidden || (!enabledSetting.Value || Items.allItemsDisabled);
            }

            // Create item def
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            // Update item texts
            UpdateItemTexts();

            // Set item expansion
            itemDef.requiredExpansion = Utils.expansionDef;

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
            if (_displaySettings != null && (enableItemDisplaysSetting == null || enableItemDisplaysSetting.Value) && !Items.allItemDisplaysDisabled)
            {
                // Get display model
                GameObject displayModel = _displaySettings.GetModel();

                // Add item display model behaviour
                ItemDisplayModel displayModelBehaviour = displayModel.AddComponent<ItemDisplayModel>();
                
                // Initialise display model behaviour
                displayModelBehaviour.Init(_token, _displaySettings.displaySpecifiers);

                // Check for display prefab modify callback
                if (_modifyItemDisplayPrefabCallback != null)
                {
                    // Call modify display prefab callback
                    _modifyItemDisplayPrefabCallback(displayModel);
                }

                // Add item and pass in item display settings
                ItemAPI.Add(new CustomItem(itemDef, _displaySettings.GetRules()));
            }
            else
            {
                // Add item and pass in null Item Display Rules
                ItemAPI.Add(new CustomItem(itemDef, new ItemDisplayRuleDict(null)));
            }

            Log.Debug($"Created item '{name}'");

            if (forceHide)
            {
                if (_hidden)
                {
                    if (Utils.debugMode)
                    {
                        Log.Debug($"Hiding item '{name}'");
                    }
                }
                else if (!enabledSetting.Value || Items.allItemsDisabled)
                {
                    Log.Debug($"Hiding item '{name}' due to user preference");
                }
                else if (_debugOnly)
                {
                    Log.Debug($"Hiding WIP item '{name}'");
                }
                else
                {
                    Log.Debug($"Hiding item '{name}' due to use of temporary assets outside of debug mode");
                }
            }
        }

        public void UpdateItemTexts()
        {
            // Update item texts
            itemDef.name = Utils.GetXMLLanguageString($"FAITHFUL_{token}_NAME");
            itemDef.nameToken = $"FAITHFUL_{token}_NAME";
            itemDef.pickupToken = Config.FormatLanguageToken(Items.extendAllPickupDescriptions ? $"FAITHFUL_{token}_DESC" : extendedPickupDescSetting == null ? $"FAITHFUL_{token}_PICKUP" : extendedPickupDescSetting.Value ? $"FAITHFUL_{token}_DESC" : $"FAITHFUL_{token}_PICKUP", $"ITEM_{token}");
            itemDef.descriptionToken = Config.FormatLanguageToken($"FAITHFUL_{token}_DESC", $"ITEM_{token}");
            itemDef.loreToken = $"FAITHFUL_{token}_LORE";
        }

        private void CreateDefaultSettings()
        {
            // Create the settings which every item should have
            enabledSetting = CreateSetting("ENABLED", "Enable Item?", true, "Should this item appear in runs?", false, _restartRequired: true);
            enableItemDisplaysSetting = CreateSetting("ENABLE_ITEM_DISPLAYS", "Enable Item Displays?", true, "Should this item have item displays on the compatible character models?", false, true, _restartRequired: true);
            extendedPickupDescSetting = CreateSetting("EXTENDED_PICKUP_DESC", "Extended Pickup Description", false, "Should this item have the logbook description appear when picking it up during runs?", false, true);

            // Clean previous unused default settings
            Setting<bool> temp1 = CreateSetting("TEMP1", "Enable item?", true, "Should this item appear in runs?");
            Setting<bool> temp2 = CreateSetting("TEMP2", "Enable item displays?", true, "Should this item have item displays on the compatible character models?");
            temp1.Delete();
            temp2.Delete();
        }

        public Setting<T> CreateSetting<T>(string _tokenAddition, string _key, T _defaultValue, string _description, bool _isStat = true, bool _isClientSide = false, T _minValue = default, T _maxValue = default, T _randomiserMin = default, T _randomiserMax = default, bool _canRandomise = true, bool _restartRequired = false, string _valueFormatting = "{0:0}")
        {
            // Check if hidden
            if (hidden)
            {
                // Error and return
                Debug.LogError($"ATTEMPTED TO CREATE SETTING ON HIDDEN ITEM '{name}'!");
                return null;
            }

            // Return new setting
            return Config.CreateSetting($"ITEM_{token}_{_tokenAddition}", $"Item: {name.Replace("'", "")}", _key, _defaultValue, _description, _isStat, _isClientSide, _minValue, _maxValue, _randomiserMin, _randomiserMax, _canRandomise, _restartRequired, _valueFormatting);
        }

        public Setting<T> FetchSetting<T>(string _tokenAddition)
        {
            // Check if hidden
            if (hidden)
            {
                // Error and return
                Debug.LogError($"ATTEMPTED TO FETCH SETTING ON HIDDEN ITEM '{name}'!");
                return null;
            }

            // Fetch setting from config
            return Config.FetchSetting<T>($"ITEM_{token}_{_tokenAddition}");
        }
    }

    internal class ItemDisplayModel : MonoBehaviour
    {
        // Item this display model is related to
        [SerializeField] string relatedItemToken;

        // List of character body specifiers
        [SerializeField] List<ItemDisplaySpecifier> displaySpecifiers = new List<ItemDisplaySpecifier>();

        public void Init(string _relatedItemToken, List<ItemDisplaySpecifier> _displaySpecifiers = null)
        {
            // Assign related item token
            relatedItemToken = _relatedItemToken;

            // Check for character body specifiers
            if (_displaySpecifiers != null)
            {
                // Assign display specifiers
                displaySpecifiers = _displaySpecifiers;
            }
        }

        void Start()
        {
            // Skip if no character body specifiers provided
            if (displaySpecifiers.Count == 0) return;

            // Attempt to get character body
            CharacterBody characterBody = body;

            // Return if no body found
            if (characterBody == null) return;

            // Get body index for this character
            BodyIndex bodyIndex = BodyCatalog.FindBodyIndex(characterBody);


            // Cycle through display specifiers
            foreach (ItemDisplaySpecifier itemDisplaySpecifier in displaySpecifiers)
            {
                // Check if for this item display instance
                if (itemDisplaySpecifier.MatchesSpecifier(gameObject))
                {
                    // Check if body index is incorrect
                    if (bodyIndex != itemDisplaySpecifier.bodyIndex)
                    {
                        // Destroy this item display
                        DestroyImmediate(gameObject);

                        // Done
                        return;
                    }

                    // Done
                    break;
                }
            }

            // Get list of components with display model behaviour from parents
            List<IDisplayModelBehaviour> displayModelBehaviours = Utils.GetComponentsInParentsWithInterface<IDisplayModelBehaviour>(transform);

            // Cycle through display model behaviour
            foreach (IDisplayModelBehaviour displayModelBehaviour in displayModelBehaviours)
            {
                // Check if display model behaviour is for this display model's related item
                if (displayModelBehaviour.relatedItemToken == relatedItemToken)
                {
                    // Call on display model created behaviour
                    displayModelBehaviour.OnDisplayModelCreated();
                }
            }
        }

        public CharacterBody body
        {
            get
            {
                // Get parent
                Transform parent = transform.parent;

                // Cycle until no new parent found
                while (parent.parent != null)
                {
                    // Get new parent
                    parent = parent.parent;
                }

                // Attempt to get character body
                CharacterBody body = parent.GetComponent<CharacterBody>();

                // Check for character body
                if (body == null)
                {
                    // Attempt to get character model
                    CharacterModel model = parent.GetComponent<CharacterModel>();

                    // Check for character model
                    if (model != null)
                    {
                        // Fetch character body
                        body = model.body;
                    }
                }

                // Check for character body
                if (body != null)
                {
                    // Return character body
                    return body;
                }

                // No character body found
                return null;
            }
        }
    }

    internal interface IDisplayModelBehaviour
    {
        // Item this display model behaviour is related to
        public string relatedItemToken { get; }

        // Behaviour to run when item display model is created
        public void OnDisplayModelCreated();
    }
}
