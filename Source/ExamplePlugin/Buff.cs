using R2API;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class Buff
    {
        // Create default settings that all buffs have
        public Setting<bool> hiddenSetting;

        // Buff def
        public BuffDef buffDef;

        // Buff token and name
        public string token;
        public string name;

        // Constructor
        public Buff(string _token, string _iconName, Color _colour, bool _canStack = true, bool _isDebuff = false, bool _isHidden = false, bool _hasConfig = true)
        {
            // Assign token
            token = _token;

            // Assign name
            name = Utils.GetLanguageString($"FAITHFUL_{_token}_BUFF");

            // Check is has config
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
                forceHide = _hasConfig ? hiddenSetting.Value : false;
            }

            // Create buff def
            buffDef = ScriptableObject.CreateInstance<BuffDef>();

            // Set buff name
            buffDef.name = Utils.GetLanguageString($"FAITHFUL_{_token}_BUFF");

            // Set buff colour
            buffDef.buffColor = _colour;

            // Set buff configs
            buffDef.canStack = _canStack;
            buffDef.isDebuff = _isDebuff;

            // Set if buff is hidden (Also hide if using temporary assets when not in debug mode)
            buffDef.isHidden = _isHidden || forceHide;

            // Set icon
            buffDef.iconSprite = Assets.GetIcon(_iconName);

            // Add buff
            ContentAddition.AddBuffDef(buffDef);

            Log.Debug($"Created buff '{_token}'");

            if (forceHide)
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

            // Clean previous unused default settings
            Setting<bool> temp1 = CreateSetting("TEMP1", "Hide buff?", false, "Should the icon for this buff be hidden during runs?");
            temp1.Delete();
        }

        public Setting<T> CreateSetting<T>(string _tokenAddition, string _key, T _defaultValue, string _description, bool _restartRequired = false, string _valueFormatting = "{0:0}")
        {
            // Return new setting
            return Config.CreateSetting($"BUFF_{token}_{_tokenAddition}", $"Buff: {name.Replace("'", "")}", _key, _defaultValue, _description, _restartRequired: _restartRequired, _valueFormatting: _valueFormatting);
        }
    }
}
