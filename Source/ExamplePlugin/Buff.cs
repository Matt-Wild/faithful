using R2API;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class Buff
    {
        // Create default settings that all buffs have
        public Setting<bool> hiddenSetting;
        public Setting<bool> enableOverlaySetting;

        // Is this buff used specifically for Quality?
        public bool qualityBuff = false;

        // Buff def
        public BuffDef buffDef;

        // Buff overlay
        public Overlays.Overlay overlay;

        // Buff token and name
        public string token;
        public string langTokenOverride;
        public string name;
        public string safeName;

        // Constructor
        public Buff(string _token, string _safeName, string _iconName, Color _colour, bool _canStack = true, bool _isDebuff = false, bool _isHidden = false, bool _hasConfig = true, bool _usePercentageDisplay = false, Overlays.Overlay _overlay = null, string _langTokenOverride = null, bool _qualityBuff = false)
        {
            // Assign token
            token = _token;

            // Assign if quality buff
            qualityBuff = _qualityBuff;

            // Assign language token override
            langTokenOverride = _langTokenOverride;

            // Assign name
            name = Languages.GetLanguageString($"FAITHFUL_{langToken}_BUFF");
            safeName = _safeName;

            // Assign overlay
            overlay = _overlay;

            // Check if has config
            if (_hasConfig)
            {
                // Create default settings (MUST HAPPEN AFTER TOKEN AND NAME IS ASSIGNED)
                CreateDefaultSettings();
            }

            // Should hide this buff due to temporary assets?
            bool forceHide = !Utils.debugMode && (_iconName == "texbufftemporalcube" || !Assets.HasAsset(_iconName));

            // Should hide anyway due to config?
            if (!forceHide)
            {
                forceHide = hiddenSetting != null ? hiddenSetting.Value : false;
            }

            // Create buff def
            buffDef = ScriptableObject.CreateInstance<BuffDef>();

            // Set buff name
            buffDef.name = $"{safeName}_FAITHFUL_{token}_BUFF";

            // Set buff colour
            buffDef.buffColor = _colour;

            // Set buff configs
            buffDef.canStack = _canStack;
            buffDef.isDebuff = _isDebuff;

            // Set if buff is hidden (Also hide if using temporary assets when not in debug mode)
            buffDef.isHidden = _isHidden || forceHide;

            // Set icon
            buffDef.iconSprite = Assets.GetIcon(_iconName);

            // Set stacking display method
            buffDef.stackingDisplayMethod = _usePercentageDisplay ? BuffDef.StackingDisplayMethod.Percentage : BuffDef.StackingDisplayMethod.Default;

            // Add buff
            ContentAddition.AddBuffDef(buffDef);

            if (Utils.verboseConsole) Log.Debug($"Created buff '{_token}'");

            if (forceHide && Utils.verboseConsole)
            {
                if (_hasConfig && hiddenSetting.Value)
                {
                    Log.Debug($"Hiding buff '{_token}' due to user preference");
                }
                else
                {
                    Log.Debug($"Hiding buff '{_token}' due to use of temporary assets outside of debug mode");
                }
            }
        }

        private void CreateDefaultSettings()
        {
            // Create the settings which every buff should have
            hiddenSetting = CreateSetting("HIDDEN", "Hide Buff?", false, "Should the icon for this buff be hidden during runs?", _restartRequired: true);

            // Only create the overlay setting if an overlay was provided
            if (overlay != null)
            {
                enableOverlaySetting = CreateSetting("OVERLAY", "Enable Overlay?", true, "Should the character overlay for this buff be enabled?");
            }

            // Clean previous unused default settings
            Setting<bool> temp1 = CreateSetting("TEMP1", "Hide buff?", false, "Should the icon for this buff be hidden during runs?");
            temp1.Delete();
        }

        public Setting<T> CreateSetting<T>(string _tokenAddition, string _key, T _defaultValue, string _description, bool _restartRequired = false, string _valueFormatting = "{0:0}")
        {
            // Never create settings for this buff if it's a quality buff but quality isn't active
            if (qualityBuff && !Utils.qualityEnabled) return null;

            // Return new setting
            return Config.CreateSetting($"BUFF_{token}_{_tokenAddition}", qualityBuff ? $"Quality Buff: {safeName}" : $"Buff: {safeName}", _key, _defaultValue, _description, _restartRequired: _restartRequired, _valueFormatting: _valueFormatting);
        }

        public bool overlayEnabled => enableOverlaySetting != null && enableOverlaySetting.Value && overlay != null;
        public string langToken => string.IsNullOrEmpty(langTokenOverride) ? token : langTokenOverride;
    }
}
