using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Faithful
{
    internal delegate void ModifyPrefabCallback(GameObject _prefab);
    internal delegate Dictionary<string, string> ManualSettingTokensCallback();
    internal delegate Dictionary<string, string> QualityManualSettingTokensCallback(Quality _quality);

    internal class Item
    {
        // Create default settings that all items have (CAN BE NULL)
        public Setting<bool> enabledSetting;
        public Setting<bool> extendedPickupDescSetting;
        public Setting<bool> enableItemDisplaysSetting;
        public Setting<string> nameOverrideSetting;
        public Setting<string> pickupOverrideSetting;
        public Setting<string> descriptionOverrideSetting;
        public Setting<string> corruptedOverrideSetting;

        // Language overlays
        LanguageAPI.LanguageOverlay nameOverlay;

        // Item def
        public ItemDef itemDef;

        // Item token, name and tier
        public string token;
        public string name;
        public string safeName;
        public ItemTier tier;

        // Is this item hidden
        public bool hidden = false;

        // Is this item hidden from the logbook
        public bool hiddenFromLogbook = false;

        // Is this item WIP
        public bool WIP = false;

        // Corrupted item token for default corrupted item and proper name for corrupted item
        private string defaultCorruptedToken;
        public string corruptedName = "";

        // Override tokens
        private string overrideName = string.Empty;
        private string overridePickup = string.Empty;
        private string overrideDescription = string.Empty;
        private string overrideLore = string.Empty;

        // Used for logbook ordering
        private string namePrefix = string.Empty;

        // Used for language token variants
        public string descriptionVariant = string.Empty;

        // Has support for Quality
        public bool supportsQuality = false;

        // Callbacks for manual setting tokens that get injected into language strings
        public ManualSettingTokensCallback descriptionManualSettings;
        public QualityManualSettingTokensCallback qualityDescriptionManualSettings;

        // Constructor
        public Item(string _token, string _safeName, ItemTag[] _tags, string _iconName, string _modelName, ItemTier _tier = ItemTier.Tier1, bool _simulacrumBanned = false, bool _canRemove = true, bool _hidden = false, string _corruptToken = null, ItemDisplaySettings _displaySettings = null, ModifyPrefabCallback _modifyItemModelPrefabCallback = null, ModifyPrefabCallback _modifyItemDisplayPrefabCallback = null, bool _canNeverBeTemporary = false, bool _debugOnly = false, bool _WIP = false, string _overrideName = null, string _overridePickup = null, string _overrideDescription = null, string _overrideLore = null, string _namePrefix = null, bool _hiddenFromLogbook = false, bool _supportsQuality = false)
        {
            // Assign token
            token = _token;

            // Assign if hidden
            hidden = _hidden;

            // Assign if hidden from logbook
            hiddenFromLogbook = _hiddenFromLogbook;

            // Assign if WIP
            WIP = _WIP;

            // Assign if supports quality
            supportsQuality = _supportsQuality;

            // Assign name prefix
            namePrefix = _namePrefix ?? "";

            // Assign name
            name = WIP ? _safeName : Languages.GetLanguageString($"FAITHFUL_{token}_NAME");
            safeName = _safeName;

            // Assign tier
            tier = _tier;

            // Assign default corrupted token
            defaultCorruptedToken = _corruptToken;

            // Don't create settings for hidden or WIP items
            if (!_hidden && !_WIP)
            {
                // Create default settings (MUST HAPPEN AFTER TOKEN AND NAME IS ASSIGNED)
                CreateDefaultSettings();
            }

            // Should hide this item due to temporary assets or debug only?
            bool forceHide = !Utils.debugMode && (_debugOnly || _iconName == "textemporalcubeicon" || !Assets.HasAsset(_iconName) || _modelName == "temporalcubemesh" || !Assets.HasAsset(_modelName));

            // Should hide anyway due to config?
            if (!forceHide)
            {
                forceHide = _hidden || (!isEnabled || Items.allItemsDisabled);
            }

            // Update tier
            _tier = forceHide ? ItemTier.NoTier : _tier;

            // Create item def
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            // Fetch item settings
            FetchSettings();

            // Assign override tokens
            overrideName = _overrideName;
            overridePickup = _overridePickup;
            overrideDescription = _overrideDescription;
            overrideLore = _overrideLore;

            // Update item texts
            UpdateItemTexts();

            // Set item expansion
            itemDef.requiredExpansion = Utils.expansionDef;

            // Set item tier (force untiered if forced to be hidden)
            itemDef.deprecatedTier = forceHide ? ItemTier.NoTier : _tier;

            // Automatically review temporary item tag
            if ((_tier == ItemTier.Tier1 || _tier == ItemTier.Tier2 || _tier == ItemTier.Tier3 || _tier == ItemTier.Boss || _tier == ItemTier.VoidTier1 || _tier == ItemTier.VoidTier2 || _tier == ItemTier.VoidTier3 || _tier == ItemTier.VoidBoss) && !_canNeverBeTemporary)
            {
                // Check for tag
                if (!_tags.Contains(ItemTag.CanBeTemporary))
                {
                    // Add tag
                    _tags = [.. _tags, ItemTag.CanBeTemporary];
                }
            }
            else
            {
                // Check for tag
                if (_tags.Contains(ItemTag.CanBeTemporary))
                {
                    // Remove tag
                    _tags = [.. _tags.Where(tag => tag != ItemTag.CanBeTemporary)];
                }
            }

            // Set item tags
            itemDef.tags = _tags;

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

            // Corrupts item? - Avoid pairing disabled items
            if (_corruptToken != null && isEnabled)
            {
                if (Utils.verboseConsole)
                {
                    if (corruptedOverrideSetting == null) Log.Debug($"Adding corruption pair for item '{name}'. Item to corrupt token: '{_corruptToken}'");
                    else Log.Debug($"Adding corruption pair for item '{name}'. Item to corrupt token: '{_corruptToken}' with override '{corruptedOverrideSetting.Value}'");
                }

                // Add corruption pair
                Utils.AddCorruptionPair(itemDef, _corruptToken, corruptedOverrideSetting != null ? corruptedOverrideSetting.Value : "");
            }

            // Set icon and model
            itemDef.pickupIconSprite = Assets.GetIcon(_iconName);
            GameObject modelPrefab = Assets.GetModel(_modelName);
            Utils.ProcessRendererRules(modelPrefab);
            itemDef.pickupModelPrefab = modelPrefab;

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

            if (Utils.verboseConsole) Log.Debug($"Created item '{name}'");

            if (forceHide && Utils.verboseConsole)
            {
                if (_hidden)
                {
                    Log.Debug($"Hiding item '{name}'");
                }
                else if (!isEnabled || Items.allItemsDisabled)
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
            // Check for name override setting
            if (nameOverrideSetting != null && !string.IsNullOrWhiteSpace(nameOverrideSetting.Value))
            {
                // Set name overlay
                nameOverlay = LanguageAPI.AddOverlay($"FAITHFUL_ITEM_{token}_NAME", nameOverrideSetting.Value);
            }

            // No name override setting (still do WIP override etc)
            else
            {
                // Set name overlay
                nameOverlay = LanguageAPI.AddOverlay($"FAITHFUL_ITEM_{token}_NAME", string.IsNullOrEmpty(overrideName) ? Languages.GetLanguageString($"FAITHFUL_{token}_NAME") : overrideName);
            }

            // Check for pickup override
            if (pickupOverrideSetting != null && !string.IsNullOrWhiteSpace(pickupOverrideSetting.Value))
            {
                overridePickup = pickupOverrideSetting.Value;
            }

            // Check for description override
            if (descriptionOverrideSetting != null && !string.IsNullOrWhiteSpace(descriptionOverrideSetting.Value))
            {
                overrideDescription = descriptionOverrideSetting.Value;
            }

            // Check if void item
            if (isVoid)
            {
                // Update corrupted name just in case
                UpdateCorruptedName();
            }

            // Create overlay tokens
            if (string.IsNullOrWhiteSpace(overridePickup))
            {
                // Use 3 parameter version to add language override for specific language
                LanguageAPI.AddOverlay($"FAITHFUL_ITEM_{token}_PICKUP", Config.FormatLanguageToken($"FAITHFUL_{token}_PICKUP", $"ITEM_{token}", corruptedNameSafe));
            }
            else
            {
                // Use 3 parameter version to add language override for specific language
                LanguageAPI.AddOverlay($"FAITHFUL_ITEM_{token}_PICKUP", Config.FormatLanguageToken(overridePickup, $"ITEM_{token}", corruptedNameSafe, true));
            }
            if (string.IsNullOrWhiteSpace(overrideDescription))
            {
                // Use 3 parameter version to add language override for specific language
                LanguageAPI.AddOverlay($"FAITHFUL_ITEM_{token}_DESC", Config.FormatLanguageToken(descriptionSourceToken, $"ITEM_{token}", corruptedNameSafe, _manualSettingTokens: GetManualDescriptionSettings()));
            }
            else
            {
                // Use 3 parameter version to add language override for specific language
                LanguageAPI.AddOverlay($"FAITHFUL_ITEM_{token}_DESC", Config.FormatLanguageToken(overrideDescription, $"ITEM_{token}", corruptedNameSafe, true, _manualSettingTokens: GetManualDescriptionSettings()));
            }

            // Update lore token
            if (!string.IsNullOrWhiteSpace(overrideLore))
            {
                // Use 3 parameter version to add language override for specific language
                LanguageAPI.AddOverlay($"FAITHFUL_ITEM_{token}_LORE", overrideLore);
            }
            else
            {
                // Use 3 parameter version to add language override for specific language
                LanguageAPI.AddOverlay($"FAITHFUL_ITEM_{token}_LORE", Languages.GetLanguageString($"FAITHFUL_{token}_LORE"));
            }

            // Update item texts
            itemDef.name = $"{Utils.GetXMLSafeString(string.IsNullOrWhiteSpace(namePrefix) ? safeName : $"{namePrefix} FROM {safeName}")}_FAITHFUL_{token}_ITEM".ToUpper();
            itemDef.nameToken = string.IsNullOrEmpty(overrideName) ? $"FAITHFUL_ITEM_{token}_NAME" : overrideName;
            itemDef.pickupToken = extendPickupDescription ? $"FAITHFUL_ITEM_{token}_DESC" : $"FAITHFUL_ITEM_{token}_PICKUP";
            itemDef.descriptionToken = $"FAITHFUL_ITEM_{token}_DESC";
            itemDef.loreToken = $"FAITHFUL_ITEM_{token}_LORE";

            // If this item supports quality and quality is enabled then update quality item texts as well
            if (supportsQuality && Utils.qualityEnabled) UpdateQualityItemTexts();
        }

        private void UpdateQualityItemTexts()
        {
            // Get prefixes for tokens
            string pickupPrefix = Config.FormatLanguageToken(pickupUnprocessed, $"ITEM_{token}", _warn: false);
            string descriptionPrefix = Config.FormatLanguageToken(descriptionUnprocessed, $"ITEM_{token}", _warn: false, _manualSettingTokens: GetManualDescriptionSettings());

            // Create overlays for quality item tokens for each quality
            foreach (Quality quality in Enum.GetValues(typeof(Quality)))
            {
                // Get string version of quality
                string qualityString = quality.ToString();

                // Create description token first
                string qualityDescription = Config.FormatLanguageToken(string.IsNullOrWhiteSpace(descriptionVariant) ? $"FAITHFUL_{token}_DESC_QUALITY" : $"FAITHFUL_{token}_{descriptionVariant}_DESC_QUALITY", $"ITEM_{token}", corruptedNameSafe, _quality: qualityString, _manualSettingTokens: GetQualityManualDescriptionSettings(quality)).Replace("[PREFIX]", descriptionPrefix);
                LanguageAPI.AddOverlay($"ITEM_{itemDef.name}_{qualityString}_DESC", qualityDescription);

                // Check if extended pickup description is enabled
                if (extendPickupDescription) LanguageAPI.AddOverlay($"ITEM_{itemDef.name}_{qualityString}_PICKUP", qualityDescription);
                else LanguageAPI.AddOverlay($"ITEM_{itemDef.name}_{qualityString}_PICKUP", Config.FormatLanguageToken($"FAITHFUL_{token}_PICKUP_QUALITY", $"ITEM_{token}", corruptedNameSafe, _quality: qualityString).Replace("[PREFIX]", pickupPrefix));
            }
        }

        public void FetchSettings()
        {
            // Update corrupted name
            UpdateCorruptedName();
        }

        private void UpdateCorruptedName()
        {
            // Check if void item
            if (isVoid)
            {
                // Check for corrupted item override
                if (corruptedOverrideSetting != null)
                {
                    // Try and find item
                    ItemDef corruptedItem = Utils.GetItem(corruptedOverrideSetting.Value);
                    if (corruptedItem != null && !corruptedItem.hidden && !string.IsNullOrWhiteSpace(corruptedItem.nameToken))
                    {
                        // Set corrupted item name
                        corruptedName = Language.GetString(corruptedItem.nameToken);

                        // Check if corrupted name was found
                        if (corruptedName != corruptedItem.nameToken)
                        {
                            // Done
                            return;
                        }
                    }
                }

                // Corrupted override not found or not provided

                // Check for default corrupted token
                if (defaultCorruptedToken == null) return;

                // Set corrupted item name as default corrupted item proper name
                corruptedName = Language.GetString(defaultCorruptedToken);
            }
        }

        private void CreateDefaultSettings()
        {
            // Create the settings which every item should have
            enabledSetting = CreateSetting("ENABLED", "Enable Item?", true, "Should this item appear in runs?", false, _restartRequired: true);
            enableItemDisplaysSetting = CreateSetting("ENABLE_ITEM_DISPLAYS", "Enable Item Displays?", true, "Should this item have item displays on the compatible character models?", false, true, _restartRequired: true);
            extendedPickupDescSetting = CreateSetting("EXTENDED_PICKUP_DESC", "Extended Pickup Description", false, "Should this item have the logbook description appear when picking it up during runs?", false, true);
            nameOverrideSetting = CreateSetting("NAME_OVERRIDE", "Override Item Name", "", "Should this item be called something different?", false, _canRandomise: false);
            pickupOverrideSetting = CreateSetting("PICKUP_OVERRIDE", "Override Item Pickup", "", "Should this item have a different pickup?\n\nFor styling and dynamic stat tags, open the .language file for examples.", false, _canRandomise: false);
            descriptionOverrideSetting = CreateSetting("DESCRIPTION_OVERRIDE", "Override Item Description", "", "Should this item have a different description?\n\nFor styling and dynamic stat tags, open the .language file for examples.", false, _canRandomise: false);

            // Check if void item and normally has a corrupted item
            if (isVoid && !string.IsNullOrWhiteSpace(defaultCorruptedToken))
            {
                // Create corrupt override setting
                corruptedOverrideSetting = CreateSetting("CORRUPTED_OVERRIDE", "Override Corrupted Item", "", "Should this item corrupt something else instead of it's default item?", false, _canRandomise: false, _restartRequired: true);
            }
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

            if (Utils.verboseConsole) Log.Info($"[ITEM] | Creating setting '{_key}' for '{safeName} ({token})'");

            // Return new setting
            return Config.CreateSetting($"ITEM_{token}_{_tokenAddition}", $"Item: {safeName}", _key, _defaultValue, _description, _isStat, _isClientSide, _minValue, _maxValue, _randomiserMin, _randomiserMax, _canRandomise, _restartRequired, _valueFormatting);
        }

        public QualitySetting<T> CreateQualitySetting<T>(string _tokenAddition, string _key, T _uncommonDefaultValue, T _rareDefaultValue, T _epicDefaultValue, T _legendaryDefaultValue, string _description, bool _isStat = true, bool _isClientSide = false, T _minValue = default, T _maxValue = default, T _randomiserMin = default, T _randomiserMax = default, bool _canRandomise = true, bool _restartRequired = false, string _valueFormatting = "{0:0}")
        {
            // Check if hidden
            if (hidden)
            {
                // Error and return
                Debug.LogError($"ATTEMPTED TO CREATE QUALITY SETTING ON HIDDEN ITEM '{name}'!");
                return null;
            }

            if (Utils.verboseConsole) Log.Info($"[ITEM] | Creating Quality setting '{_key}' for '{safeName} ({token})'");

            // Return new setting
            return QualityConfig.CreateSetting($"ITEM_{token}_{_tokenAddition}", $"Item: {safeName}", _key, _uncommonDefaultValue, _rareDefaultValue, _epicDefaultValue, _legendaryDefaultValue, _description, _isStat, _isClientSide, _minValue, _maxValue, _randomiserMin, _randomiserMax, _canRandomise, _restartRequired, _valueFormatting);
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

        public QualitySetting<T> FetchQualitySetting<T>(string _tokenAddition)
        {
            // Check if hidden
            if (hidden)
            {
                // Error and return
                Debug.LogError($"ATTEMPTED TO FETCH QUALITY SETTING ON HIDDEN ITEM '{name}'!");
                return null;
            }

            // Fetch quality setting from config
            return QualityConfig.FetchSetting<T>($"ITEM_{token}_{_tokenAddition}");
        }

        private Dictionary<string, string> GetManualDescriptionSettings()
        {
            // Check for manual description setting tokens callback
            if (descriptionManualSettings != null)
            {
                // Return result of callback
                return descriptionManualSettings();
            }

            // No manual description setting tokens
            return null;
        }

        private Dictionary<string, string> GetQualityManualDescriptionSettings(Quality _quality)
        {
            // Check for quality manual description setting tokens callback
            if (qualityDescriptionManualSettings != null)
            {
                // Return result of callback
                return qualityDescriptionManualSettings(_quality);
            }

            // No quality manual description setting tokens
            return null;
        }

        public string corruptedNameSafe
        {
            get
            {
                // Check for corrupted name
                if (!string.IsNullOrWhiteSpace(corruptedName))
                {
                    // Return corrupted name
                    return corruptedName;
                }

                // Check for default corrupted name
                if (!string.IsNullOrWhiteSpace(defaultCorruptedToken))
                {
                    // Get language string
                    string languageString = Language.GetString(defaultCorruptedToken);

                    // Check if language string is valid
                    if (languageString != defaultCorruptedToken)
                    {
                        // Return language string
                        return languageString;
                    }
                }

                // Return empty string as fallback
                return "";
            }
        }

        public bool isVoid
        {
            get
            {
                // Check if void item tier
                if (tier == ItemTier.VoidTier1 || tier == ItemTier.VoidTier2 || tier == ItemTier.VoidTier3 || tier == ItemTier.VoidBoss) return true;

                // Not void item tier
                return false;
            }
        }

        public bool isEnabled => WIP ? Utils.WIPContentEnabled : enabledSetting.Value;
        string pickupUnprocessed => string.IsNullOrWhiteSpace(overridePickup) ? $"FAITHFUL_{token}_PICKUP" : overridePickup;
        string descriptionUnprocessed => string.IsNullOrWhiteSpace(overrideDescription) ? descriptionSourceToken : overrideDescription;
        string descriptionSourceToken => string.IsNullOrWhiteSpace(descriptionVariant) ? $"FAITHFUL_{token}_DESC" : $"FAITHFUL_{token}_{descriptionVariant}_DESC";
        bool extendPickupDescription => Items.extendAllPickupDescriptions || (!(extendedPickupDescSetting == null) && extendedPickupDescSetting.Value);
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
